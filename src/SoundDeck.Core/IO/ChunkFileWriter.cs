namespace SoundDeck.Core.Capture
{
    using NAudio.Wave;
    using SoundDeck.Core.IO;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a wave audio file writer, used for writing <see cref="Chunk" />, and normalizing audio levels.
    /// </summary>
    public sealed class ChunkFileWriter : IDisposable
    {
        /// <summary>
        /// The synchronize root, used to synchronize processes.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkFileWriter"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="waveFormat">The wave format.</param>
        public ChunkFileWriter(Chunk[] chunks, WaveFormat waveFormat)
        {
            this.Chunks = chunks;
            this.WaveFormat = waveFormat;
        }

        /// <summary>
        /// Gets the chunks.
        /// </summary>
        public Chunk[] Chunks { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to encode to MP3.
        /// </summary>
        public bool EncodeToMP3 { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to normalize the volume.
        /// </summary>
        public bool NormalizeVolume { get; set; } = false;

        /// <summary>
        /// Gets the wave format.
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this._syncRoot.Wait();
                this.Chunks = null;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Saves the chunks asynchronously.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The task of saving.</returns>
        public async Task SaveAsync(string path)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                using (var writer = new WaveFileWriter(path, this.WaveFormat))
                {
                    foreach (var chunk in this.Chunks)
                    {
                        await writer.WriteAsync(chunk.Buffer, 0, chunk.BytesRecorded);
                        await writer.FlushAsync();
                    }
                }

                // determine if we need to re-write the audio file based on the desired output
                if (this.NormalizeVolume || this.EncodeToMP3)
                {
                    this.WriteAudioFile(path);
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Writes the audio file from the specified path, applying changes based on the state of this instance.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The saved audio file path.</returns>
        private void WriteAudioFile(string path)
        {
            var tempPath = path + ".tmp";
            try
            {
                // move the current file to a temporary location
                File.Move(path, tempPath);

                // re-write and the file
                using (var reader = new AudioFileReader(tempPath))
                using (var writer = new AudioFileWriter(reader))
                {
                    writer.EncodeToMP3 = this.EncodeToMP3;
                    writer.NormalizeVolume = this.NormalizeVolume;
                    writer.Save(path);
                }

                File.Delete(tempPath);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempPath);
            }
        }
    }
}
