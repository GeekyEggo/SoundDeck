namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio buffer designed to capture and save audio data.
    /// </summary>
    public sealed class AudioBuffer : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBuffer"/> class.
        /// </summary>
        /// <param name="capture">The audio capturer.</param>
        /// <param name="outputFolder">The output folder.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public AudioBuffer(IAudioCapture capture, string outputFolder, TimeSpan bufferDuration, ILogger logger = null)
        {
            this.OutputFolder = outputFolder;
            this.Chunks = new ChunkCollection(bufferDuration, logger);
            this.Logger = logger;

            this.Capture = capture;
            this.Capture.DataAvailable += this.CaptureDataAvailable;
            this.Capture.Start();
        }        

        /// <summary>
        /// Gets the output folder.
        /// </summary>
        public string OutputFolder { get; }

        /// <summary>
        /// Gets the audio capturer.
        /// </summary>
        private IAudioCapture Capture { get; }

        /// <summary>
        /// Gets the chunks of captured audio data.
        /// </summary>
        private ChunkCollection Chunks { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Capture.Dispose();
            this.Chunks.Dispose();
        }

        /// <summary>
        /// Saves an audio file of the current buffer, for the specified duration.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>The file path.</returns>
        public async Task<string> SaveAsync(TimeSpan duration)
        {
            var chunks = await this.Chunks.GetAsync(duration);
            var path = Path.Combine(this.OutputFolder, $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}{this.Capture.FileWriter.Extension}");

            await this.Capture.FileWriter.WriteAsync(path, chunks);
            this.Logger?.LogInformation("Audio capture saved: {0}", path); 
            return path;
        }

        /// <summary>
        /// Handles the <see cref="IAudioCapture.DataAvailable"/> event for <see cref="Capture"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Chunk"/> instance containing the event data.</param>
        private async void CaptureDataAvailable(object sender, Chunk e)
            => await this.Chunks.AddAsync(e);
    }
}
