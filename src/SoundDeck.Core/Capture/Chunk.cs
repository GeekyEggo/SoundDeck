namespace SoundDeck.Core
{
    using NAudio.Wave;
    using System;

    /// <summary>
    /// Provides information about capture byte data, and when it was captured.
    /// </summary>
    public class Chunk : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chunk"/> class.
        /// </summary>
        /// <param name="e">The <see cref="WaveInEventArgs"/> instance containing the event data.</param>
        internal Chunk(WaveInEventArgs e)
        {
            this.Buffer = new byte[e.Buffer.Length];
            this.BytesRecorded = e.BytesRecorded;

            e.Buffer.CopyTo(this.Buffer, 0);
        }

        /// <summary>
        /// Gets or sets the buffer.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Gets or sets the bytes recorded.
        /// </summary>
        public int BytesRecorded { get; set; }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        public DateTime DateTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Buffer = null;
            this.BytesRecorded = 0;
        }
    }
}
