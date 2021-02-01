namespace SoundDeck.Core.Playback.Controllers
{
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

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override ControllerActionType Action { get; }

        /// <summary>
        /// Applies the next action asynchronously.
        /// </summary>
        /// <returns>The task of running the action.</returns>
        protected override Task ActionAsync()
        {
            if (this.AudioPlayer.IsPlaying)
            {
                this.Stop();
                return Task.CompletedTask;
            }

            return this.PlayAsync();
        }
    }
}
