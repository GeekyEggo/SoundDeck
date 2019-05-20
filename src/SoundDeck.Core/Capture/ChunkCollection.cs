namespace SoundDeck.Core
{
    using Extensions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a collection of <see cref="Chunk"/> data, containing captured audio.
    /// </summary>
    public sealed class ChunkCollection : IDisposable
    {
        /// <summary>
        /// The synchronize root, used to synchronize access.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkCollection"/> class.
        /// </summary>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public ChunkCollection(TimeSpan bufferDuration, ILogger logger = null)
        {
            this.Logger = logger;
            Task.Run(() => this.FlushAsync(bufferDuration));
        }

        /// <summary>
        /// Gets or sets the flush delay.
        /// </summary>
        public TimeSpan FlushDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets the data.
        /// </summary>
        private LinkedList<Chunk> Data { get; } = new LinkedList<Chunk>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Adds the chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The chunk of data.</param>
        /// <returns>The task of adding the chunk.</returns>
        public Task AddAsync(Chunk chunk)
        {
            return _syncRoot.WaitAsync(() =>
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException($"Unable to add {nameof(Chunk)}, the {nameof(ChunkCollection)} has been disposed.");
                }

                this.Data.AddLast(chunk);
            });
        }

        /// <summary>
        /// Gets the chunks of data from with the last <paramref name="duration"/> time span asynchronously.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>The chunks of data.</returns>
        public Task<Chunk[]> GetAsync(TimeSpan duration)
        {
            return _syncRoot.WaitAsync(() =>
            {
                var threshold = DateTime.UtcNow.Subtract(duration);
                return this.Data.Where(c => c.DateTime >= threshold).ToArray();
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _syncRoot.Wait(() =>
            {
                this.Data.Clear();
                this.IsDisposed = true;
            });
        }

        /// <summary>
        /// Flushes the <see cref="Data"/> asynchronously, ensuring minimal memory footprint.
        /// </summary>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <returns>The task of flushing the chunks of data.</returns>
        private async Task FlushAsync(TimeSpan bufferDuration)
        {
            await Task.Delay(bufferDuration);
            while (!this.IsDisposed)
            {
                await Task.Delay(this.FlushDelay);
                this.Logger.LogTrace("Flushing Chunk Collection");

                var threshold = DateTime.UtcNow.Subtract(bufferDuration);
                while (this.Data.Count > 0 && this.Data.First.Value.DateTime < threshold)
                {
                    this.Data.RemoveFirst();
                }
            }
        }
    }
}
