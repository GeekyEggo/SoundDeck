namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides play-next functionality.
    /// </summary>
    public class PlayNextPlayer : PlaylistPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayNextPlayer" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public PlayNextPlayer(PlaylistPlayerOptions options)
            : base(options)
        {
            this.PlaybackType = PlaylistPlaybackType.Single;
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override PlaylistPlayerActionType Action { get; } = PlaylistPlayerActionType.PlayNext;

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
