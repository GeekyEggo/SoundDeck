using System;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundDeck.Core.Extensions;
using SoundDeck.Core.IO;

namespace SoundDeck.Core.Capture
{
    public sealed class CircularAudioBuffer : IAudioBuffer
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        private TimeSpan _bufferDuration;

        public CircularAudioBuffer(MMDevice device, TimeSpan bufferDuration, ILogger<CircularAudioBuffer> logger)
        {
            this.Buffer = new CircularBuffer<byte>(1);

            this._bufferDuration = bufferDuration;
            this.Device = device;
            this.Logger = logger;

            this.StartRecording();
        }

        public TimeSpan BufferDuration
        {
            get => this._bufferDuration;
            set => this._bufferDuration = value;
        }

        public string DeviceId => this.Device.ID;

        private CircularBuffer<byte> Buffer { get; }
        private WasapiCapture Capture { get; set; }
        private MMDevice Device { get; }
        private bool IsDisposed { get; set; } = false;
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
                    this.Buffer.SetCapacity(0);

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
                this.Logger.LogTrace($"Last {settings.Duration.TotalSeconds} seconds of \"{this.Device.FriendlyName}\" saved to \"{path}\".");

                using (var writer = new AudioFileWriter(path, this.Capture.WaveFormat))
                {
                    writer.Settings = settings;

                    var count = this.Capture.WaveFormat.AverageBytesPerSecond * 30;
                    var buffer = new byte[1024 * 4];
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
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
            => this.Buffer.Write(e.Buffer, 0, e.BytesRecorded);

        /// <summary>
        /// Sets the <see cref="AudioBuffer.Capture"/> and starts recording.
        /// </summary>
        private void StartRecording()
        {
            this.Capture = this.Device.DataFlow == DataFlow.Capture ? new WasapiCapture(this.Device) : new WasapiLoopbackCapture(this.Device);
            this.Buffer.SetCapacity(this.Capture.WaveFormat.AverageBytesPerSecond * 30);

            this.Capture.DataAvailable += this.Capture_DataAvailable;
            this.Capture.StartRecording();
        }
    }
}
