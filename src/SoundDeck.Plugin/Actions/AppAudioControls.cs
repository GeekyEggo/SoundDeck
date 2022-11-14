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
    using SoundDeck.Core.Volume;
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
        /// Private backing field for <see cref="AppAudioSession"/>.
        /// </summary>
        private AppAudioSession _appAudioSession;

        /// <summary>
        /// Private backing field for <see cref="Settings"/>.
        /// </summary>
        private AppAudioControlSettings _settings;

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
        private AppAudioSession AppAudioSession
        {
            get => this._appAudioSession;
            set
            {
                if (this._appAudioSession != null)
                {
                    this._appAudioSession.MediaSessionChanged -= this.OnMediaSessionChanged;
                    this._appAudioSession.ThumbnailChanged -= this.OnThumbnailChanged;
                    this._appAudioSession.TimelineChanged -= this.OnTimelineChanged;
                    this._appAudioSession.VolumeChanged -= this.OnVolumeChanged;
                }

                this._appAudioSession = value;
                if (value != null)
                {
                    this._appAudioSession.MediaSessionChanged += this.OnMediaSessionChanged;
                    this._appAudioSession.ThumbnailChanged += this.OnThumbnailChanged;
                    this._appAudioSession.TimelineChanged += this.OnTimelineChanged;
                    this._appAudioSession.VolumeChanged += this.OnVolumeChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current feedback.
        /// </summary>
        private AudioControlAction? CurrentFeedback { get; set; } = null;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        private AppAudioControlSettings Settings
        {
            get => this._settings;
            set
            {
                if (value.ActionLayout == ActionLayout.Custom)
                {
                    this.CurrentFeedback = value.RotateAction;
                }
                else if (this.CurrentFeedback == null)
                {
                    this.CurrentFeedback = AudioControlAction.Track;
                }

                this._settings = value;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            using (this._syncRoot.Lock())
            {
                this.AppAudioSession?.Dispose();
                this.AppAudioSession = null;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, AppAudioControlSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            using (this._syncRoot.Lock())
            {
                if (this.AppAudioSession != null)
                {
                    this.AppAudioSession.Predicate = settings.ToPredicate();
                    this.Settings = settings;
                    await this.RefreshCurrentFeedbackAsync(setIcon: false);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
        {
            await base.OnDialPress(args);
            if (args.Payload.Pressed)
            {
                if ((this.Settings.ActionLayout == ActionLayout.PlaybackAndVolume && this.CurrentFeedback == AudioControlAction.Volume)
                    || this.Settings.PressAction == AudioControlAction.Volume)
                {
                    this.AppAudioSession?.Audio?.SimpleAudioVolume?.Set(new VolumeSettings(VolumeAction.ToggleMute));
                }
                else
                {
                    await (this.AppAudioSession?.Media?.TryTogglePlayPauseAsync()?.AsTask() ?? Task.CompletedTask);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);

            if (args.Payload.Ticks < 0)
            {
                if (this.CurrentFeedback == AudioControlAction.Volume)
                {
                    this.AppAudioSession.Audio?.SimpleAudioVolume?.Set(new VolumeSettings(VolumeAction.DecreaseBy, this.Settings.VolumeValue));
                }
                else
                {
                    await (this.AppAudioSession?.Media?.TrySkipPreviousAsync()?.AsTask() ?? Task.CompletedTask);
                }
            }
            else
            {
                if (this.CurrentFeedback == AudioControlAction.Volume)
                {
                    this.AppAudioSession.Audio?.SimpleAudioVolume?.Set(new VolumeSettings(VolumeAction.IncreaseBy, this.Settings.VolumeValue));
                }
                else
                {
                    await (this.AppAudioSession?.Media?.TrySkipNextAsync()?.AsTask() ?? Task.CompletedTask);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTouchTap(ActionEventArgs<TouchTapPayload> args)
        {
            await base.OnTouchTap(args);
            if (this.Settings.ActionLayout == ActionLayout.PlaybackAndVolume)
            {
                this.CurrentFeedback = this.CurrentFeedback == AudioControlAction.Track
                    ? AudioControlAction.Volume
                    : AudioControlAction.Track;

                await this.RefreshCurrentFeedbackAsync(setIcon: false);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            using (await this._syncRoot.LockAsync())
            {
                this.Settings = args.Payload.GetSettings<AppAudioControlSettings>();

                if (this.AppAudioSession == null)
                {
                    var manager = await this.AppAudioService.GetMultimediaSessionManagerAsync();
                    this.AppAudioSession = new AppAudioSession(manager, this.Settings.ToPredicate());
                }
                else
                {
                    this.AppAudioSession.EnableRaisingEvents = true;
                    await this.RefreshCurrentFeedbackAsync(setIcon: true);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            using (await this._syncRoot.LockAsync())
            {
                if (this.AppAudioSession != null)
                {
                    this.AppAudioSession.EnableRaisingEvents = false;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="AppAudioSession.MediaSessionChanged"/> event, updating the image associated with this action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMediaSessionChanged(object sender, EventArgs e)
        {
            if (this.CurrentFeedback == AudioControlAction.Track)
            {
                var feedback = new VolumeFeedback
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = this.AppAudioSession.Media != null,
                        Value = 0
                    },
                    Value = string.Empty
                };

                this.SetFeedbackAsync(feedback).Forget(this.Logger);
            }
        }

        /// <summary>
        /// Handles the <see cref="AppAudioSession.ThumbnailChanged"/> event, updating the image associated with this action.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnThumbnailChanged(object sender, EventArgs e)
        {
            var feedback = new VolumeFeedback
            {
                Icon = this.AppAudioSession.ThumbnailAsBase64
            };

            this.SetFeedbackAsync(feedback).Forget(this.Logger);
        }

        /// <summary>
        /// Handles the <see cref="AppAudioSession.TimelineChanged"/> event, providing feedback to the Stream Deck.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TimelineEventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
        {
            if (this.CurrentFeedback == AudioControlAction.Track)
            {
                this.RefreshCurrentFeedbackAsync().Forget(this.Logger);
            }
        }

        /// <summary>
        /// Handles the <see cref="AppAudioSession.VolumeChanged"/> event, providing feedback to the Stream Deck.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="VolumeEventArgs"/> instance containing the event data.</param>
        private void OnVolumeChanged(object sender, VolumeEventArgs e)
            => this.RefreshCurrentFeedbackAsync().Forget(this.Logger);

        private async Task RefreshCurrentFeedbackAsync(bool setIcon = false)
        {
            if (this.CurrentFeedback == AudioControlAction.Volume)
            {
                // Set the feedback from the volume.
                var value = (int)(Math.Round(this.AppAudioSession?.Audio?.SimpleAudioVolume?.Volume ?? 0, 2) * 100);
                await this.SetFeedbackAsync(new VolumeFeedback()
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = value
                    },
                    Icon = setIcon ? this.AppAudioSession?.ThumbnailAsBase64 : null,
                    Value = this.AppAudioSession?.Audio?.SimpleAudioVolume?.Mute == true ? "Muted" : $"{value}%"
                });
            }
            else if (this.AppAudioSession?.TrackEndTime > TimeSpan.Zero)
            {
                // Set the feedback from the timeline, when we have a track and a value.
                await this.SetFeedbackAsync(new VolumeFeedback()
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = (int)Math.Ceiling(100 / this.AppAudioSession.TrackEndTime.TotalSeconds * this.AppAudioSession.TrackPosition.TotalSeconds)
                    },
                    Icon = setIcon ? this.AppAudioSession?.ThumbnailAsBase64 : null,
                    Value = this.AppAudioSession.TrackEndTime.Subtract(this.AppAudioSession.TrackPosition).ToString("mm':'ss")
                });
            }
            else
            {
                // Set the feedback from the timeline, when we have a track.
                await this.SetFeedbackAsync(new VolumeFeedback()
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = 0
                    },
                    Icon = setIcon ? this.AppAudioSession?.ThumbnailAsBase64 : null,
                    Value = "--:--"
                });
            }
        }
    }
}
