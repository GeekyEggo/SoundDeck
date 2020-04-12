namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides options required of all <see cref="IPlaylistPlayer"/>
    /// </summary>
    public class PlaylistPlayerOptions
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the normalization provider.
        /// </summary>
        public INormalizationProvider NormalizationProvider { get; set; }

        /// <summary>
        /// Gets or sets the playlist.
        /// </summary>
        public IPlaylist Playlist { get; set; }
    }
}
