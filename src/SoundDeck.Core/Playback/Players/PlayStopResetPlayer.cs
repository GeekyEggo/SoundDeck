namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player whereby the current position of the <see cref="IPlaylist"/> is reset prior to playing.
    /// </summary>
    public class PlayStopResetPlayer : PlayStopPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayStopResetPlayer"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="playbackType">Type of the playback.</param>
        public PlayStopResetPlayer(PlaylistPlayerOptions options, PlaylistPlayerActionType actionType = PlaylistPlayerActionType.PlayStop, PlaylistPlaybackType playbackType = PlaylistPlaybackType.Single)
            : base(options, actionType, playbackType)
        {
        }

        /// <summary>
        /// Continues playing asynchronously.
        /// </summary>
        /// <returns>The task of playing.</returns>
        protected override Task PlayAsync()
        {
            this.Playlist.Reset();
            return base.PlayAsync();
        }
    }
}
