namespace SoundDeck.Core.Capture
{
    using Microsoft.Extensions.Logging;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.IO;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a <see cref="ChunkCollection"/> that compresses data as it is saved.
    /// </summary>
    public class CompressedChunkCollection : ChunkCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedChunkCollection"/> class.
        /// </summary>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public CompressedChunkCollection(TimeSpan bufferDuration, ILogger logger = null)
            : base(bufferDuration, logger)
        {
        }

        /// <summary>
        /// Adds the chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The chunk of data.</param>
        /// <returns>The task of adding the chunk.</returns>
        public override Task AddAsync(Chunk chunk)
        {
            chunk.Buffer = Compressor.Compress(chunk.Buffer);
            return base.AddAsync(chunk);
        }

        /// <summary>
        /// Gets the chunks of data from with the last <paramref name="duration" /> time span asynchronously.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>The chunks of data.</returns>
        public override async Task<Chunk[]> GetAsync(TimeSpan duration)
        {
            var chunks = await base.GetAsync(duration);
            var result = new Chunk[chunks.Length];

            chunks.ForEach((c, i) => result[i] = new Chunk(Compressor.Decompress(c.Buffer), c.BytesRecorded, c.DateTime));

            return result;
        }
    }
}
