namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;
    using SoundDeck.Plugin.Models.Settings;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class CaptureAudioBuffer : StreamDeckAction<CaptureAudioBufferSettings>
    {
        public CaptureAudioBuffer(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
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

        [PropertyInspectorMethod]
        public Task<AudioDevicesPayload> GetAudioDevices()
        {
            return Task.FromResult(new AudioDevicesPayload
            {
                Devices = this.AudioService.GetDevices().ToArray()
            });
        }

        protected override Task OnPropertyInspectorDidAppear(ActionEventArgs args)
            => base.OnPropertyInspectorDidAppear(args);

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
