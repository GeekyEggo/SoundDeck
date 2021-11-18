namespace SoundDeck.Core.Capture
{
    using System;
    using System.Threading.Tasks;
    using NAudio.Wave;

    /// <summary>
    /// Provides a gated audio recorder, whereby recording only starts after <see cref="GateThreshold"/> has been met.
    /// </summary>
    [Obsolete("Requires further testing", true)]
    public class GatedAudioRecorder : AudioRecorder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GatedAudioRecorder"/> class.
        /// </summary>
        /// <param name="deviceId">The device.</param>
        private GatedAudioRecorder(IAudioDevice device)
            : base(device, null)
        {
        }

        /// <summary>
        /// Gets or sets the gate threshold; the gate remains open until the recording is written.
        /// </summary>
        public int GateThreshold { get; set; } = -50;

        /// <summary>
        /// Gets or sets a value indicating whether this instance's gate open.
        /// </summary>
        private bool IsGateOpen { get; set; } = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.IsGateOpen = false;
        }

        /// <summary>
        /// Handles the <see cref="WasapiCapture.DataAvailable"/> event of the <see cref="Capture"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WaveInEventArgs"/> instance containing the event data.</param>
        protected override void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.TryRead(e, out var buffer, out var length))
            {
                Task.WaitAll(this.FileWriter.WriteAsync(buffer, 0, length));
            }
        }

        /// <summary>
        /// Attempts to start listening to the buffer supplied by <paramref name="e"/> based on the amplitude vs <see cref="GateThreshold"/>.
        /// </summary>
        /// <param name="e">The <see cref="WaveInEventArgs"/> instance containing the event data.</param>
        /// <param name="buffer">The resulting buffer.</param>
        /// <param name="length">The resulting buffer length.</param>
        /// <returns><c>true</c> when the read was read, otherwise <c>false</c>.</returns>
        private bool TryRead(WaveInEventArgs e, out byte[] buffer, out int length)
        {
            length = e.BytesRecorded;
            buffer = e.Buffer;

            // gate open, return the supplied buffer
            if (this.IsGateOpen)
            {
                return true;
            }

            // determine if any of the samples trigger the gate
            for (var offset = 0; offset < e.BytesRecorded; offset += 4)
            {
                var sample = BitConverter.ToSingle(e.Buffer, offset);
                if (this.CanOpenGate(sample))
                {
                    // set the resulting buffer from the offset, and open the gate for future captures
                    length = e.BytesRecorded - offset;
                    buffer = new byte[length];
                    Buffer.BlockCopy(e.Buffer, offset, buffer, 0, buffer.Length);

                    this.IsGateOpen = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instances gate can open based on the supplied amplitude vs <see cref="GateThreshold"/>.
        /// </summary>
        /// <param name="amplitude">The amplitude.</param>
        /// <returns><c>true</c> when the gate can open; otherwise <c>false</c></returns>
        private bool CanOpenGate(float amplitude)
        {
            var dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB > this.GateThreshold;
        }
    }
}
