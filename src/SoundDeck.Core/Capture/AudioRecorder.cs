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
    /// Provides audio capturing for an audio device.
    /// </summary>
    public class AudioRecorder : IAudioRecorder
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRecorder" /> class.
        /// </summary>
        /// <param name="deviceId">The device.</param>
        /// <param name="logger">The logger.</param>
        public AudioRecorder(IAudioDevice device, ILogger<AudioRecorder> logger)
        {
            device.DeviceChanged += (_, __) => this.OnCaptureDeviceChanged();

            this.Device = device;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the audio device.
        /// </summary>
        public IAudioDevice Device { get; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public ISaveAudioSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the file writer.
        /// </summary>
        protected AudioFileWriter FileWriter { get; set; }

        /// <summary>
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// The capturing completion source containing the location of the file where the audio was saved to.
        /// </summary>
        private TaskCompletionSource<string> SavedFilePath;

        /// <summary>
        /// Gets a value indicating whether the audio recorder is active.
        /// </summary>
        private bool IsRecording { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Task.WaitAll(this.StopAsync());
            }
            else
            {
                this.Capture?.StopRecording();
                this.Capture = null;

                var filename = this.FileWriter?.Filename;
                this.FileWriter?.Dispose();
                this.FileWriter = null;

                this.SavedFilePath?.SetResult(filename);
                this.SavedFilePath = null;

                this.Logger.LogTrace($"Recording of \"{this.Device.GetMMDevice()?.FriendlyName}\" saved to \"{filename}\".");
            }
        }

        /// <summary>
        /// Starts capturing audio asynchronously.
        /// </summary>
        /// <returns>The task of starting.</returns>
        public async Task StartAsync()
        {
            try
            {
                await this._syncRoot.WaitAsync();

                if (!this.IsRecording
                    && this.SavedFilePath == null)
                {
                    // Initialize the capture and file writer.
                    this.Capture = this.GetCapture();
                    this.FileWriter = new AudioFileWriter(this.Settings.GetPath(), this.Capture.WaveFormat)
                    {
                        Settings = this.Settings
                    };

                    // Start recording.
                    this.SavedFilePath = new TaskCompletionSource<string>();

                    this.IsRecording = true;
                    this.Capture.StartRecording();
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Stops capturing audio asynchronously.
        /// </summary>
        /// <returns>The file name of where the audio was saved.</returns>
        public Task<string> StopAsync()
        {
            try
            {
                // When there is no capturing completion source, assume we arent recording.
                this._syncRoot.Wait();
                if (!this.IsRecording)
                {
                    return Task.FromResult(string.Empty);
                }

                // Stop recording, and await actual stop.
                this.Capture.RecordingStopped += (_, __) => this.Dispose(false);
                this.Capture.StopRecording();
                this.IsRecording = false;

                return this.SavedFilePath.Task;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Handles the <see cref="WasapiCapture.DataAvailable"/> event of the <see cref="Capture"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WaveInEventArgs"/> instance containing the event data.</param>
        protected virtual void Capture_DataAvailable(object sender, WaveInEventArgs e)
            => Task.WaitAll(this.FileWriter.WriteAsync(e.Buffer, 0, e.BytesRecorded));

        /// <summary>
        /// Updates the active <see cref="Capture"/> device.
        /// </summary>
        private void OnCaptureDeviceChanged()
        {
            try
            {
                this._syncRoot.Wait();

                // We should only update the capture when there is an active recording.
                if (this.IsRecording)
                {
                    if (this.Capture != null)
                    {
                        this.Capture.DataAvailable -= this.Capture_DataAvailable;
                        this.Capture.StopRecording();
                    }

                    this.Capture = this.GetCapture();
                    this.Capture.StartRecording();
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Gets the <see cref="WasapiCapture"/> associated with the <see cref="Device"/>.
        /// </summary>
        /// <returns>The <see cref="WasapiCapture"/>.</returns>
        private WasapiCapture GetCapture()
        {
            var capture = this.Device.Flow == DataFlow.Capture ? new WasapiCapture(this.Device.GetMMDevice()) : new WasapiLoopbackCapture(this.Device.GetMMDevice());
            capture.DataAvailable += this.Capture_DataAvailable;
            capture.RecordingStopped += (_, __) =>
            {
                capture.DataAvailable -= this.Capture_DataAvailable;
                capture.Dispose();
            };

            return capture;
        }
    }
}
