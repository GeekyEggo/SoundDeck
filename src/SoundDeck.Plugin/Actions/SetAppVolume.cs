namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using Nito.AsyncEx;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Layouts;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action that is capable of changing the volume of an application.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setappvolume")]
    public class SetAppVolume : AppActionBase<SetAppVolumeSettings>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="SetAppVolume"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="appAudioService">The application audio service.</param>
        public SetAppVolume(IAudioService audioService, IAppAudioService appAudioService)
            : base(audioService, appAudioService)
        {
        }

        /// <summary>
        /// Gets or sets the session that monitors the volume associated with an application.
        /// </summary>
        private AppAudioSession AudioSession { get; set; }

        /// <inheritdoc/>
        protected async override Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            if (args.Payload.Controller == Controller.Keypad)
            {
                return;
            }

            using (await this._syncRoot.LockAsync())
            {
                if (this.AudioSession is null)
                {
                    this.AudioSession = new AppAudioSession(this.AppAudioService, new SetAppVolumeSettings
                    {
                        ProcessSelectionType = ProcessSelectionType.Foreground
                    });

                    this.AudioSession.Changed += this.OnAudioSessionChanged;
                }

                this.RefreshFeedbackAsync().Forget();
                this.AudioSession.EnableRaisingEvents = true;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            if (args.Payload.Controller == Controller.Encoder
                && this.AudioSession is AppAudioSession session)
            {
                session.EnableRaisingEvents = false;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);
            await this.UpdateWithVolumeAsync(() =>
            {
                var step = args.Payload.GetSettings<SetAppVolumeSettings>().VolumeValue;
                step = step > 25 ? 5 : step;

                return args.Payload.Ticks > 0
                    ? new VolumeSettings(VolumeAction.IncreaseBy, step * args.Payload.Ticks)
                    : new VolumeSettings(VolumeAction.DecreaseBy, step * args.Payload.Ticks * -1);
            });
        }

        /// <inheritdoc/>
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<SetAppVolumeSettings>();

            try
            {
                this.AppAudioService.SetVolume(settings);
                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set app audio volume; Action=\"{settings.VolumeAction}\", Value=\"{settings.VolumeValue}\", ProcessSelectionType=\"{settings.ProcessSelectionType}\", ProcessName=\"{settings.ProcessName}\".");
                await this.ShowAlertAsync();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.AudioSession?.Dispose();
            this.AudioSession = null;
        }

        /// <summary>
        /// Attempts to update the volume associated with the <see cref="MMDevice"/> for the <see cref="AudioDevice"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The task of updating the volume.</returns>
        private async Task UpdateWithVolumeAsync(Func<VolumeSettings> getVolume)
        {
            try
            {
                using (await this._syncRoot.LockAsync())
                {
                    try
                    {
                        this.AudioSession.Session.SimpleAudioVolume.Set(getVolume());
                    }
                    catch
                    {
                        await this.ShowAlertAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set app audio volume; App=\"{this.AudioSession?.DisplayName}\".");
                await this.ShowAlertAsync();
            }
        }

        /// <summary>
        /// Refreshes the feedback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnAudioSessionChanged(object sender, EventArgs e)
            => this.RefreshFeedbackAsync().Forget();

        /// <summary>
        /// Refreshes the feedback asynchronously.
        /// </summary>
        /// <returns>The task of refreshing the feedback.</returns>
        private async Task RefreshFeedbackAsync()
        {
            using (await this._syncRoot.LockAsync())
            {
                if (this.AudioSession is not null)
                {
                    var percent = (int)Math.Round(this.AudioSession.Volume * 100);
                    await this.SetFeedbackAsync(new LayoutB1
                    {
                        Indicator = percent,
                        Title = "Chrome",
                        Value = this.AudioSession.IsMuted ? "Muted" : $"{percent}%"
                    });
                }
                else
                {
                    await this.SetFeedbackAsync(new LayoutB1
                    {
                        Indicator = 0,
                        Title = "None",
                        Value = "-"
                    });
                }
            }
        }
    }
}
