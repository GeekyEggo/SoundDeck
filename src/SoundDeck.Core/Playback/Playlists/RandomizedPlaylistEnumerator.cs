namespace SoundDeck.Core.Playback.Playlists
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Provides a randomized <see cref="IPlaylistEnumerator"/>.
    /// </summary>
    public class RandomizedPlaylistEnumerator : PlaylistEnumerator
    {
        /// <summary>
        /// The random generator.
        /// </summary>
        private static readonly Random Rnd = new Random();

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizedPlaylistEnumerator"/> class.
        /// </summary>
        /// <param name="playlist">The playlist.</param>
        public RandomizedPlaylistEnumerator(IPlaylist playlist)
            : base(playlist)
        {
            this.RandomizeIndexes();
        }

        /// <summary>
        /// Gets the index of the current item.
        /// </summary>
        public override int CurrentIndex
        {
            get
            {
                try
                {
                    return this.Indexes[this.SequenceIndex];
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the randomized indexes.
        /// </summary>
        private List<int> Indexes { get; set; }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            lock (this._syncRoot)
            {
                this.RandomizeIndexes();
            }
        }

        /// <summary>
        /// Handles the <see cref="INotifyCollectionChanged.CollectionChanged" /> of the <see cref="Playlist" />.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.Playlist_CollectionChanged(sender, e);

            lock (this._syncRoot)
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                    || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    // When there are new items, or the items were reset, randomize the indexes.
                    this.RandomizeIndexes();
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    // When an item was removed, remove its index from the random list.
                    this.Indexes.RemoveAll(i => i == e.OldStartingIndex);
                }
            }
        }

        /// <summary>
        /// Randomizes the indexes of items within the playlist and persists the order within <see cref="Indexes"/>.
        /// </summary>
        private void RandomizeIndexes()
        {
            lock (this._syncRoot)
            {
                this.Indexes = Enumerable.Range(0, this.Playlist.Count)
                    .Select((index, _) => index)
                    .OrderBy(_ => Rnd.Next())
                    .ToList();
            }
        }
    }
}
