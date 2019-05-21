namespace SoundDeck.Core.Capture
{
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.IO;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a wave audio file writer, used for writing <see cref="Chunk" />, and normalizing audio levels.
    /// </summary>
    public sealed class WavWriter : IDisposable
    {
        /// <summary>
        /// The synchronize root, used to synchronize processes.
        /// </summary>
        private SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="WavWriter"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="waveFormat">The wave format.</param>
        public WavWriter(Chunk[] chunks, WaveFormat waveFormat)
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
            => this._syncRoot.Wait(() => this.Chunks = null);

        /// <summary>
        /// Saves the chunks asynchronously, returning the name of the file path.
        /// </summary>
        /// <param name="outputPath">The output directory path.</param>
        /// <returns>The file path.</returns>
        public Task<string> SaveAsync(string outputPath)
        {
            return this._syncRoot.WaitAsync(async () =>
            {
                // construct the path of the file, and write to the output
                var path = Path.Combine(outputPath, $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.wav");
                using (var writer = new WaveFileWriter(path, this.WaveFormat))
                {
                    foreach (var chunk in this.Chunks)
                    {
                        await writer.WriteAsync(chunk.Buffer, 0, chunk.BytesRecorded);
                    }
                }

                // determine if we need to re-write the audio file based on the desired output
                return this.NormalizeVolume || this.EncodeToMP3
                    ? this.WriteAudioFile(path)
                    : path;
            });
        }

        /// <summary>
        /// Writes the audio file from the specified path, applying changes based on the state of this instance.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The saved audio file path.</returns>
        private string WriteAudioFile(string path)
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
                    path = writer.Save(path);
                }

                File.Delete(tempPath);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempPath);
            }

            return path;
        }
    }
}
