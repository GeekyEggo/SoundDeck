namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SharpDeck.PropertyInspectors.Payloads;
    using SharpDeck.Registration;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    [StreamDeckAction("Audio Replay", "com.geekyEggo.soundDeckCaptureAudioBuffer", "Images/Action", PropertyInspectorPath = "PI/captureAudioBuffer.html", Tooltip = "Capture the last x seconds of audio.")]
    [StreamDeckActionState("Images/Action")]
    public class CaptureAudioBuffer : StreamDeckAction<CaptureAudioBufferSettings>
    {
        public CaptureAudioBuffer(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
            this.Devices = audioService.GetDevices().ToArray();
            this.Initialized += this.ReplayBufferAction_Initialized;
        }

        private void ReplayBufferAction_Initialized(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.Settings.AudioDeviceId))
            {
                this.AudioBuffer = this.AudioService.GetAudioBuffer(this.Settings.AudioDeviceId, this.Settings.ClipDuration);
            }
        }

        public IAudioService AudioService { get; }
        private IAudioBuffer AudioBuffer { get; set; }
        private AudioDevice[] Devices { get; }

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

        protected override Task OnPropertyInspectorDidAppear(ActionEventArgs args)
            => base.OnPropertyInspectorDidAppear(args);

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
            => base.OnWillAppear(args);

        protected async override Task OnDidReceiveSettings(ActionEventArgs<ActionPayload> args, CaptureAudioBufferSettings settings)
        {
            await base.OnDidReceiveSettings(args, settings);
            if (this.Settings.AudioDeviceId != settings.AudioDeviceId)
            {
                this.AudioBuffer.Dispose();
                this.AudioBuffer = this.AudioService.GetAudioBuffer(settings.AudioDeviceId, settings.ClipDuration);
            }

            if (this.AudioBuffer != null)
            {
                this.AudioBuffer.BufferDuration = settings.ClipDuration;
            }

            this.Settings = settings;
        }

        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            if (this.AudioBuffer != null)
            {
                var path = await this.AudioBuffer.SaveAsync(this.Settings.ClipDuration, this.Settings.OutputPath);

                await this.StreamDeck.LogMessageAsync($"Saved captured from device {this.Settings.AudioDeviceId} to {path}");
                await this.ShowOkAsync();
            }
        }
    }
}
