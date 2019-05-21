namespace SoundDeck.Core.IO
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Provides a helper class for compressing / decompressing bytes of data.
    /// </summary>
    public static class Compressor
    {
        /// <summary>
        /// Compresses the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The compresse data.</returns>
        public static byte[] Compress(byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        /// <summary>
        /// Decompresses the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The decompressed data.</returns>
        public static byte[] Decompress(byte[] data)
        {
            var output = new MemoryStream();

            using (var input = new MemoryStream(data))
            using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }

            return output.ToArray();
        }
    }
}
