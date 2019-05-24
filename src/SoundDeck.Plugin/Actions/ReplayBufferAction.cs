namespace SoundDeck.Plugin.Actions
{
    using SharpDeck.Events;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Settings;
    using System.Threading.Tasks;

    public class ReplayBufferAction : StreamDeckAction
    {
        public ReplayBufferAction(IAudioService audioService)
            : base()
        {
            this.AudioService = audioService;
            this.Settings = new ReplayBufferSettings();

            this.Buffer = this.AudioService.GetBuffer(this.Settings.AudioDeviceId);
        }

        public IAudioService AudioService { get; }
        public ReplayBufferSettings Settings { get; }
        public IAudioBuffer Buffer { get; }
        
        protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            var path = await this.Buffer.SaveAsync(this.Settings.ClipDuration, this.Settings.OutputPath);
            
            await this.StreamDeck.LogMessage($"Saved captured from device {this.Settings.AudioDeviceId} to {path}");
            await this.ShowOkAsync();
        }
    }
}
