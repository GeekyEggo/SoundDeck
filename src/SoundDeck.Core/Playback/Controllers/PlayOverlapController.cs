namespace SoundDeck.Core.Playback.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a <see cref="PlaylistController"/> that overlaps the next audio track over the current.
    /// </summary>
    public class PlayOverlapController : PlaylistController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayOverlapController"/> class.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        public PlayOverlapController(IAudioPlayer audioPlayer)
            : base(audioPlayer)
        {
        }

        /// <inheritdoc/>
        public override ControllerActionType Action { get; } = ControllerActionType.PlayOverlap;

        /// <inheritdoc/>
        protected override Task PlayAsync(AudioFileInfo file, CancellationToken cancellationToken)
        {
            _ = this.AudioPlayer
                .Clone()
                .PlayAsync(file, cancellationToken);

            return Task.CompletedTask;
        }
    }
}
