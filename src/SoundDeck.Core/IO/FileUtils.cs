namespace SoundDeck.Core.IO
{
    using System.IO;

    /// <summary>
    /// Provides utility methods relating to files.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Deletes the file for the specified path if it exists.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
