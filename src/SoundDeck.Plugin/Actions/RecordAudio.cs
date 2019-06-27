namespace SoundDeck.Plugin.Actions
{
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Settings;
    using System.Threading.Tasks;

    [StreamDeckAction("Record Audio", UUID, "Images/RecordAudio/Action", Tooltip = "Record Audio", SupportedInMultiActions = false)]
    [StreamDeckActionState("Images/RecordAudio/Key0")]
    [StreamDeckActionState("Images/RecordAudio/Key1")]
    public class RecordAudio : BaseCaptureAction<RecordAudioSettings, IAudioRecorder>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeck.recordAudio";

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAudio"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        /// <param name="args">The <see cref="ActionEventArgs{AppearancePayload}"/> instance containing the event data.</param>
        public RecordAudio(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
            : base(audioService)
        {
            this.CaptureDevice = this.GetCaptureDevice(args.Payload.GetSettings<RecordAudioSettings>());
        }

        /// <summary>
        /// Gets the capture device, for the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The capture device.</returns>
        protected override IAudioRecorder GetCaptureDevice(RecordAudioSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings?.AudioDeviceId)
                ? this.AudioService.GetAudioRecorder(settings.AudioDeviceId)
                : null;
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{KeyPayload}" /> instance containing the event data.</param>
        /// <returns>The task of the key down.</returns>
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            if (args.Payload.State == 0)
            {
                this.CaptureDevice.Start();
            }
            else if (args.Payload.State == 1)
            {
                this.CaptureDevice.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
