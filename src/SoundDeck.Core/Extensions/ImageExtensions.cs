namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Provides extension methods for <see cref="Image"/>.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Crops this <see cref="Image"/> to the <paramref name="rect"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to crop.</param>
        /// <param name="rect">The dimensions of the crop.</param>
        /// <returns>The cropped <see cref="Image"/>.</returns>
        public static Image Crop(this Image image, Rectangle rect)
        {
            var target = new Bitmap(rect.Width, rect.Height);
            using var graphics = Graphics.FromImage(target);

            graphics.DrawImage(image, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
            return target;
        }

        /// <summary>
        /// Converts this <see cref="Image"/> to a base64 encoded <see cref="string"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to convert.</param>
        /// <returns>The base64 encoded <see cref="string"/>.</returns>
        public static string ToBase64(this Image image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);

            return stream.ToBase64("image/png");
        }

        /// <summary>
        /// Resizes the <see cref="Image"/>, and returns a new image that is square.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to resize.</param>
        /// <returns>When the dimensions of <paramref name="image"/> are square, the original <paramref name="image"/>; otherwise a resized <see cref="Image"/>.</returns>
        public static Image ToSquare(this Image image)
        {
            if (image.Width == image.Height)
            {
                return image;
            }

            var size = Math.Min(image.Width, image.Height);
            var rect = new Rectangle(
                Math.Max(0, (image.Width - size) / 2),
                Math.Max(0, (image.Height - size) / 2),
                size,
                size);

            return image.Crop(rect);
        }
    }
}
