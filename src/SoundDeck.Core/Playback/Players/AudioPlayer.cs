namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="Volume"/>.
        /// </summary>
        private float _volume = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        internal AudioPlayer(string deviceId)
        {
            this.DeviceId = deviceId;
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
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string DeviceId { get; set; }

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
        /// Gets or sets the internal cancellation token source; this is used to cancel all audio on the player.
        /// </summary>
        private CancellationTokenSource InternalCancellationTokenSource { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is in a playable state.
        /// </summary>
        private bool IsPlayableState
        {
            get { return this.InternalCancellationTokenSource?.IsCancellationRequested == false && !this.IsDisposed; }
        }

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

            return Task.Run(async () =>
            {
                try
                {
                    await this._syncRoot.WaitAsync();

                    this.InternalCancellationTokenSource = new CancellationTokenSource();
                    await this.InternalPlayAsync(file);
                    this.InternalCancellationTokenSource = null;
                }
                finally
                {
                    this._syncRoot.Release();
                }
            });
        }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        public void Stop()
            => this.InternalCancellationTokenSource?.Cancel();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            this.InternalCancellationTokenSource?.Cancel();
            if (dispose)
            {
                if (!this.IsDisposed)
                {
                    this.Disposed?.Invoke(this, EventArgs.Empty);
                }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Plays the audio file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The task of playing the audio file.</returns>
        private async Task InternalPlayAsync(AudioFileInfo file)
        {
            this.FileName = file.Path;
            this.Volume = file.Volume;

            using (var device = AudioDevices.Current.GetDevice(this.DeviceId))
            using (var player = new AsyncWasapiOut(device, file.Path))
            {
                void SynchronizeVolume(object sender, EventArgs e) => player.FileVolume = this.Volume;

                // prepare the player
                player.TimeChanged += this.TimeChanged;
                this.VolumeChanged += SynchronizeVolume;

                player.Init();
                SynchronizeVolume(this, EventArgs.Empty);

                do
                {
                    this.IsPlaying = true;
                    await player.PlayAsync(this.InternalCancellationTokenSource.Token);
                    this.IsPlaying = false;

                } while (this.IsLooped && this.IsPlayableState);

                this.TimeChanged?.Invoke(this, new PlaybackTimeEventArgs(TimeSpan.Zero, TimeSpan.Zero));
                this.VolumeChanged -= SynchronizeVolume;
            }

            this.FileName = string.Empty;
        }
    }
}
