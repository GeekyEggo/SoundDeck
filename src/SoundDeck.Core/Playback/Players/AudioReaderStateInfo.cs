namespace SoundDeck.Core.Playback.Players
{
    using System;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides information about an audio reader's state.
    /// </summary>
    public sealed class AudioReaderStateInfo : IDisposable
    {
        /// <summary>
        /// Private member field for <see cref="Time"/>.
        /// </summary>
        private PlaybackTimeEventArgs _time;

        /// <summary>
        /// Private member field for <see cref="Volume"/>.
        /// </summary>
        private float _volume = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioReaderStateInfo"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public AudioReaderStateInfo(IAudioFileReader reader)
        {
            this.Reader = new WeakReference<IAudioFileReader>(reader);

            this._volume = reader.Volume;
            this.FileName = reader.FileName;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AudioReaderStateInfo"/> class from being created.
        /// </summary>
        private AudioReaderStateInfo()
            => this.IsDisposed = true;

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        public event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets the default audio state reader information.
        /// </summary>
        public static AudioReaderStateInfo Default { get; } = new AudioReaderStateInfo();

        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        public string FileName { get; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is playing.
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// Gets or sets the current time of the playback.
        /// </summary>
        public PlaybackTimeEventArgs Time
        {
            get => this._time;
            set
            {
                if (!this.IsDisposed
                    && this._time?.Equals(value) != true)
                {
                    this._time = value;
                    this.TimeChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        public float Volume
        {
            get => this._volume;
            set
            {
                if (this.IsDisposed)
                {
                    return;
                }
                else if (!this.Reader.TryGetTarget(out var reader))
                {
                    this.Dispose();
                }
                else if (this._volume != value)
                {
                    try
                    {
                        this._volume = value;
                        reader.Volume = value;
                    }
                    catch (ObjectDisposedException)
                    {
                        this.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the audio file reader..
        /// </summary>
        private WeakReference<IAudioFileReader> Reader { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
