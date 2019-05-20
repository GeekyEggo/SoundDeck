﻿namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio buffer designed to capture and save audio data.
    /// </summary>
    public sealed class AudioBuffer : IAudioBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBuffer" /> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public AudioBuffer(MMDevice device, TimeSpan bufferDuration, ILogger logger = null)
        {
            this.Chunks = new ChunkCollection(bufferDuration, logger);
            this.Logger = logger;

            // initialize the capture
            this.Capture = device.DataFlow == DataFlow.Capture ? new WasapiCapture(device) : new WasapiLoopbackCapture(device);
            this.Capture.DataAvailable += this.Capture_DataAvailable;
            this.Capture.StartRecording();
        }

        /// <summary>
        /// Gets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; }

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
        public async Task<string> SaveAsync(TimeSpan duration, string outputPath)
        {
            var chunks = await this.Chunks.GetAsync(duration);
            using (var writer = new WavFileWriter(chunks, outputPath, this.Capture.WaveFormat))
            {
                var path = await writer.SaveAsync();
                this.Logger?.LogInformation("Audio capture saved: {0}", path); 

                return path;
            }
        }

        /// <summary>
        /// Handles the <see cref="WasapiCapture.DataAvailable"/> event of <see cref="Capture"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WaveInEventArgs"/> instance containing the event data.</param>
        private async void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            var chunk = new Chunk(e);
            await this.Chunks.AddAsync(chunk);
        }
    }
}
