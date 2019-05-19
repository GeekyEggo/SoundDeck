using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;

namespace SoundDeck.Core.NAudio
{
    public class WasapiAudioCapture : IAudioCapture
    {
        //private const string SAMPLE_DEVICE_ID = "{0.0.1.00000000}.{7be9d233-fc82-4185-8fbf-c14484837ad7}";

        public WasapiAudioCapture(string deviceId = "")
        {
            this.Capture = this.GetCapture(deviceId);
            this.Capture.DataAvailable += this.Capture_DataAvailable;

            this.FileWriter = new WaveFileWriter
            {
                WaveFormat = this.Capture.WaveFormat
            };
        }

        public event EventHandler<Chunk> DataAvailable;

        private WasapiCapture Capture { get; }

        public IAudioFileWriter FileWriter { get; } = new WaveFileWriter();

        public void Dispose()
        {
            this.Capture.StopRecording();
            this.Capture.Dispose();
        }

        public void Start()
            => this.Capture.StartRecording();

        public void Stop()
            => this.Capture.StopRecording();

        private WasapiCapture GetCapture(string deviceId = "")
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return new WasapiLoopbackCapture();
            }

            try
            {
                var device = new MMDeviceEnumerator().GetDevice(deviceId);
                return device.DataFlow == DataFlow.Capture ? new WasapiCapture(device) : new WasapiLoopbackCapture(device);
            }
            catch
            {
                return new WasapiLoopbackCapture();
            }
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            var chunk = new Chunk
            {
                Buffer = new byte[e.Buffer.Length],
                BytesRecorded = e.BytesRecorded
            };

            e.Buffer.CopyTo(chunk.Buffer, 0);
            this.DataAvailable?.Invoke(this, chunk);
        }
    }
}
