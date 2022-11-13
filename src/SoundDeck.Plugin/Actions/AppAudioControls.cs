namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Nito.AsyncEx;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Events.Sent.Feedback;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides application specific audio controls for a dial / touchscreen.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.appaudiocontrols")]
    public class AppAudioControls : AppActionBase<AppAudioControlSettings>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private backing field for <see cref="Session"/>.
        /// </summary>
        private SessionWatcher _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAudioControls"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService"></param>
        public AppAudioControls(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService, appAudioService)
        {
        }

        /// <summary>
        /// Gets or sets the current session.
        /// </summary>
        private SessionWatcher Session
        {
            get => this._session;
            set
            {
                if (this._session != null)
                {
                    this._session.MediaSessionChanged -= this.OnMediaSessionChanged;
                    this._session.ThumbnailChanged -= this.OnThumbnailChanged;
                    this._session.TimelineChanged -= this.OnTimelineChanged;
                }

                this._session = value;
                if (value != null)
                {
                    this._session.MediaSessionChanged += this.OnMediaSessionChanged;
                    this._session.ThumbnailChanged += this.OnThumbnailChanged;
                    this._session.TimelineChanged += this.OnTimelineChanged;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            using (this._syncRoot.Lock())
            {
                this.Session?.Dispose();
                this.Session = null;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, AppAudioControlSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            using (this._syncRoot.Lock())
            {
                if (this.Session != null)
                {
                    this.Session.Predicate = settings.ToPredicate();
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
        {
            await base.OnDialPress(args);
            if (args.Payload.Pressed)
            {
                await (this.Session?.MediaSession?.TryTogglePlayPauseAsync()?.AsTask() ?? Task.CompletedTask);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);
            if (args.Payload.Ticks < 0)
            {
                await (this.Session?.MediaSession?.TrySkipPreviousAsync()?.AsTask() ?? Task.CompletedTask);
            }
            else
            {
                await (this.Session?.MediaSession?.TrySkipNextAsync()?.AsTask() ?? Task.CompletedTask);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            using (await this._syncRoot.LockAsync())
            {
                if (this.Session == null)
                {
                    var manager = await this.AppAudioService.GetMultimediaSessionManagerAsync();
                    this.Session = new SessionWatcher(manager, args.Payload.GetSettings<AppAudioControlSettings>().ToPredicate());
                }
                else
                {
                    this.Session.EnableRaisingEvents = true;
                    await this.SetFeedbackAsync(new VolumeFeedback { Icon = this.Session.ThumbnailAsBase64 });
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            using (await this._syncRoot.LockAsync())
            {
                if (this.Session != null)
                {
                    this.Session.EnableRaisingEvents = false;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="SessionWatcher.MediaSessionChanged"/> event, updating the image associated with this action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMediaSessionChanged(object sender, EventArgs e)
        {
            var feedback = new VolumeFeedback
            {
                Indicator = new VolumeIndicator
                {
                    IsEnabled = this.Session.MediaSession != null,
                    Value = 0
                },
                Value = string.Empty
            };

            this.SetFeedbackAsync(feedback).Forget(this.Logger);
        }

        /// <summary>
        /// Handles the <see cref="SessionWatcher.ThumbnailChanged"/> event, updating the image associated with this action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnThumbnailChanged(object sender, EventArgs e)
        {
            var feedback = new VolumeFeedback
            {
                Icon = this.Session.ThumbnailAsBase64
            };

            this.SetFeedbackAsync(feedback).Forget(this.Logger);
        }

        /// <summary>
        /// Handles the <see cref="SessionWatcher.TimelineChanged"/> event, providing feedback to the Stream Deck.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TimelineEventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
        {
            if (e.EndTime != TimeSpan.Zero)
            {
                this.SetFeedbackAsync(new VolumeFeedback()
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = (int)Math.Ceiling(100 / e.EndTime.TotalSeconds * e.Position.TotalSeconds)
                    },
                    Value = e.EndTime.Subtract(e.Position).ToString("mm':'ss")
                });
            }
        }
    }
}
