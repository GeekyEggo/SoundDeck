namespace SoundDeck.Plugin.Actions
{
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides an action for setting the default audio device.
    /// </summary>
    [StreamDeckAction("com.geekyeggo.sounddeck.setdefaultaudiodevice")]
    public class SetDefaultAudioDevice : ActionBase<SetDefaultAudioDeviceSettings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultAudioDevice"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public SetDefaultAudioDevice(IAudioService audioService)
            : base(audioService)
        {
        }

        /// <inheritdoc/>
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var settings = args.Payload.GetSettings<SetDefaultAudioDeviceSettings>();
            this.AudioService.SetDefaultDevice(settings.AudioDeviceId, settings.Role);

            return this.ShowOkAsync();
        }
    }
}
