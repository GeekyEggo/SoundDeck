namespace SoundDeck.Plugin.Actions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action that adjusts the volume of an audio device.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setaudiodevicevolume")]
    public class SetAudioDeviceVolume : ActionBase<SetAudioDeviceVolumeSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetAudioDeviceVolume"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public SetAudioDeviceVolume(IAudioService audioService)
            : base(audioService)
        {
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
    }
}
