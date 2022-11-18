namespace SoundDeck.Core.Extensions
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides extension methods for <see cref="Process"/>.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Gets the icon from the <see cref="Process"/> as a base64 encoded string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>The icon, as a base64 encoded string.</returns>
        public static string GetIconAsBase64(this Process process)
        {
            if (process.MainModule.FileName is string filePath and not null)
            {
                var icon = Icon.ExtractAssociatedIcon(filePath).ToBitmap();
                using (var stream = new MemoryStream())
                {
                    icon.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var cryptoStream = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream))
                    {
                        return $"data:image/png;base64,{reader.ReadToEnd()}";
                    }
                }
            }

            return null;
        }
    }
}
