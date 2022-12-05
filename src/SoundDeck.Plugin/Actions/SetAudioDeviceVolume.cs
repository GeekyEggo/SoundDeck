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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using (this._syncRoot.Lock())
            {
                AudioDevices.Current.DefaultDeviceChanged -= this.AudioDevices_Changed;
                AudioDevices.Current.DevicesChanged -= this.AudioDevices_Changed;
                this.AudioDevice = null;

                base.Dispose(disposing);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            await base.OnDidReceiveSettings(args);
            await this.TrySetAudioDeviceAsync(args.Payload.GetSettings<SetAudioDeviceVolumeSettings>()?.AudioDeviceKey);
        }

        /// <inheritdoc/>
        protected override async Task OnDialPress(ActionEventArgs<DialPayload> args)
        {
            await base.OnDialPress(args);
            if (args.Payload.Pressed)
            {
                await this.TryUpdateVolumeAsync(volume => volume.Set(VolumeSettings.TOGGLE_MUTE));
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDialRotate(ActionEventArgs<DialRotatePayload> args)
        {
            await base.OnDialRotate(args);
            await this.TryUpdateVolumeAsync(volume =>
            {
                var step = args.Payload.GetSettings<SetAudioDeviceVolumeSettings>().VolumeValue;
                step = step > 25 ? 5 : step;

                if (args.Payload.Ticks > 0)
                {
                    volume.Set(new VolumeSettings(VolumeAction.IncreaseBy, step * args.Payload.Ticks));
                }
                else if (args.Payload.Ticks < 0)
                {
                    volume.Set(new VolumeSettings(VolumeAction.DecreaseBy, step * args.Payload.Ticks * -1));
                }
            });
        }

        /// <inheritdoc/>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            await base.OnKeyDown(args);
            await this.TryUpdateVolumeAsync(volume =>
            {
                var settings = args.Payload.GetSettings<SetAudioDeviceVolumeSettings>();
                volume.Set(settings);

                this.ShowOkAsync().Forget(this.Logger);
            });
        }

        /// <inheritdoc/>
        protected override async Task OnTouchTap(ActionEventArgs<TouchTapPayload> args)
        {
            await base.OnTouchTap(args);
            await this.TryUpdateVolumeAsync(volume => volume.Set(VolumeSettings.TOGGLE_MUTE));
        }

        /// <inheritdoc/>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillAppear(args);
            await this.TrySetAudioDeviceAsync(args.Payload.GetSettings<SetAudioDeviceVolumeSettings>()?.AudioDeviceKey);
        }

        /// <inheritdoc/>
        protected override async Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            await base.OnWillDisappear(args);
            this.AudioDevice = null;
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
                if (this.AudioDevice is not null)
                {
                    var percent = (int)Math.Round(this.AudioDevice.Volume * 100);
                    await this.SetFeedbackAsync(new VolumeFeedback
                    {
                        Indicator = new VolumeIndicator
                        {
                            IsEnabled = true,
                            Opacity = 1,
                            Value = percent
                        },
                        Title = this.AudioDevice.DeviceName,
                        Value = this.AudioDevice.IsMuted ? "Muted" : $"{percent}%"
                    });
                }
                else
                {
                    await this.SetFeedbackAsync(new VolumeFeedback
                    {
                        Indicator = new VolumeIndicator
                        {
                            IsEnabled = true,
                            Opacity = 1,
                            Value = 0
                        },
                        Title = "None",
                        Value = "-"
                    });
                }
            }
        }

        /// <summary>
        /// When <see cref="ActionBase{TSettings}.IsEncoder"/> is <c>true</c>, and there is a valid <see cref="SetAudioDeviceVolumeSettings.AudioDeviceId"/>, the <see cref="AudioDevice"/> is set.
        /// </summary>
        /// <param name="audioDeviceKey">The audio device key.</param>
        private async Task TrySetAudioDeviceAsync(string audioDeviceKey = default)
        {
            if (!this.IsEncoder)
            {
                return;
            }

            using (await this._syncRoot.LockAsync())
            {
                var key = audioDeviceKey ?? this.AudioDevice?.Key ?? AudioDevices.PLAYBACK_DEFAULT;
                this.AudioDevice = !string.IsNullOrWhiteSpace(key)
                    ? AudioDevices.Current.GetDeviceByKey(key)
                    : null;
            }

            await this.RefreshFeedbackAsync();
        }

        /// <summary>
        /// Attempts to update the volume associated with the <see cref="MMDevice"/> for the <see cref="AudioDevice"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The task of updating the volume.</returns>
        private async Task TryUpdateVolumeAsync(Action<AudioEndpointVolume> action)
        {
            try
            {
                using (await this._syncRoot.LockAsync())
                {
                    if (this.AudioDevice is IAudioDevice device and not null)
                    {
                        using (var mmDevice = device.GetMMDevice())
                        {
                            action(mmDevice.AudioEndpointVolume);
                        }
                    }
                    else
                    {
                        await this.ShowAlertAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, $"Failed to set audio device volume; Device=\"{this.AudioDevice.Id}\".");
                await this.ShowAlertAsync();
            }
        }
    }
}
