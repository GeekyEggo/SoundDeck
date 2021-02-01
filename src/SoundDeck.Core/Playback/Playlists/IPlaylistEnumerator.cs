namespace SoundDeck.Core.Playback.Playlists
{
    /// <summary>
    /// Provides an enumerator for a playlist.
    /// </summary>
    public interface IPlaylistEnumerator
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IPlaylist.Current" /> is the last item.
        /// </summary>
        bool IsLast { get; }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <param name="item">The current item when advancing was successful.</param>
        /// <returns><c>true</c> whilst there are items within the collection; <c>false</c> if the collection has no items.</returns>
        bool TryMoveNext(out AudioFileInfo item);

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        void Reset();
    }
}
