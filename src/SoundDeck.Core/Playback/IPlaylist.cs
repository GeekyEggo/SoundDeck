namespace SoundDeck.Core.Playback
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a playlist of files.
    /// </summary>
    public interface IPlaylist : IEnumerator<AudioFileInfo>
    {
        /// <summary>
        /// Gets the number of items within the playlist.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IEnumerator{T}.Current"/> is the last.
        /// </summary>
        bool IsLast { get; }
    }
}
