namespace SoundDeck.Core.Playback.Playlists
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Provides an audio collection of files that represent a playlist.
    /// </summary>
    public class AudioFileCollection : IPlaylist
    {
        /// <summary>
        /// Occurs when the <see cref="Files"/> changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public AudioFileCollection(IEnumerable<AudioFileInfo> collection = null)
        {
            this.Files = collection?.ToList() ?? new List<AudioFileInfo>();
        }

        /// <summary>
        /// Gets the <see cref="AudioFileInfo"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The audio file at the index.</returns>
        public AudioFileInfo this[int index] => this.Files[index];

        /// <summary>
        /// Gets the count of files.
        /// </summary>
        public int Count => this.Files.Count;

        /// <summary>
        /// Private member field for <see cref="Files"/>.
        /// </summary>
        private List<AudioFileInfo> Files { get; } = new List<AudioFileInfo>();

        /// <summary>
        /// Adds the collection of files to the playlist.
        /// </summary>
        /// <param name="paths">The file paths to add.</param>
        public void AddRange(IEnumerable<string> paths)
        {
            if (paths != null)
            {
                this.Files.AddRange(paths.Select(AudioFileInfo.FromPath));
                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, paths));
            }
        }

        /// <summary>
        /// Moves the file from the specified <paramref name="oldIndex" /> to the <paramref name="newIndex" />.
        /// </summary>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public void Move(int oldIndex, int newIndex)
        {
            var item = this.Files[oldIndex];

            this.Files.RemoveAt(oldIndex);
            this.Files.Insert(newIndex, item);

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        /// <summary>
        /// Removes the file at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            var item = this.Files[index];

            this.Files.RemoveAt(index);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<AudioFileInfo> GetEnumerator()
            => this.Files.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
