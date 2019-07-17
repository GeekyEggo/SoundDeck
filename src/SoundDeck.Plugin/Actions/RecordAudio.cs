namespace SoundDeck.Plugin.Actions
{
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Core.Capture;
    using SoundDeck.Plugin.Models.Settings;
    using System.Threading;
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

        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

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
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        private bool IsInitialized { get; set; }

        /// <summary>
        /// Occurs when an instance of an action appears.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                await this.SetStateAsync(0);
            }
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
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                if (args.Payload.State == 0)
                {
                    this.CaptureDevice.Settings = args.Payload.GetSettings<RecordAudioSettings>();
                    await this.CaptureDevice.StartAsync();
                }
                else if (args.Payload.State == 1)
                {
                    await this.CaptureDevice.StopAsync();
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }
    }
}
