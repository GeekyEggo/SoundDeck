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
    using SharpDeck.Events.Sent.Feedback;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Models.Settings;
    using SoundDeck.Plugin.Windows;

    /// <summary>
    /// Provides an action that adjusts the volume of an audio device.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setaudiodevicevolume")]
    public class SetAudioDeviceVolume : ActionBase<SetAudioDeviceVolumeSettings>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private backing field for <see cref="AudioDevice"/>.
        /// </summary>
        private IAudioDevice _audioDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetAudioDeviceVolume"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public SetAudioDeviceVolume(IAudioService audioService)
            : base(audioService)
        {
            AudioDevices.Current.DefaultDeviceChanged += this.AudioDevices_Changed;
            AudioDevices.Current.DevicesChanged += this.AudioDevices_Changed;
        }

        /// <summary>
        /// Gets or sets the audio device associated with this action.
        /// </summary>
        private IAudioDevice AudioDevice
        {
            get => this._audioDevice;
            set
            {
                if (this._audioDevice?.Id == value?.Id)
                {
                    return;
                }

                if (this._audioDevice is not null)
                {
                    this._audioDevice.VolumeChanged -= this.AudioDevice_VolumeChanged;
                }

                this._audioDevice = value;
                if (this._audioDevice is not null)
                {
                    this._audioDevice.VolumeChanged += this.AudioDevice_VolumeChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        private string AudioDeviceKey { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using (this._syncRoot.Lock())
            {
                AudioDevices.Current.DefaultDeviceChanged -= this.AudioDevices_Changed;
                AudioDevices.Current.DevicesChanged -= this.AudioDevices_Changed;
                App.Current.Dispatcher.Invoke(() => this.AudioDevice = null);

                base.Dispose(disposing);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            await base.OnDidReceiveSettings(args);
            await this.TrySetAudioDeviceAsync(args.Payload.GetSettings<SetAudioDeviceVolumeSettings>());
        }

        /// <inheritdoc/>
        protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
        {
            await base.OnDialPress(args);
            this.ToggleMute();
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);
            using (this._syncRoot.Lock())
            {
                if (this.AudioDevice is IAudioDevice device and not null)
                {
                    using (var mmDevice = device.GetMMDevice())
                    {
                        var step = args.Payload.GetSettings<SetAudioDeviceVolumeSettings>()?.VolumeValue ?? 5;
                        if (args.Payload.Ticks > 0)
                        {
                            mmDevice.AudioEndpointVolume.Set(new VolumeSettings(VolumeAction.IncreaseBy, step * args.Payload.Ticks));
                        }
                        else if (args.Payload.Ticks < 0)
                        {
                            mmDevice.AudioEndpointVolume.Set(new VolumeSettings(VolumeAction.DecreaseBy, step * args.Payload.Ticks * -1));
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<SetAudioDeviceVolumeSettings>();
            try
            {
                using (var device = AudioDevices.Current.GetDeviceByKey(settings.AudioDeviceId).GetMMDevice())
                {
                    device.AudioEndpointVolume.Set(settings);
                }

                await this.ShowOkAsync();
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set audio device volume; Device=\"{settings.AudioDeviceId}\", Action=\"{settings.VolumeAction}\", Value=\"{settings.VolumeValue}\".");
                await this.ShowAlertAsync();
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTouchTap(ActionEventArgs<TouchTapPayload> args)
        {
            await base.OnTouchTap(args);
            this.ToggleMute();
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            await this.TrySetAudioDeviceAsync(args.Payload.GetSettings<SetAudioDeviceVolumeSettings>());
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            App.Current.Dispatcher.Invoke(() => this.AudioDevice = null);
        }

        /// <summary>
        /// Handles the <see cref="AudioEndMaster"/>
        /// </summary>
        /// <param name="data">The data.</param>
        private void AudioDevice_VolumeChanged(IAudioDevice device, AudioVolumeNotificationData data)
            => this.RefreshFeedbackAsync().Forget(this.Logger);

        /// <summary>
        /// Handles <see cref="AudioDevices.DefaultDeviceChanged"/> and <see cref="AudioDevices.DevicesChanged"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AudioDevices_Changed(object sender, EventArgs e)
            => this.TrySetAudioDeviceAsync().Forget(this.Logger);

        /// <summary>
        /// Refreshes the feedback asynchronously.
        /// </summary>
        /// <returns>The task of refreshing the feedback.</returns>
        private async Task RefreshFeedbackAsync()
        {
            using (await this._syncRoot.LockAsync())
            {
                if (this.AudioDevice is null)
                {
                    return;
                }

                var percent = (int)Math.Round(this.AudioDevice.Volume * 100);
                await this.SetFeedbackAsync(new VolumeFeedback
                {
                    Indicator = new VolumeIndicator
                    {
                        IsEnabled = true,
                        Opacity = 1,
                        Value = percent
                    },
                    Title = this.AudioDevice.FriendlyName,
                    Value = this.AudioDevice.IsMuted ? "Muted" : $"{percent}%"
                });
            }
        }

        /// <summary>
        /// Toggles the mute state of <see cref="AudioDevice"/>.
        /// </summary>
        private void ToggleMute()
        {
            using (this._syncRoot.Lock())
            {
                if (this.AudioDevice is MMDevice mmDevice and not null)
                {
                    mmDevice.AudioEndpointVolume.Mute = !mmDevice.AudioEndpointVolume.Mute;
                }
            }
        }

        /// <summary>
        /// When <see cref="ActionBase{TSettings}.IsEncoder"/> is <c>true</c>, and there is a valid <see cref="SetAudioDeviceVolumeSettings.AudioDeviceId"/>, the <see cref="AudioDevice"/> is set.
        /// </summary>
        /// <param name="settings">The settings.</param>
        private async Task TrySetAudioDeviceAsync(SetAudioDeviceVolumeSettings settings = null)
        {
            if (!this.IsEncoder)
            {
                return;
            }

            using (await this._syncRoot.LockAsync())
            {
                this.AudioDeviceKey = settings?.AudioDeviceId ?? this.AudioDeviceKey;
                this.AudioDevice = !string.IsNullOrWhiteSpace(this.AudioDeviceKey)
                    ? AudioDevices.Current.GetDeviceByKey(this.AudioDeviceKey)
                    : null;
            }

            await this.RefreshFeedbackAsync();
        }
    }
}
