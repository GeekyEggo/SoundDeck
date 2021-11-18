namespace SoundDeck.Core.Capture
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.IO;

    /// <summary>
    /// Provides a <see cref="IAudioBuffer"/> that utilizes a <see cref="CircularBuffer{byte}"/>.
    /// </summary>
    public sealed class CircularAudioBuffer : IAudioBuffer
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="BufferDuration"/>.
        /// </summary>
        private TimeSpan _bufferDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularAudioBuffer"/> class.
        /// </summary>
        /// <param name="device">The device to capture.</param>
        /// <param name="bufferDuration">Initial duration of the buffer.</param>
        /// <param name="logger">The logger.</param>
        public CircularAudioBuffer(IAudioDevice device, TimeSpan bufferDuration, ILogger<CircularAudioBuffer> logger)
        {
            this.Buffer = new CircularBuffer<byte>(1);

            this._bufferDuration = bufferDuration;
            this.Device = device;
            this.Logger = logger;

            this.StartRecording();
        }

        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        public TimeSpan BufferDuration
        {
            get => this._bufferDuration;
            set
            {
                try
                {
                    this._syncRoot.Wait();

                    this._bufferDuration = value;
                    this.Buffer.SetCapacity(this.Capture.WaveFormat.AverageBytesPerSecond * (int)this._bufferDuration.TotalSeconds);
                }
                finally
                {
                    this._syncRoot.Release();
                }
            }
        }

        /// <summary>
        /// Gets the audio device.
        /// </summary>
        public IAudioDevice Device { get; }

        /// <summary>
        /// Gets the circular buffer responsible for storing the bytes that represent the audio clip.
        /// </summary>
        private CircularBuffer<byte> Buffer { get; }

        /// <summary>
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<CircularAudioBuffer> Logger { get; }

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
                    this.Buffer.SetCapacity(1);

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

                // Determine the save location, and log the recording.
                var path = settings.GetPath();
                this.Logger.LogTrace($"Last {settings.Duration.TotalSeconds} seconds of \"{this.Device.FriendlyName}\" saved to \"{path}\".");

                using (var writer = new AudioFileWriter(path, this.Capture.WaveFormat))
                {
                    writer.Settings = settings;

                    var buffer = new byte[1024 * 4];

                    // Continuously read the bytes from the buffer, and write them to the audio file writer.
                    var count = this.Capture.WaveFormat.AverageBytesPerSecond * (int)settings.Duration.TotalSeconds;
                    var read = 0;
                    var offset = 0;

                    while ((read = this.Buffer.Read(buffer, offset, buffer.Length)) > 0)
                    {
                        await writer.WriteAsync(buffer, 0, read);
                        offset += read;
                    }

                    await writer.SaveAsync();
                }

                return path;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to write clip.");
                throw;
            }
            finally
            {
                GC.Collect(2, GCCollectionMode.Forced);
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

                // Clear the previous capture.
                if (this.Capture != null)
                {
                    this.Capture.DataAvailable -= this.Capture_DataAvailable;
                    this.Capture.StopRecording();
                    this.Capture.Dispose();
                }

                // Start recording.
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
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
            => this.Buffer.Write(e.Buffer, 0, e.BytesRecorded);

        /// <summary>
        /// Sets the <see cref="AudioBuffer.Capture"/> and starts recording.
        /// </summary>
        private void StartRecording()
        {
            this.Capture = this.Device.Flow == DataFlow.Capture ? new WasapiCapture(this.Device.GetMMDevice()) : new WasapiLoopbackCapture(this.Device.GetMMDevice());
            this.Buffer.SetCapacity(this.Capture.WaveFormat.AverageBytesPerSecond * (int)this._bufferDuration.TotalSeconds);

            this.Capture.DataAvailable += this.Capture_DataAvailable;
            this.Capture.StartRecording();
        }
    }
}
