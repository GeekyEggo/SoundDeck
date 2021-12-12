namespace SoundDeck.Core.Playback.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides play-next functionality.
    /// </summary>
    public class PlayNextController : PlaylistController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayNextController" /> class.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        internal PlayNextController(IAudioPlayer audioPlayer)
            : base(audioPlayer)
        {
            this.PlaybackType = ContinuousPlaybackType.Single;
        }

        /// <inheritdoc/>
        public override ControllerActionType Action { get; } = ControllerActionType.PlayNext;

        /// <inheritdoc/>
        protected override Task ActionAsync(CancellationToken cancellationToken)
        {
            // Stopping sets the cancellation token to cancelled, so we must supply a new one.
            this.Stop();
            return this.PlayAsync(this.ActiveCancellationToken);
        }
    }
}
