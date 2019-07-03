namespace SoundDeck.Core.IO
{
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Playback;
    using System;

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
        /// Gets or sets the normalization provider.
        /// </summary>
        public INormalizationProvider NormalizationProvider { get; set; } = new NormalizationProvider();

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
        public void Save(string path)
        {
            // applies peak normalization to the reader
            if (this.NormalizeVolume)
            {
                this.NormalizationProvider.ApplyPeakNormalization(this.Reader);
            }

            // determine the output type
            if (this.EncodeToMP3)
            {
                MediaFoundationApi.Startup();
                MediaFoundationEncoder.EncodeToMp3(this.Reader, path);
                MediaFoundationApi.Shutdown();
            }
            else
            {
                WaveFileWriter.CreateWaveFile(path, this.Reader);
            }
        }
    }
}
