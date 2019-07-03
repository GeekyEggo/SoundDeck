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
        /// <param name="device">The device.</param>
        public AudioRecorder(MMDevice device)
        {
            this.Device = device;
        }

        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        public string DeviceId => this.Device.ID;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public ISaveAudioSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// The capturing completion source.
        /// </summary>
        private TaskCompletionSource<bool> CapturingCompletionSource;

        /// <summary>
        /// Gets the device.
        /// </summary>
        private MMDevice Device { get; }

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

            this.FileWriter?.Dispose();
            this.FileWriter = null;

            this.CapturingCompletionSource?.SetResult(!disposing);
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
                this.Capture = this.Device.DataFlow == DataFlow.Capture ? new WasapiCapture(this.Device) : new WasapiLoopbackCapture(this.Device);
                this.Capture.DataAvailable += this.Capture_DataAvailable;
                this.Capture.RecordingStopped += this.Capture_RecordingStopped;

                // initialize the writer, and start recording
                this.FileWriter = new AudioFileWriter(this.Settings.GetPath(), this.Capture.WaveFormat)
                {
                    Settings = this.Settings
                };

                this.CapturingCompletionSource = new TaskCompletionSource<bool>();
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
        /// <returns>The task of starting.</returns>
        public async Task StopAsync()
        {
            try
            {
                // when there is no capturing completion source, assume we arent recording
                await this._syncRoot.WaitAsync();
                if (this.CapturingCompletionSource == null)
                {
                    return;
                }

                // stop recording, and awaiting actual stop
                this.Capture.StopRecording();
                await this.CapturingCompletionSource.Task;
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
    }
}
