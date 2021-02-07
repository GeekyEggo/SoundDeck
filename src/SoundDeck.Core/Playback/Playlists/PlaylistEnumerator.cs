namespace SoundDeck.Core.Playback.Playlists
{
    using System;
    using System.Collections.Specialized;

    /// <summary>
    /// Provides a <see cref="IPlaylistEnumerator"/>.
    /// </summary>
    public class PlaylistEnumerator : IPlaylistEnumerator
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistEnumerator"/> class.
        /// </summary>
        /// <param name="playlist">The playlist.</param>
        public PlaylistEnumerator(IPlaylist playlist)
        {
            this.Playlist = playlist;
            this.Playlist.CollectionChanged += this.Playlist_CollectionChanged;
        }

        /// <summary>
        /// Gets the index of the current item.
        /// </summary>
        public virtual int CurrentIndex => this.SequenceIndex;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPlaylist.Current" /> is the last item.
        /// </summary>
        public bool IsLast => this.SequenceIndex + 1 >= this.Playlist.Count;

        /// <summary>
        /// Gets the current item.
        /// </summary>
        protected virtual AudioFileInfo Current => this.Playlist[this.SequenceIndex];

        /// <summary>
        /// Gets the playlist.
        /// </summary>
        protected IPlaylist Playlist { get; }

        /// <summary>
        /// Gets the sequence index.
        /// </summary>
        protected int SequenceIndex { get; private set; } = -1;

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public virtual void Reset()
        {
            lock (this._syncRoot)
            {
                this.SequenceIndex = -1;
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <param name="item">The current item when advancing was successful.</param>
        /// <returns><c>true</c> whilst there are items within the collection; <c>false</c> if the collection has no items.</returns>
        public bool TryMoveNext(out AudioFileInfo item)
        {
            lock (this._syncRoot)
            {
                if (this.Playlist.Count == 0)
                {
                    item = null;
                    return false;
                }

                this.SequenceIndex++;
                if (this.SequenceIndex < 0
                    || this.SequenceIndex == this.Playlist.Count)
                {
                    this.Reset();
                    this.SequenceIndex++;
                }

                try
                {
                    item = this.Playlist[this.CurrentIndex];
                    return true;
                }
                catch (IndexOutOfRangeException)
                {
                    item = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="INotifyCollectionChanged.CollectionChanged"/> of the <see cref="Playlist"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (this._syncRoot)
            {
                // When we were at the last item, and remove an item, we must reset.
                if (e.Action == NotifyCollectionChangedAction.Remove
                    && this.SequenceIndex >= this.Playlist.Count)
                {
                    this.Reset();
                }
            }
        }
    }
}
