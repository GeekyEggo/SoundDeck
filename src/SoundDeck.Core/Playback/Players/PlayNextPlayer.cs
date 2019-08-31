namespace SoundDeck.Core.Playback.Players
{
    using System.Threading.Tasks;

    /// <summary>
    /// A playlist player that provides play-next functionality.
    /// </summary>
    public class PlayNextPlayer : PlaylistPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayNextPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="playlist">The playlist.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public PlayNextPlayer(string deviceId, Playlist playlist, INormalizationProvider normalizationProvider)
            : base(deviceId, playlist, normalizationProvider)
        {
            this.IsLooped = false;
        }

        /// <summary>
        /// Gets the underlying action that determines how the player functions.
        /// </summary>
        public override PlaylistPlayerActionType Action => PlaylistPlayerActionType.PlayNext;

        /// <summary>
        /// Plays the next item within the playlist.
        /// </summary>
        /// <returns>The task of playing the next item.</returns>
        protected override Task PlayNextAsnyc()
        {
            this.Stop();
            return base.PlayNextAsnyc();
        }
    }
}
