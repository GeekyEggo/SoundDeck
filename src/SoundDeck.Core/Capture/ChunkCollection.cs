namespace SoundDeck.Core.Capture
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
    public class ChunkCollection : IChunkCollection
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
            this.BufferDuration = bufferDuration;
            this.Logger = logger;

            Task.Run(() => this.FlushAsync());
        }

        /// <summary>
        /// Gets or sets the flush delay.
        /// </summary>
        public TimeSpan FlushDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        public TimeSpan BufferDuration { get; set; }

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
        public virtual async Task AddAsync(Chunk chunk)
        {
            try
            {
                await _syncRoot.WaitAsync();
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException($"Unable to add {nameof(Chunk)}, the {nameof(ChunkCollection)} has been disposed.");
                }

                this.Data.AddLast(chunk);
            }
            finally
            {
                _syncRoot.Release();
            };
        }

        /// <summary>
        /// Gets the chunks of data from with the last <paramref name="duration"/> time span asynchronously.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>The chunks of data.</returns>
        public virtual async Task<Chunk[]> GetAsync(TimeSpan duration)
        {
            try
            {
                await _syncRoot.WaitAsync();

                var threshold = DateTime.UtcNow.Subtract(duration);
                return this.Data.Where(c => c.DateTime >= threshold).ToArray();
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                _syncRoot.Wait();
                if (!this.IsDisposed)
                {
                    this.Data.Clear();
                    this.IsDisposed = true;
                }
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <summary>
        /// Flushes the <see cref="Data"/> asynchronously, ensuring minimal memory footprint.
        /// </summary>
        /// <returns>The task of flushing the chunks of data.</returns>
        private async Task FlushAsync()
        {
            await Task.Delay(this.BufferDuration);
            while (!this.IsDisposed)
            {
                await Task.Delay(this.FlushDelay);
                this.Logger.LogTrace("Flushing Chunk Collection");

                // add an additional buffer of 5 seconds
                var threshold = DateTime.UtcNow.Subtract(this.BufferDuration).Subtract(TimeSpan.FromSeconds(5));
                while (this.Data.Count > 0 && this.Data.First.Value.DateTime < threshold)
                {
                    this.Data.RemoveFirst();
                }
            }
        }
    }
}
