namespace SoundDeck.Core.Capture
{
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a wave audio file writer, used for writing <see cref="Chunk"/>, and normalizing audio levels.
    /// </summary>
    public sealed class WavFileWriter : IDisposable
    {
        /// <summary>
        /// The synchronize root, used to synchronize processes.
        /// </summary>
        private SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="WavFileWriter"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="waveFormat">The wave format.</param>
        public WavFileWriter(Chunk[] chunks, string outputPath, WaveFormat waveFormat)
        {
            this.Chunks = chunks;
            this.OutputPath = outputPath;
            this.WaveFormat = waveFormat;
        }

        /// <summary>
        /// Gets the chunks.
        /// </summary>
        public Chunk[] Chunks { get; private set; }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        public string OutputPath { get; }

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
        /// <param name="normalize">When set to <c>true</c> the volume of the audio file will be normalized.</param>
        /// <returns>The file path.</returns>
        public Task<string> SaveAsync(bool normalize = true)
        {
            return this._syncRoot.WaitAsync(async () =>
            {
                // construct the path of the file, and write to the output
                var path = Path.Combine(this.OutputPath, $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.wav");
                using (var writer = new WaveFileWriter(path, this.WaveFormat))
                {
                    foreach (var chunk in this.Chunks)
                    {
                        await writer.WriteAsync(chunk.Buffer, 0, chunk.BytesRecorded);
                    }
                }

                // determine if we should normalize the file
                if (normalize)
                {
                    this.Normalize(path);
                }

                return path;
            });
        }

        /// <summary>
        /// Normalizes the volume of the audio for the specified file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        private void Normalize(string path)
        {
            var tempNormalizedPath = path + ".tmp";
            try
            {
                using (var reader = new AudioFileReader(path))
                {
                    reader.Volume = this.GetVolume(reader);
                    WaveFileWriter.CreateWaveFile(tempNormalizedPath, reader);
                }

                // remove the original file, and replace with the normalized
                File.Delete(path);
                File.Move(tempNormalizedPath, path);
            }
            finally
            {
                // ensure we clean up if anything fails
                if (File.Exists(tempNormalizedPath))
                {
                    File.Delete(tempNormalizedPath);
                }
            }
        }

        /// <summary>
        /// Gets the maximum volume for the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The maximum volume.</returns>
        private float GetVolume(AudioFileReader reader)
        {
            float max = 0;

            var buffer = new float[reader.WaveFormat.SampleRate];
            int read;

            // determine the max volume
            do
            {
                read = reader.Read(buffer, 0, buffer.Length);
                for (int n = 0; n < read; n++)
                {
                    var abs = Math.Abs(buffer[n]);
                    if (abs > max) max = abs;
                }
            } while (read > 0);

            // rewind the reader and return the volume
            reader.Position = 0;
            return 1.0f / max;
        }
    }
}
