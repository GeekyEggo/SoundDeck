namespace SoundDeck.Core.Playback.Controllers
{
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

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override ControllerActionType Action { get; } = ControllerActionType.PlayNext;

        /// <summary>
        /// Applies the next action asynchronously.
        /// </summary>
        /// <returns>The task of running the action.</returns>
        protected override Task ActionAsync()
        {
            this.Stop();
            return this.PlayAsync();
        }
    }
}
