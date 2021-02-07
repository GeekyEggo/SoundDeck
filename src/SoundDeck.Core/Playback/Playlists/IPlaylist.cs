namespace SoundDeck.Core.Playback.Playlists
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// Provides information about a playlist.
    /// </summary>
    public interface IPlaylist : INotifyCollectionChanged, IEnumerable<AudioFileInfo>
    {
        /// <summary>
        /// Gets the <see cref="AudioFileInfo"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The audio file at the index.</returns>
        AudioFileInfo this[int index] { get; }

        /// <summary>
        /// Gets the count of files.
        /// </summary>
        int Count { get; }
    }
}
