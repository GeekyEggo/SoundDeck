namespace SoundDeck.Plugin.Actions
{
    using SharpDeck;
    using SharpDeck.Events;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Settings;
    using System;
    using System.Threading.Tasks;

    public class ReplayBufferAction : StreamDeckAction<ReplayBufferSettings>
    {
        public ReplayBufferAction(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
            this.Initialized += this.ReplayBufferAction_Initialized;
        }

        private void ReplayBufferAction_Initialized(object sender, EventArgs e)
        {
            this.Buffer = this.AudioService.GetBuffer(this.Settings.AudioDeviceId);
        }

        public IAudioService AudioService { get; }
        public IAudioBuffer Buffer { get; private set; }

        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var path = await this.Buffer.SaveAsync(this.Settings.ClipDuration, this.Settings.OutputPath);
            
            await this.StreamDeck.LogMessageAsync($"Saved captured from device {this.Settings.AudioDeviceId} to {path}");
            await this.ShowOkAsync();
        }
    }
}
