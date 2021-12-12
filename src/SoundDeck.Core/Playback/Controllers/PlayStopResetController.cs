namespace SoundDeck.Core.Playback.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player whereby the current position of the <see cref="IPlaylist"/> is reset prior to playing.
    /// </summary>
    public class PlayStopResetController : PlayStopController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayStopResetController"/> class.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="playbackType">Type of the playback.</param>
        internal PlayStopResetController(IAudioPlayer audioPlayer, ControllerActionType actionType = ControllerActionType.PlayStop, ContinuousPlaybackType playbackType = ContinuousPlaybackType.Single)
            : base(audioPlayer, actionType, playbackType)
        {
        }

        /// <inheritdoc/>
        protected override Task PlayAsync(CancellationToken cancellationToken)
        {
            this.Enumerator.Reset();
            return base.PlayAsync(cancellationToken);
        }
    }
}
