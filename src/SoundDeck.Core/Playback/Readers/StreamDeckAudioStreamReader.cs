namespace SoundDeck.Core.Playback.Readers
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a file stream reader for a .streamDeckAudio file.
    /// </summary>
    public class StreamDeckAudioStreamReader : FileStream
    {
        /// <summary>
        /// The byte conversion map.
        /// </summary>
        private static readonly byte[] ByteMap = new byte[256] { 94, 95, 92, 93, 90, 91, 88, 89, 86, 87, 84, 85, 82, 83, 80, 81, 78, 79, 76, 77, 74, 75, 72, 73, 70, 71, 68, 69, 66, 67, 64, 65, 126, 127, 124, 125, 122, 123, 120, 121, 118, 119, 116, 117, 114, 115, 112, 113, 110, 111, 108, 109, 106, 107, 104, 105, 102, 103, 100, 101, 98, 99, 96, 97, 30, 31, 28, 29, 26, 27, 24, 25, 22, 23, 20, 21, 18, 19, 16, 17, 14, 15, 12, 13, 10, 11, 8, 9, 6, 7, 4, 5, 2, 3, 0, 1, 62, 63, 60, 61, 58, 59, 56, 57, 54, 55, 52, 53, 50, 51, 48, 49, 46, 47, 44, 45, 42, 43, 40, 41, 38, 39, 36, 37, 34, 35, 32, 33, 222, 223, 220, 221, 218, 219, 216, 217, 214, 215, 212, 213, 210, 211, 208, 209, 206, 207, 204, 205, 202, 203, 200, 201, 198, 199, 196, 197, 194, 195, 192, 193, 254, 255, 252, 253, 250, 251, 248, 249, 246, 247, 244, 245, 242, 243, 240, 241, 238, 239, 236, 237, 234, 235, 232, 233, 230, 231, 228, 229, 226, 227, 224, 225, 158, 159, 156, 157, 154, 155, 152, 153, 150, 151, 148, 149, 146, 147, 144, 145, 142, 143, 140, 141, 138, 139, 136, 137, 134, 135, 132, 133, 130, 131, 128, 129, 190, 191, 188, 189, 186, 187, 184, 185, 182, 183, 180, 181, 178, 179, 176, 177, 174, 175, 172, 173, 170, 171, 168, 169, 166, 167, 164, 165, 162, 163, 160, 16 };

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDeckAudioStreamReader"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public StreamDeckAudioStreamReader(string fileName)
            : base(fileName, FileMode.Open)
        {
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base.Read(buffer, offset, count);
            for (var i = 0; i < read; i++)
            {
                buffer[i] = ByteMap[buffer[i]];
            }

            return read;
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult">TResult</paramref> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(this.Read(buffer, offset, count));
    }
}
