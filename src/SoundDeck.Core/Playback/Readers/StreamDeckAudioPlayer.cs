namespace SoundDeck.Core.Playback.Readers
{
    using System;
    using System.IO;
    using NAudio.Wave;
    using NAudio.Wave.SampleProviders;

    /// <summary>
    /// Provides an <see cref="IAudioFileReader"/> capable of reading StreamDeckAudio files.
    /// </summary>
    public class StreamDeckAudioPlayer : WaveStream, IAudioFileReader
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private member field for <see cref="Length"/>.
        /// </summary>
        private long _length { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileReaderWrapper"/> class.
        /// </summary>
        /// <param name="fileName">The file to open</param>
        public StreamDeckAudioPlayer(string fileName)
            : base()
        {
            this.FileStream = new StreamDeckAudioStreamReader(fileName);
            this.Reader = new WaveFileReader(this.FileStream);

            if (this.Reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm
                && this.Reader.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                this.Reader = WaveFormatConversionStream.CreatePcmStream(this.Reader);
                this.Reader = new BlockAlignReductionStream(this.Reader);
            }

            this.SampleChannel = new SampleChannel(this.Reader, false);

            this.SourceBytesPerSample = (this.Reader.WaveFormat.BitsPerSample / 8) * this.Reader.WaveFormat.Channels;
            this.DestinationBytesPerSample = 4 * this.SampleChannel.WaveFormat.Channels;

            this._length = this.SourceToDestination(this.Reader.Length);
        }

        /// <summary>
        /// Gets the sample channel.
        /// </summary>
        private  SampleChannel SampleChannel { get; }

        /// <summary>
        /// File Name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// WaveFormat of this stream
        /// </summary>
        public override WaveFormat WaveFormat => this.SampleChannel.WaveFormat;

        /// <summary>
        /// Length of this stream (in bytes)
        /// </summary>
        public override long Length => this._length;

        /// <summary>
        /// Gets the number of bytes per sample, for the destination.
        /// </summary>
        private int DestinationBytesPerSample { get; }

        /// <summary>
        /// Gets the number of bytes per sample, for the source.
        /// </summary>
        private int SourceBytesPerSample { get; }

        /// <summary>
        /// Gets or sets the underlying file stream.
        /// </summary>
        private Stream FileStream { get; set; }

        /// <summary>
        /// Gets or sets the stream reader.
        /// </summary>
        private WaveStream Reader { get; set; }

        /// <summary>
        /// Determines whether this instance can read the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if this instance can read the specified file name; otherwise, <c>false</c>.</returns>
        public static bool CanReadFile(string fileName)
            => fileName.EndsWith(".streamDeckAudio", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Position of this stream (in bytes).
        /// </summary>
        public override long Position
        {
            get => this.SourceToDestination(this.Reader.Position);
            set
            {
                lock (this._syncRoot)
                {
                    this.Reader.Position = this.DestinationToSource(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume of the audio; 1.0f is full volume
        /// </summary>
        public float Volume
        {
            get => this.SampleChannel.Volume;
            set => this.SampleChannel.Volume = value;
        }

        /// <summary>
        /// Reads from this wave stream.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset into the stream to read from.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var waveBuffer = new WaveBuffer(buffer);
            var samplesRequired = count / 4;
            var samplesRead = this.Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);

            return samplesRead * 4;
        }

        /// <summary>
        /// Reads audio from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (this._syncRoot)
            {
                return this.SampleChannel.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Stream"></see> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.FileStream?.Dispose();
                this.FileStream = null;

                this.Reader?.Dispose();
                this.Reader = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper to convert destination to source bytes.
        /// </summary>
        private long DestinationToSource(long destBytes)
            => this.SourceBytesPerSample * (destBytes / this.DestinationBytesPerSample);

        /// <summary>
        /// Helper to convert source to destination bytes.
        /// </summary>
        private long SourceToDestination(long sourceBytes)
            => this.DestinationBytesPerSample * (sourceBytes / this.SourceBytesPerSample);
    }
}
