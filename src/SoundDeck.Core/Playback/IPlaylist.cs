namespace SoundDeck.Core.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a playlist of files.
    /// </summary>
    public interface IPlaylist : IEnumerator<PlaylistFile>
    {
        /// <summary>
        /// Gets the number of items within the playlist.
        /// </summary>
        int Count { get; }
    }
}
