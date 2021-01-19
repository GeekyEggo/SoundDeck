namespace SoundDeck.Core.Playback
{
    /// <summary>
    /// Provides options for a playlist.
    /// </summary>
    public interface IPlaylistOptions
    {
        /// <summary>
        /// Gets the audio files to play.
        /// </summary>
        AudioFileInfo[] Files { get; }

        /// <summary>
        /// Gets the playback order.
        /// </summary>
        PlaylistOrderType Order { get; }
    }
}
