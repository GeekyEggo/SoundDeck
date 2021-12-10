namespace SoundDeck.Core.Playback.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class PlayOverlapController : PlaylistController
    {
        public PlayOverlapController(IAudioPlayer audioPlayer, Func<IAudioPlayer> createAudioPlayer)
            : base(audioPlayer)
        {
            this.CreateAudioPlayer = createAudioPlayer;
        }

        public override ControllerActionType Action { get; } = ControllerActionType.PlayOverlap;
        private Func<IAudioPlayer> CreateAudioPlayer { get; }

        protected override Task PlayAsync(AudioFileInfo file, CancellationToken cancellationToken)
        {
            var audioPlayer = this.CreateAudioPlayer();
            cancellationToken.Register(() => audioPlayer.Stop(), useSynchronizationContext: false);

            _ = Task.Factory.StartNew(async (state) =>
            {
                try
                {
                    await audioPlayer.PlayAsync((AudioFileInfo)state);
                }
                finally
                {
                    audioPlayer.Dispose();
                }
            },
            file,
            TaskCreationOptions.RunContinuationsAsynchronously);

            return Task.CompletedTask;
        }
    }
}
