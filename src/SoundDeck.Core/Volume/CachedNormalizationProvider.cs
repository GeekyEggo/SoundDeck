namespace SoundDeck.Core.Volume
{
    using System.Collections.Concurrent;
    using System.IO;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides a static helper class for getting the normalization levels of an audio file based on its full path.
    /// </summary>
    public class CachedNormalizationProvider : NormalizationProvider, INormalizationProvider
    {
        /// <summary>
        /// Gets the items in the cache.
        /// </summary>
        private ConcurrentDictionary<string, CacheEntry> Items { get; } = new ConcurrentDictionary<string, CacheEntry>();

        /// <summary>
        /// Gets the peak of the audio file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The peak as an absolute value of the byte.</returns>
        public override float GetPeak(IAudioFileReader reader)
        {
            var key = this.GetKey(reader);
            var entry = this.Items.GetOrAdd(key, _ => this.GetNewEntry(key, reader));

            return entry.Peak;
        }

        /// <summary>
        /// Gets the cache entry key.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The key.</returns>
        private string GetKey(IAudioFileReader reader)
            => this.GetKey(reader?.FileName);

        /// <summary>
        /// Gets the cache entry key.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The key.</returns>
        private string GetKey(string path)
            => path?.Replace('/', '\\');

        /// <summary>
        /// Gets a new cache entry for the specified reader.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="reader">The reader.</param>
        /// <returns>The cache entry.</returns>
        private CacheEntry GetNewEntry(string key, IAudioFileReader reader)
        {
            var peak = base.GetPeak(reader);
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(key),
                Filter = Path.GetFileName(key)
            };

            watcher.Changed += this.RemoveCacheEntry;
            watcher.Deleted += this.RemoveCacheEntry;
            watcher.Renamed += this.RenameCacheEntry;
            watcher.EnableRaisingEvents = true;

            return new CacheEntry(peak, watcher);
        }

        /// <summary>
        /// Moves the cache entry to a new key based on the new file name.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RenamedEventArgs"/> instance containing the event data.</param>
        private void RenameCacheEntry(object sender, RenamedEventArgs e)
        {
            if (this.Items.TryRemove(this.GetKey(e.OldFullPath), out var entry))
            {
                this.Items.TryAdd(this.GetKey(e.FullPath), entry);
            }
        }

        /// <summary>
        /// Removes the cache entry based on the file name.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void RemoveCacheEntry(object sender, FileSystemEventArgs e)
        {
            if (this.Items.TryRemove(this.GetKey(e.FullPath), out var entry))
            {
                entry.Watcher.Dispose();
            }
        }

        /// <summary>
        /// A cache entry.
        /// </summary>
        private struct CacheEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CacheEntry"/> struct.
            /// </summary>
            /// <param name="peak">The peak.</param>
            /// <param name="watcher">The watcher.</param>
            public CacheEntry(float peak, FileSystemWatcher watcher)
            {
                this.Peak = peak;
                this.Watcher = watcher;
            }

            /// <summary>
            /// Gets the peak, as an absolute value.
            /// </summary>
            public float Peak { get; }

            /// <summary>
            /// Gets the watcher.
            /// </summary>
            public FileSystemWatcher Watcher { get; }
        }
    }
}
