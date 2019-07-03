namespace SoundDeck.Core.IO
{
    using NAudio.Wave;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides audio file writing, including encoding and volume normalization.
    /// </summary>
    public class AudioFileWriter : IDisposable
    {
        /// <summary>
        /// The temporary extension.
        /// </summary>
        private const string TEMP_EXTENSION = ".tmp";

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileWriter"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="format">The format.</param>
        public AudioFileWriter(string filename, WaveFormat format)
        {
            this.Writer = new WaveFileWriter(filename, format);
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public IAudioFileWriterSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a buffer has been written.
        /// </summary>
        private bool IsBufferWriten { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets or sets the writer.
        /// </summary>
        private WaveFileWriter Writer { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            // free up the original writer, and encode where required
            this.Dispose(true);
            if (this.IsBufferWriten && (this.Settings.NormalizeVolume || this.Settings.EncodeToMP3))
            {
                this.EncodeAudioFile();
            }

            this.IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Saves the written bytes, and applies encoding and volume normalization, where applicable; this will also dispose of the internal writer and will prevent any further writing.
        /// </summary>
        /// <returns>The task of saving the audio file.</returns>
        public virtual Task SaveAsync()
        {
            // close the original writer, to allow for encoding where required
            this.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes the buffer asynchronously, and flushes causing the underlying stream to be written.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>The task of writing the bytes.</returns>
        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            await this.Writer.WriteAsync(buffer, offset, count);
            await this.Writer.FlushAsync();

            this.IsBufferWriten = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
            => this.Writer.Dispose();

        /// <summary>
        /// Writes the audio file from the specified path, applying changes based on the state of this instance.
        /// </summary>
        /// <returns>The saved audio file path.</returns>
        private void EncodeAudioFile()
        {
            try
            {
                // move the current file to a temporary location
                File.Move(this.Writer.Filename, this.Writer.Filename + TEMP_EXTENSION);

                // re-write and the file
                using (var reader = new AudioFileReader(this.Writer.Filename + TEMP_EXTENSION))
                using (var writer = new AudioFileEncoder(reader))
                {
                    writer.Settings = this.Settings;
                    writer.Save(this.Writer.Filename);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(this.Writer.Filename + TEMP_EXTENSION);
            }
        }
    }
}
