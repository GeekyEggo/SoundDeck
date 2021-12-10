namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private member field for <see cref="Volume"/>.
        /// </summary>
        private float _volume = 1;

        /// <summary>
        /// Private member field that supports <see cref="LiveCancellationToken"/>.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal AudioPlayer(IAudioDevice device, ILogger<AudioPlayer> logger)
        {
            this.Device = device;
            this.Logger = logger;
        }

        /// <summary>
        /// Occurs when the audio player is disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        public event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Occurs when the volume of the audio player changes.
        /// </summary>
        public event EventHandler VolumeChanged;

        /// <summary>
        /// Gets or sets the audio device.
        /// </summary>
        public IAudioDevice Device { get; set; }

        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        public bool IsLooped { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Gets or sets the volume of the audio being played; this can be between 0 and 1.
        /// </summary>
        public float Volume
        {
            get => this._volume;
            set
            {
                if (this._volume != value)
                {
                    this._volume = value;
                    this.VolumeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets the active cancellation token.
        /// </summary>
        private CancellationToken LiveCancellationToken
        {
            get
            {
                lock (this._syncRoot)
                {
                    if (this._cancellationTokenSource.IsCancellationRequested
                        && !this.IsDisposed)
                    {
                        this._cancellationTokenSource = new CancellationTokenSource();
                    }

                    return this._cancellationTokenSource.Token;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in a playable state.
        /// </summary>
        private bool IsPlayableState => !this.IsDisposed;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<AudioPlayer> Logger { get; }

        /// <inheritdoc/>
        public IAudioPlayer Clone()
            => new AudioPlayer(this.Device, this.Logger);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <returns>The task of the audio file being played.</returns>
        public Task PlayAsync(AudioFileInfo file)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("The audio player has been disposed.");
            }

            return this.InternalPlayAsync(file, this.LiveCancellationToken);
        }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        public void Stop()
        {
            lock (this._syncRoot)
            {
                this._cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            lock (this._syncRoot)
            {
                this._cancellationTokenSource.Cancel();

                if (dispose)
                {
                    if (!this.IsDisposed)
                    {
                        this.Disposed?.Invoke(this, EventArgs.Empty);
                    }

                    this.IsDisposed = true;
                }
            }
        }

        /// <summary>
        /// Plays the audio file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="cancellationToken">The cancellation token responsible for stopping playback.</param>
        /// <returns>The task of playing the audio file.</returns>
        private async Task InternalPlayAsync(AudioFileInfo file, CancellationToken cancellationToken)
        {
            this.FileName = file.Path;
            this.Volume = file.Volume;

            using (var device = this.Device.GetMMDevice())
            using (var player = new AsyncWasapiOut(device, file.Path))
            {
                this.Logger.LogTrace($"Playing \"{file.Path}\" on \"{device.FriendlyName}\".");
                void SynchronizeVolume(object sender, EventArgs e) => player.FileVolume = this.Volume;

                // prepare the player
                player.TimeChanged += this.TimeChanged;
                this.VolumeChanged += SynchronizeVolume;

                player.Init();
                SynchronizeVolume(this, EventArgs.Empty);

                do
                {
                    this.IsPlaying = true;

                    cancellationToken.Register(() => this.IsPlaying = false);
                    await player.PlayAsync(cancellationToken);

                    this.IsPlaying = false;

                } while (this.IsLooped && this.IsPlayableState);

                this.TimeChanged?.Invoke(this, new PlaybackTimeEventArgs(TimeSpan.Zero, TimeSpan.Zero));
                this.VolumeChanged -= SynchronizeVolume;
            }

            this.FileName = string.Empty;
        }
    }
}
