namespace SoundDeck.Core.Playback
{
    using System;
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Provides a playlist for a collection of audio files.
    /// </summary>
    public class Playlist : IPlaylist
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
        /// Private member field for <see cref="OriginalItems"/>.
        /// </summary>
        private AudioFileInfo[] _originalItems = new AudioFileInfo[0];

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
        public AudioFileInfo Current => this.OrderedItems[this.CurrentIndex];

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Gets the number of items within the playlist.
        /// </summary>
        public int Count => this.OrderedItems.Length;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IEnumerator{T}.Current" /> is the last.
        /// </summary>
        public bool IsLast
        {
            get
            {
                lock (this._syncRoot)
                {
                    return this.CurrentIndex >= (this.OrderedItems.Length - 1);
                }
            }
        }

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
        private AudioFileInfo[] OrderedItems { get; set; }

        /// <summary>
        /// Gets or sets the original items.
        /// </summary>
        private AudioFileInfo[] OriginalItems
        {
            get => this._originalItems;
            set
            {
                void SetOriginalItems()
                {
                    this._originalItems = value;
                    this.RefreshOrder();
                    this.Reset();
                }

                lock (this._syncRoot)
                {
                    // When both values are null, do nothing.
                    if (this._originalItems == null
                        && value == null)
                    {
                        return;
                    }

                    // When the collection lengths differ, set the items again.
                    if (this._originalItems?.Length != value?.Length)
                    {
                        SetOriginalItems();
                        return;
                    }

                    // Whilst the items are sequentially the same, set their volumes; otherwise simply reset the items.
                    for (var i = 0; i < this._originalItems.Length; i++)
                    {
                        if (this._originalItems[i].Path == value[i].Path)
                        {
                            this._originalItems[i].Volume = value[i].Volume;
                        }
                        else
                        {
                            SetOriginalItems();
                        }
                    }
                }
            }
        }

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
                this.OriginalItems = options.Files;

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
    }
}
