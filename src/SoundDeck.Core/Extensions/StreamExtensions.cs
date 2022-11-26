namespace SoundDeck.Core.Extensions
{
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides extension methods for <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts this <see cref="Stream"/> to a base64 encoded <see cref="string"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to encode.</param>
        /// <param name="mimeType">The mime-type of the <paramref name="stream"/>.</param>
        /// <returns>The base64 encoded <see cref="string"/>.</returns>
        public static string ToBase64(this Stream stream, string mimeType)
        {
            stream.Position = 0;

            using var cryptoStream = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);

            return $"data:{mimeType};base64,{reader.ReadToEnd()}";
        }
    }
}
