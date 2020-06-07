namespace SoundDeck.Core.Capture
{
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.IO;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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
        /// <param name="deviceId">The device identifier.</param>
        public AudioRecorder(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public ISaveAudioSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// The capturing completion source containing the location of the file where the audio was saved to.
        /// </summary>
        private TaskCompletionSource<string> CapturingCompletionSource;

        /// <summary>
        /// Gets or sets the file writer.
        /// </summary>
        private AudioFileWriter FileWriter { get; set; }

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

            this.Capture?.Dispose();
            this.Capture = null;

            var filename = this.FileWriter?.Filename;
            this.FileWriter?.Dispose();
            this.FileWriter = null;

            this.CapturingCompletionSource?.SetResult(filename);
            this.CapturingCompletionSource = null;
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

                // when there is already a completion source, we assume we are already capturing
                if (this.CapturingCompletionSource != null)
                {
                    return;
                }

                // set the capture information
                var device = this.GetDevice();
                this.Capture = device.DataFlow == DataFlow.Capture ? new WasapiCapture(device) : new WasapiLoopbackCapture(device);
                this.Capture.DataAvailable += this.Capture_DataAvailable;
                this.Capture.RecordingStopped += this.Capture_RecordingStopped;

                // initialize the writer, and start recording
                this.FileWriter = new AudioFileWriter(this.Settings.GetPath(), this.Capture.WaveFormat)
                {
                    Settings = this.Settings
                };

                this.CapturingCompletionSource = new TaskCompletionSource<string>();
                this.Capture.StartRecording();
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
                // when there is no capturing completion source, assume we arent recording
                this._syncRoot.Wait();
                if (this.CapturingCompletionSource == null)
                {
                    return null;
                }

                // stop recording, and awaiting actual stop
                this.Capture.StopRecording();
                return this.CapturingCompletionSource.Task;
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
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
            => Task.WaitAll(this.FileWriter.WriteAsync(e.Buffer, 0, e.BytesRecorded));

        /// <summary>
        /// Handles the <see cref="WasapiCapture.RecordingStopped"/> event of the <see cref="Capture"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoppedEventArgs"/> instance containing the event data.</param>
        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
            => this.Dispose(false);

        /// <summary>
        /// Gets the device associated with the audio recorder.
        /// </summary>
        /// <returns>The device.</returns>
        private MMDevice GetDevice()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                return enumerator.GetDevice(this.DeviceId);
            }
        }
    }
}
