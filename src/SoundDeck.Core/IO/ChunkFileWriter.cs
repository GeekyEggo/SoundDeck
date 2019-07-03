namespace SoundDeck.Core.Capture
{
    using NAudio.Wave;
    using SoundDeck.Core.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a wave audio file writer, used for writing <see cref="Chunk" />, and normalizing audio levels.
    /// </summary>
    public sealed class ChunkFileWriter : AudioFileWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkFileWriter"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="format">The format.</param>
        /// <param name="chunks">The chunks.</param>
        public ChunkFileWriter(string filename, WaveFormat format, Chunk[] chunks)
            : base(filename, format)
        {
            this.Chunks = chunks;
        }

        /// <summary>
        /// Gets the chunks.
        /// </summary>
        public Chunk[] Chunks { get; private set; }

        /// <summary>
        /// Saves the written bytes, and applies encoding and volume normalization, where applicable; this will also dispose of the internal writer and will prevent any further writing.
        /// </summary>
        /// <returns>The task of saving the audio file.</returns>
        public override async Task SaveAsync()
        {
            foreach (var chunk in this.Chunks)
            {
                await this.WriteAsync(chunk.Buffer, 0, chunk.BytesRecorded);
            }

            await base.SaveAsync();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.Chunks = null;
        }
    }
}
