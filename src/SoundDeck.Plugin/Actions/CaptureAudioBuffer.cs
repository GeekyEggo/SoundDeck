namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Provides capturing of an audio buffer, similar to an instat replay.
    /// </summary>
    [StreamDeckAction("Audio Replay", UUID, "Images/Action", PropertyInspectorPath = "PI/captureAudioBuffer.html", Tooltip = "Capture the last x seconds of audio.")]
    [StreamDeckActionState("Images/Action")]
    public class CaptureAudioBuffer : StreamDeckAction<CaptureAudioBufferSettings>
    {
        /// <summary>
        /// The unique identifier for the action.
        /// </summary>
        public const string UUID = "com.geekyEggo.soundDeckCaptureAudioBuffer";

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureAudioBuffer"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public CaptureAudioBuffer(IAudioService audioService, ActionEventArgs<AppearancePayload> args)
            : base()
        {
            this.AudioService = audioService;

            var settings = args.Payload.GetSettings<CaptureAudioBufferSettings>();
            if (!string.IsNullOrWhiteSpace(settings?.AudioDeviceId))
            {
                this.AudioBuffer = this.AudioService.GetAudioBuffer(settings.AudioDeviceId, settings.Duration);
            }
        }

        /// <summary>
        /// Gets the audio service.
        /// </summary>
        private IAudioService AudioService { get; }

        /// <summary>
        /// Gets or sets the audio buffer.
        /// </summary>
        private IAudioBuffer AudioBuffer { get; set; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.AudioService.Devices.GroupBy(d => d.Flow).Select(g =>
            {
                var children = g.Select(opt => new Option(opt.FriendlyName, opt.Id)).ToList();
                return new Option(g.Key.ToString(), children);
            }).ToList();

            return Task.FromResult(new OptionsPayload(options));
        }

        /// <summary>
        /// Provides an entry point for the property inspector, allowing the user to select an output path.
        /// </summary>
        /// <returns>The payload containing information about the selected output path.</returns>
        [PropertyInspectorMethod]
        public async Task<FolderPickerPayload> GetOutputPath()
        {
            var settings = await this.GetSettingsAsync();
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Output Path";
                dialog.SelectedPath = settings.OutputPath;
                dialog.UseDescriptionForTitle = true;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    settings.OutputPath = dialog.SelectedPath;
                    await this.SetSettingsAsync(settings);

                    return new FolderPickerPayload(settings.OutputPath, true);
                }

                return new FolderPickerPayload(settings?.OutputPath, false);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:SharpDeck.StreamDeckActionEventReceiver.DidReceiveSettings" /> event.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of updating updating the audio buffer based on the new settings.</returns>
        protected override async Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args)
        {
            await base.OnDidReceiveSettings(args);
            var settings = args.Payload.GetSettings<CaptureAudioBufferSettings>();

            if (this.AudioBuffer != null && this.AudioBuffer.DeviceId != settings.AudioDeviceId)
            {
                this.AudioBuffer.Dispose();
                this.AudioBuffer = null;
            }

            if (this.AudioBuffer == null && !string.IsNullOrWhiteSpace(settings.AudioDeviceId))
            {
                this.AudioBuffer = this.AudioService.GetAudioBuffer(settings.AudioDeviceId, settings.Duration);
            }

            if (this.AudioBuffer != null)
            {
                this.AudioBuffer.BufferDuration = settings.Duration;
            }
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        /// <param name="args">The <see cref="T:SharpDeck.Events.Received.ActionEventArgs`1" /> instance containing the event data.</param>
        /// <returns>The task of saving the buffer to a file.</returns>
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            try
            {
                var settings = args.Payload.GetSettings<CaptureAudioBufferSettings>();
                if (this.AudioBuffer != null)
                {
                    var path = await this.AudioBuffer.SaveAsync(settings);

                    await this.StreamDeck.LogMessageAsync($"Saved captured from device {settings.AudioDeviceId} to {path}");
                    await this.ShowOkAsync();
                }
            }
            catch (Exception ex)
            {
                await this.StreamDeck.LogMessageAsync(ex.Message);
                await this.ShowAlertAsync();
            }
        }
    }
}
