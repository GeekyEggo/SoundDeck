namespace SoundDeck.Core.Capture
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;

    /// <summary>
    /// Provides an audio buffer designed to capture and save audio data.
    /// </summary>
    public sealed class AudioBuffer : IAudioBuffer
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBuffer" /> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public AudioBuffer(MMDevice device, TimeSpan bufferDuration, ILogger logger = null)
        {
            this.Device = device;
            this.Chunks = new ChunkCollection(bufferDuration, logger);
            this.Logger = logger;

            this.StartRecording();
        }

        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        public TimeSpan BufferDuration
        {
            get { return this.Chunks.BufferDuration; }
            set { this.Chunks.BufferDuration = value; }
        }

        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        public string DeviceId => this.Device.ID;

        /// <summary>
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// Gets the chunks of captured audio data.
        /// </summary>
        private IChunkCollection Chunks { get; }

        /// <summary>
        /// Gets the underlying audio device.
        /// </summary>
        private MMDevice Device { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                try
                {
                    _syncRoot.Wait();

                    this.Capture?.StopRecording();
                    this.Capture?.Dispose();
                    this.Chunks?.Dispose();

                    this.IsDisposed = true;
                }
                finally
                {
                    _syncRoot.Release();
                }
            }
        }

        /// <summary>
        /// Saves an audio file of the current buffer.
        /// </summary>
        /// <param name="settings">The settings containing information about how, and where to save the capture.</param>
        /// <returns>The file path.</returns>
        public async Task<string> SaveAsync(ISaveBufferSettings settings)
        {
            try
            {
                await _syncRoot.WaitAsync();

                // determine the name
                var path = settings.GetPath();
                var chunks = await this.Chunks.GetAsync(settings.Duration);

                using (var writer = new ChunkFileWriter(path, this.Capture.WaveFormat, chunks))
                {
                    writer.Settings = settings;
                    await writer.SaveAsync();
                }

                this.Logger?.LogInformation("Audio capture saved: {0}", path);
                return path;
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <summary>
        /// Restarts the audio buffer.
        /// </summary>
        public void Restart()
        {
            try
            {
                _syncRoot.Wait();

                // clear the previous capture
                if (this.Capture != null)
                {
                    this.Capture.DataAvailable -= this.Capture_DataAvailable;
                    this.Capture.StopRecording();
                    this.Capture.Dispose();
                }

                // start recording
                this.StartRecording();
            }
            finally
            {
                _syncRoot.Release();
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

        /// <summary>
        /// Sets the <see cref="AudioBuffer.Capture"/> and starts recording.
        /// </summary>
        private void StartRecording()
        {
            this.Capture = this.Device.DataFlow == DataFlow.Capture ? new WasapiCapture(this.Device) : new WasapiLoopbackCapture(this.Device);
            this.Capture.DataAvailable += this.Capture_DataAvailable;
            this.Capture.StartRecording();
        }
    }
}
