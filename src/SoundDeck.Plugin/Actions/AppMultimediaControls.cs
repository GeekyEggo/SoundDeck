namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Nito.AsyncEx;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Events.Sent.Feedback;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action that is capable of controlling media for a specific app.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.appmultimediacontrols")]
    public class AppMultimediaControls : AppActionBase<AppMultimediaControlsSettings>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private backing field for <see cref="MediaSessionWatcher"/>.
        /// </summary>
        private MediaSessionWatcher _mediaSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppMultimediaControls" /> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public AppMultimediaControls(IAudioService audioService, IAppAudioService appAudioService)
           : base(audioService, appAudioService)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="MediaSessionWatcher"/>, responsible for tracking the media session associated with the action.
        /// </summary>
        private MediaSessionWatcher SessionWatcher
        {
            get => this._mediaSession;
            set
            {
                if (this._mediaSession is not null)
                {
                    this._mediaSession.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
                    this._mediaSession.ProcessIconChanged -= this.OnProcessIconChanged;
                    this._mediaSession.SessionChanged -= this.OnSessionChanged;
                    this._mediaSession.TimelineChanged -= this.OnTimelineChanged;
                }

                this._mediaSession = value;
                if (this._mediaSession is not null)
                {
                    this._mediaSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                    this._mediaSession.ProcessIconChanged += this.OnProcessIconChanged;
                    this._mediaSession.SessionChanged += this.OnSessionChanged;
                    this._mediaSession.TimelineChanged += this.OnTimelineChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets friendly name.
        /// </summary>
        private string FriendlyName { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            using (this._syncRoot.Lock())
            {
                this.SessionWatcher?.Dispose();
                this.SessionWatcher = null;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, AppMultimediaControlsSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            using (this._syncRoot.Lock())
            {
                if (this.SessionWatcher is null)
                {
                    return;
                }

                this.SessionWatcher.Predicate = settings.ToPredicate();
                this.FriendlyName = settings.ProcessLabel;
            }

            await this.RefreshFeedbackAsync(updateIcon: true);
        }

        /// <inheritdoc/>
        protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
        {
            await base.OnDialPress(args);
            if (args.Payload.Pressed)
            {
                await (this.SessionWatcher?.Session?.TryTogglePlayPauseAsync()?.AsTask() ?? Task.CompletedTask);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);
            if (args.Payload.Ticks < 0)
            {
                await (this.SessionWatcher?.Session?.TrySkipPreviousAsync()?.AsTask() ?? Task.CompletedTask);
            }
            else
            {
                await (this.SessionWatcher?.Session?.TrySkipNextAsync()?.AsTask() ?? Task.CompletedTask);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTouchTap(ActionEventArgs<TouchTapPayload> args)
        {
            await base.OnTouchTap(args);
            await (this.SessionWatcher?.Session?.TryTogglePlayPauseAsync()?.AsTask() ?? Task.CompletedTask);
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            if (args.Payload.Controller is Controller.Encoder)
            {
                using (await this._syncRoot.LockAsync())
                {
                    var settings = args.Payload.GetSettings<AppMultimediaControlsSettings>();
                    this.FriendlyName = settings.ProcessLabel;

                    if (this.SessionWatcher is null)
                    {
                        var manager = await this.AppAudioService.GetMultimediaSessionManagerAsync();
                        this.SessionWatcher = new MediaSessionWatcher(manager, settings.ToPredicate());
                    }
                    else
                    {
                        this.SessionWatcher.EnableRaisingEvents = true;
                    }
                }

                await this.RefreshFeedbackAsync(updateIcon: true);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            using (await this._syncRoot.LockAsync())
            {
                if (this.SessionWatcher != null)
                {
                    this.SessionWatcher.EnableRaisingEvents = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<AppMultimediaControlsSettings>();

            try
            {
                await this.AppAudioService.ControlAsync(settings, settings.Action);
                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to control application's multimedia; Action=\"{settings.Action}\", ProcessSelectionType=\"{settings.ProcessSelectionType}\", ProcessName=\"{settings.ProcessName}\".");
                await this.ShowAlertAsync();
            }
        }

        /// <summary>
        /// Occurs when <see cref="MediaSessionWatcher.MediaPropertiesChanged"/> occurs, and updates the feedback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMediaPropertiesChanged(object sender, EventArgs e)
            => this.RefreshFeedbackAsync(updateIcon: true).Forget(this.Logger);

        /// <summary>
        /// Called when <see cref="SessionWatcher{T}.ProcessIconChanged"/> occurs, and updates the image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="image">The image.</param>
        private void OnProcessIconChanged(object sender, string image)
        {
            this.SetImageAsync(image).Forget(this.Logger);
            this.RefreshFeedbackAsync(updateIcon: true).Forget(this.Logger);
        }

        /// <summary>
        /// Occurs when <see cref="SessionWatcher{T}.SessionChanged"/> occurs, and updates the feedback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="session">The new session.</param>
        private void OnSessionChanged(object sender, global::Windows.Media.Control.GlobalSystemMediaTransportControlsSession session)
            => this.RefreshFeedbackAsync(updateIcon: true).Forget(this.Logger);

        /// <summary>
        /// Occurs when <see cref="MediaSessionWatcher.TimelineChanged"/> occurs, and updates the feedback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
            => this.RefreshFeedbackAsync(updateIcon: false).Forget(this.Logger);

        /// <summary>
        /// Refreshes the feedback provided to the user asynchronously.
        /// </summary>
        /// <param name="updateIcon">When <c>true</c>, the icon is updated based on the <see cref="SessionWatcher"/>.</param>
        private async Task RefreshFeedbackAsync(bool updateIcon)
        {
            using (await this._syncRoot.LockAsync())
            {
                // Update the feedback.
                var hasTimeline = this.SessionWatcher?.TrackEndTime is TimeSpan and { TotalSeconds: > 0 };
                var feedback = new VolumeFeedback()
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = hasTimeline ? (int)Math.Ceiling(100 / this.SessionWatcher.TrackEndTime.TotalSeconds * this.SessionWatcher.TrackPosition.TotalSeconds) : 0
                    },
                    Title = this.FriendlyName,
                    Icon = updateIcon ? this.SessionWatcher?.Thumbnail ?? this.SessionWatcher?.ProcessIcon : null,
                    Value = hasTimeline ? this.SessionWatcher.TrackEndTime.Subtract(this.SessionWatcher.TrackPosition).ToString("mm':'ss") : this.SessionWatcher?.Title
                };

                await this.SetFeedbackAsync(feedback);

                // Update the state image.
                if (updateIcon
                    && this.SessionWatcher?.ProcessIcon is string processIcon)
                {
                    this.SetImageAsync(processIcon).Forget(this.Logger);
                }
            }
        }
    }
}
