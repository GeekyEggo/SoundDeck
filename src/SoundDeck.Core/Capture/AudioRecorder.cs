namespace SoundDeck.Core.Capture
{
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using System;

    /// <summary>
    /// Provides audio capturing for an audio device.
    /// </summary>
    public sealed class AudioRecorder : IAudioRecorder
    {
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
        /// Gets or sets the audio capturer.
        /// </summary>
        private WasapiCapture Capture { get; set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        private MMDevice Device { get; }

        /// <summary>
        /// Gets or sets the file writer.
        /// </summary>
        private WaveFileWriter FileWriter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Stop();

                this.Capture?.Dispose();
                this.Capture = null;

                this.IsDisposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            this.Capture = this.Device.DataFlow == DataFlow.Capture ? new WasapiCapture(this.Device) : new WasapiLoopbackCapture(this.Device);
            this.Capture.DataAvailable += this.Capture_DataAvailable;
            this.Capture.RecordingStopped += this.Capture_RecordingStopped;

            this.FileWriter = new WaveFileWriter(@"C:\Temp\Test0001.wav", this.Capture.WaveFormat);

            this.Capture.StartRecording();
        }

        public void Stop()
            => this.Capture.StopRecording();

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            this.FileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            this.FileWriter.Dispose();
            this.FileWriter = null;

            this.Capture.Dispose();
            this.Capture = null;


        }
    }
}
