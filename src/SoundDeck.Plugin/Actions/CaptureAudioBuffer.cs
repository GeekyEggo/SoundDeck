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
        public CaptureAudioBuffer(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
            this.Devices = audioService.GetDevices().ToArray();

            this.Initialized += this.ReplayBufferAction_Initialized;
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
        /// Gets the devices.
        /// </summary>
        private AudioDevice[] Devices { get; }

        /// <summary>
        /// Provides an entry point for the property inspector, which can be used to get the audio devices available on the system.
        /// </summary>
        /// <returns>The payload containing the audio devices.</returns>
        [PropertyInspectorMethod]
        public Task<OptionsPayload> GetAudioDevices()
        {
            var options = this.Devices.GroupBy(d => d.Flow).Select(g =>
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
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Output Path";
                dialog.SelectedPath = this.Settings?.OutputPath;
                dialog.UseDescriptionForTitle = true;

                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.Settings.OutputPath = dialog.SelectedPath;
                    await this.SetSettingsAsync(this.Settings);

                    return new FolderPickerPayload(this.Settings.OutputPath, true);
                }

                return new FolderPickerPayload(this.Settings?.OutputPath, false);
            }
        }

        /// <summary>
        /// Called when the action receives the strongly-typed settings.
        /// </summary>
        /// <param name="args">The <see cref="ActionEventArgs{ActionPayload}" /> instance containing the event data.</param>
        /// <param name="settings">The settings.</param>
        protected async override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, CaptureAudioBufferSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            if (this.Settings.AudioDeviceId != settings.AudioDeviceId)
            {
                this.AudioBuffer.Dispose();
                this.AudioBuffer = this.AudioService.GetAudioBuffer(settings.AudioDeviceId, settings.Duration);
            }

            if (this.AudioBuffer != null)
            {
                this.AudioBuffer.BufferDuration = settings.Duration;
            }

            this.Settings = settings;
        }

        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            if (this.AudioBuffer != null)
            {
                var path = await this.AudioBuffer.SaveAsync(this.Settings);

                await this.StreamDeck.LogMessageAsync($"Saved captured from device {this.Settings.AudioDeviceId} to {path}");
                await this.ShowOkAsync();
            }
        }

        /// <summary>
        /// Handles the <see cref="StreamDeckAction.Initialized"/> event of the this instance.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ReplayBufferAction_Initialized(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.Settings?.AudioDeviceId))
            {
                this.AudioBuffer = this.AudioService.GetAudioBuffer(this.Settings.AudioDeviceId, this.Settings.Duration);
            }
        }
    }
}
