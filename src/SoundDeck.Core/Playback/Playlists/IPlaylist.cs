namespace SoundDeck.Core.Playback.Playlists
{
    using System;

    /// <summary>
    /// Provides information about a playlist.
    /// </summary>
    public interface IPlaylist
    {
        /// <summary>
        /// Occurs when the playlist has changed.
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        AudioFileInfo[] Files { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        PlaybackOrderType Order { get; set; }
    }
}
