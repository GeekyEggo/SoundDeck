namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides play-stop functionality.
    /// </summary>
    public class PlayStopPlayer : PlaylistPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayStopPlayer" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="playbackType">Type of the playback.</param>
        public PlayStopPlayer(PlaylistPlayerOptions options, PlaylistPlayerActionType actionType, PlaylistPlaybackType playbackType)
            : base(options)
        {
            this.Action = actionType;
            this.PlaybackType = playbackType;
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override PlaylistPlayerActionType Action { get; }

        /// <summary>
        /// Applies the next action asynchronously.
        /// </summary>
        /// <returns>The task of running the action.</returns>
        protected override Task ActionAsync()
        {
            if (this.State == PlaybackStateType.Playing
                || this.Player.State == PlaybackStateType.Playing)
            {
                this.Stop();
                return Task.CompletedTask;
            }

            return this.PlayAsync();
        }
    }
}
