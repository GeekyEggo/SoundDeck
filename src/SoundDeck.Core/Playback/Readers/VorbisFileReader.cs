namespace SoundDeck.Core.Playback.Readers
{
    using System;
    using System.IO;
    using NAudio.Vorbis;
    using NAudio.Wave;
    using NAudio.Wave.SampleProviders;

    /// <summary>
    /// Provides a wrapper for <see cref="VorbisWaveReader"/> that allows for greater control of the underlying <see cref="WaveStream"/>.
    /// </summary>
    public sealed class VorbisFileReader : IAudioFileReader
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="VorbisFileReader"/> class.
        /// </summary>
        /// <param name="fileName">The path to the file.</param>
        public VorbisFileReader(string fileName)
        {
            this.FileName = fileName;

            this.Reader = new VorbisWaveReader(fileName);
            this.SampleChannel = new SampleChannel(this.Reader);
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets or sets the underlying reader responsible for reading the OGG file.
        /// </summary>
        private VorbisWaveReader Reader { get; set; }

        /// <summary>
        /// Gets or sets the sample channel; this allows us to control the volume.
        /// </summary>
        private SampleChannel SampleChannel { get; set; }

        /// <summary>
        /// Gets the current time of the audio.
        /// </summary>
        public TimeSpan CurrentTime => this.Reader.CurrentTime;

        /// <summary>
        /// Gets the Length of this stream (in bytes)
        /// </summary>
        public long Length => this.Reader.Length;

        /// <summary>
        /// Gets the total time of the audio.
        /// </summary>
        public TimeSpan TotalTime => this.Reader.TotalTime;

        /// <summary>
        /// Gets or sets the volume of the audio .
        /// </summary>
        public float Volume
        {
            get => this.SampleChannel.Volume;
            set => this.SampleChannel.Volume = value;
        }

        /// <summary>
        /// Gets the wave format.
        /// </summary>
        public WaveFormat WaveFormat => this.SampleChannel.WaveFormat;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Reader?.Dispose();
            this.Reader = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines whether this instance can read the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if this instance can read the specified file name; otherwise, <c>false</c>.</returns>
        public static bool CanReadFile(string fileName)
        {
            return fileName.EndsWith(".oga", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".opus", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads information contained within this stream to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            const int BYTES_PER_FLOAT = 4;

            var waveBuffer = new WaveBuffer(buffer);
            var samplesRequired = count / BYTES_PER_FLOAT;

            return this.Read(waveBuffer.FloatBuffer, offset / BYTES_PER_FLOAT, samplesRequired) * BYTES_PER_FLOAT;
        }

        /// <summary>
        /// Reads information contained within this stream to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The number of floats to read.</param>
        /// <returns>The number of floats read.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (this._syncRoot)
            {
                return this.SampleChannel.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Seeks to the specified <paramref name="offset" />, based on the <paramref name="origin" />.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        /// <returns>The new position.</returns>
        public long Seek(long offset, SeekOrigin origin)
            => this.Reader.Seek(offset, origin);
    }
}
