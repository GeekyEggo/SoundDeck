namespace SoundDeck.Core.Playback.Playlists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides an enumerator for a playlist.
    /// </summary>
    public class PlaylistEnumerator : IPlaylistEnumerator
    {
        /// <summary>
        /// The random seed.
        /// </summary>
        private static readonly Random Rnd = new Random();

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistEnumerator"/> class.
        /// </summary>
        /// <param name="files">The files.</param>
        public PlaylistEnumerator(IPlaylist playlist)
        {
            this.PlaylistSource = playlist;
            playlist.Changed += (_, __) => this.Bind();

            this.Bind();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPlaylist.Current" /> is the last item.
        /// </summary>
        public bool IsLast { get; }

        /// <summary>
        /// Gets or sets the index of the current file.
        /// </summary>
        private int CurrentIndex { get; set; }

        /// <summary>
        /// Gets the files associated with the enumerator.
        /// </summary>
        private IReadOnlyList<AudioFileInfo> Files { get; set; }

        /// <summary>
        /// Gets the playlist source.
        /// </summary>
        private IPlaylist PlaylistSource { get; }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <param name="item">The current item when advancing was successful.</param>
        /// <returns><c>true</c> whilst there are items within the collection; <c>false</c> if the collection has no items.</returns>
        public bool TryMoveNext(out AudioFileInfo item)
        {
            lock (this._syncRoot)
            {
                if (this.Files.Count == 0)
                {
                    item = null;
                    return false;
                }

                this.CurrentIndex++;
                if (this.CurrentIndex < 0
                    || this.CurrentIndex == this.Files.Count)
                {
                    this.CurrentIndex = 0;
                }

                item = this.Files[this.CurrentIndex];
                return true;
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
        /// Binds this instance to the <see cref="PlaylistSource"/>.
        /// </summary>
        private void Bind()
        {
            lock (this._syncRoot)
            {
                this.Reset();
                this.Files = this.PlaylistSource.Order == PlaybackOrderType.Sequential
                    ? this.Files = this.PlaylistSource.Files
                    : this.Files = this.PlaylistSource.Files.OrderBy(_ => Rnd.Next()).ToList();
            }
        }
    }
}
