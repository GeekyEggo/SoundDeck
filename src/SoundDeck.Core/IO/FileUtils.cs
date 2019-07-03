namespace SoundDeck.Core.IO
{
    using System;
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

        /// <summary>
        /// Gets a time stamped path, utilising the specified <paramref name="format"/>, with {0} being the injected format.
        /// </summary>
        /// <param name="dir">The parent directory path.</param>
        /// <param name="format">The file name format.</param>
        /// <param name="ensureUnique">When set to <c>true</c>, the file path will be unique.</param>
        /// <returns>The path.</returns>
        public static string GetTimeStampPath(string dir, string format, bool ensureUnique = false)
        {
            var path = Path.Combine(dir, string.Format(format, DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss")));
            return ensureUnique ? GetUniquePath(path) : path;
        }

        /// <summary>
        /// Gets the unique path, based on the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The path; otherwise the path indexed, i.e. "My document (1).doc"</returns>
        public static string GetUniquePath(string path)
        {
            if (!File.Exists(path) || string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            var info = new FileInfo(path);
            return GetUniquePath(info.DirectoryName, info.Name.Substring(0, info.Name.Length - info.Extension.Length), info.Extension);
        }

        /// <summary>
        /// Recursively gets a path, applying the index to the name, until the path name is unique.
        /// </summary>
        /// <param name="dir">The parent directory.</param>
        /// <param name="name">The file name.</param>
        /// <param name="extension">The file extension.</param>
        /// <param name="index">The index.</param>
        /// <returns>The unique indexed path.</returns>
        private static string GetUniquePath(string dir, string name, string extension, int index = 1)
        {
            var path = Path.Combine(dir, $"{name} ({index}){extension}");
            if (!File.Exists(path))
            {
                return path;
            }

            return GetUniquePath(dir, name, extension, index + 1);
        }
    }
}
