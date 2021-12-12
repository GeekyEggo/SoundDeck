namespace SoundDeck.Core.Playback.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides play-stop functionality.
    /// </summary>
    public class PlayStopController : PlaylistController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayStopController" /> class.
        /// </summary>
        /// <param name="audioPlayer">The audio player.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="playbackType">Type of the playback.</param>
        internal PlayStopController(IAudioPlayer audioPlayer, ControllerActionType actionType, ContinuousPlaybackType playbackType)
            : base(audioPlayer)
        {
            this.Action = actionType;
            this.PlaybackType = playbackType;
        }

        /// <inheritdoc/>
        public override ControllerActionType Action { get; }

        /// <inheritdoc/>
        protected override Task ActionAsync(CancellationToken cancellationToken)
        {
            if (this.AudioPlayer.IsPlaying)
            {
                this.Stop();
                return Task.CompletedTask;
            }

            return this.PlayAsync(cancellationToken);
        }
    }
}
