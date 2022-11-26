namespace SoundDeck.Core.Extensions
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides extension methods for <see cref="Icon"/>.
    /// </summary>
    public static class IconExtensions
    {
        /// <summary>
        /// Converts this <see cref="Icon"/> to a base64 encoded <see cref="string"/>.
        /// </summary>
        /// <param name="icon">The <see cref="Icon"/> to convert.</param>
        /// <returns>The base64 encoded <see cref="string"/>.</returns>
        public static string ToBase64(this Icon icon)
        {
            using (var bitmap = icon.ToBitmap())
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                using (var cryptoStream = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    return $"data:image/png;base64,{reader.ReadToEnd()}";
                }
            }
        }
    }
}
