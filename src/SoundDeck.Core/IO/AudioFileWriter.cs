namespace SoundDeck.Core.IO
{
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using System;
    using System.IO;

    /// <summary>
    /// Provides a file writer for a <see cref="AudioFileReader"/>.
    /// </summary>
    public sealed class AudioFileWriter : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileWriter"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public AudioFileWriter(AudioFileReader reader)
        {
            this.Reader = reader;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to encode the file to an MP3.
        /// </summary>
        public bool EncodeToMP3 { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to normalize the volume of the audio file reader.
        /// </summary>
        public bool NormalizeVolume { get; set; } = false;

        /// <summary>
        /// Gets the reader; the main data source of the writer.
        /// </summary>
        private AudioFileReader Reader { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
            => this.Reader?.Dispose();

        /// <summary>
        /// Applies changes based on the state of this instance, and saves to the specified path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The output path; this may change depending on the state of this instance, e.g. encoding.</returns>
        public string Save(string path)
        {
            if (this.NormalizeVolume)
            {
                this.Reader.NormalizeVolume();
            }

            // attempt to encode if desired
            if (this.EncodeToMP3
                && this.TryEncodeToMP3(path, out var mp3Path))
            {
                return mp3Path;
            }

            // ensure we always have a file saved
            WaveFileWriter.CreateWaveFile(path, this.Reader);
            return path;
        }

        /// <summary>
        /// Gets the MP3 file path, based on the specified path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The MP3 file path.</returns>
        private string GetMP3FilePath(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length) + ".mp3";
        }

        /// <summary>
        /// Attempts to encode the audio file from the <paramref name="sourcePath"/> to an MP3, and saves to <paramref name="destinationPath"/>.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <returns><c>true</c> when encoding was successful; otherwise <c>false</c>.</returns>
        private bool TryEncodeToMP3(string sourcePath, out string destinationPath)
        {
            try
            {
                destinationPath = this.GetMP3FilePath(sourcePath);

                MediaFoundationApi.Startup();
                MediaFoundationEncoder.EncodeToMp3(this.Reader, destinationPath);
                MediaFoundationApi.Shutdown();

                return true;
            }
            catch
            {
                destinationPath = sourcePath;
                return false;
            }
        }
    }
}
