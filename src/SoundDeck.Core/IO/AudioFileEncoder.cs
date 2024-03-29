namespace SoundDeck.Core.IO
{
    using System;
    using NAudio.Lame;
    using NAudio.Wave;
    using SoundDeck.Core.Playback.Readers;
    using SoundDeck.Core.Volume;

    /// <summary>
    /// Provides a file writer for a <see cref="AudioFileReader"/>.
    /// </summary>
    public sealed class AudioFileEncoder : IDisposable
    {
        /// <summary>
        /// The MP3 bit rate.
        /// </summary>
        private const int MP3_BIT_RATE = 192;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileEncoder"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public AudioFileEncoder(IAudioFileReader reader)
        {
            this.Reader = reader;
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public IAudioFileWriterSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the normalization provider.
        /// </summary>
        public INormalizationProvider NormalizationProvider { get; set; } = new NormalizationProvider();

        /// <summary>
        /// Gets the reader; the main data source of the writer.
        /// </summary>
        private IAudioFileReader Reader { get; }

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
            if (this.Settings.NormalizeVolume)
            {
                this.NormalizationProvider.ApplyPeakNormalization(this.Reader);
            }

            // determine the output type
            if (this.Settings.EncodeToMP3
                && this.Reader.Length > 0)
            {
                using (var writer = new LameMP3FileWriter(path, this.Reader.WaveFormat, MP3_BIT_RATE))
                {
                    var buffer = new byte[1024 * 4];
                    var read = 0;

                    while ((read = this.Reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, read);
                    }
                }
            }
            else
            {
                WaveFileWriter.CreateWaveFile(path, this.Reader);
            }
        }
    }
}
