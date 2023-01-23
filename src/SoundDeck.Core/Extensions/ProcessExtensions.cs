namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using SoundDeck.Core.Interop;
    using static SoundDeck.Core.Interop.Shell32;

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
        /// <remarks>Credit: https://stackoverflow.com/a/28530403/259656.</remarks>
        public static string GetIconAsBase64(this Process process)
        {
            try
            {
                if (process.MainModule.FileName is string filePath and not null)
                {
                    // Determine the index of the icon.
                    var sfi = new SHFILEINFO();
                    Shell32.SHGetFileInfo(filePath, 0, ref sfi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(sfi), (uint)(SHGFI.SysIconIndex | SHGFI.LargeIcon | SHGFI.UseFileAttributes));

                    // Get the icon from the image list.
                    IImageList spiml = null;
                    var guil = new Guid(IID_IImageList2);

                    Shell32.SHGetImageList(Shell32.SHIL_JUMBO, ref guil, ref spiml);
                    var hIcon = IntPtr.Zero;
                    spiml.GetIcon(sfi.iIcon, Shell32.ILD_TRANSPARENT | Shell32.ILD_IMAGE, ref hIcon);

                    // Clone the icon and destroy the origin, before converting to base64
                    using (var icon = (Icon)Icon.FromHandle(hIcon).Clone())
                    {
                        User32.DestroyIcon(hIcon);
                        return icon.ToBase64();
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
