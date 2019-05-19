namespace SoundDeck.Core
{
    using System;

    /// <summary>
    /// Provides information about capture byte data, and when it was captured.
    /// </summary>
    public class Chunk : IDisposable
    {
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
