namespace SoundDeck.Core.Playback
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides a playlist for a collection of audio files.
    /// </summary>
    public class Playlist : IEnumerator<string>
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        private static readonly Random Rnd = new Random();

        /// <summary>
        /// The synchronization root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Playlist"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public Playlist(IPlaylistOptions options)
        {
            this.SetOptions(options);
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public string Current => this.OrderedItems[this.CurrentIndex];

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Gets the number of items within the playlist.
        /// </summary>
        public int Length => this.OrderedItems.Length;

        /// <summary>
        /// Gets or sets the playback order.
        /// </summary>
        private PlaylistOrderType Order { get; set; }

        /// <summary>
        /// Gets or sets the index of the current.
        /// </summary>
        private int CurrentIndex { get; set; }

        /// <summary>
        /// Gets or sets the items; these are the ordered <see cref="OriginalItems"/>.
        /// </summary>
        private string[] OrderedItems { get; set; }

        /// <summary>
        /// Gets or sets the original items.
        /// </summary>
        private string[] OriginalItems { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.OrderedItems = null;
            this.OriginalItems = null;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            lock (this._syncRoot)
            {
                if (this.OrderedItems.Length == 0)
                {
                    return false;
                }

                this.CurrentIndex++;
                if (this.CurrentIndex < 0 || this.CurrentIndex == this.OrderedItems.Length)
                {
                    this.CurrentIndex = 0;
                }

                return true;
            }
        }

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="options">The options.</param>
        public void SetOptions(IPlaylistOptions options)
        {
            lock (this._syncRoot)
            {
                // set the files
                if (this.TrySetFiles(options.Files))
                {
                    this.Reset();
                }

                // refresh the order if its changed
                if (options.Order != this.Order)
                {
                    this.RefreshOrder();
                    this.Reset();
                }

                this.Order = options.Order;
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            lock (this._syncRoot)
            {
                this.CurrentIndex = -1;
            }
        }

        /// <summary>
        /// Refreshes the order.
        /// </summary>
        private void RefreshOrder()
        {
            if (this.Order == PlaylistOrderType.Random)
            {
                this.OrderedItems = this.OriginalItems.OrderBy(_ => Rnd.Next()).ToArray();
            }
            else
            {
                this.OrderedItems = this.OriginalItems;
            }
        }

        /// <summary>
        /// Tries to set <see cref="OriginalItems"/>, based on the equality of <paramref name="newFiles"/>.
        /// </summary>
        /// <param name="newFiles">The new files.</param>
        /// <returns><c>true</c> when the files were updated; otherwise <c>false</c>.</returns>
        private bool TrySetFiles(string[] newFiles)
        {
            newFiles = newFiles ?? new string[0];
            if (this.OriginalItems?.SequenceEqual(newFiles) == true)
            {
                return false;
            }

            this.OriginalItems = newFiles;
            this.RefreshOrder();

            return true;
        }
    }
}
