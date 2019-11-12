namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// The playback state polling delay, in milliseconds.
        /// </summary>
        private const int PLAYBACK_STATE_POLL_DELAY = 250;

        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="Time"/>.
        /// </summary>
        private PlaybackTimeEventArgs _time = PlaybackTimeEventArgs.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        internal AudioPlayer(string deviceId, INormalizationProvider normalizationProvider)
        {
            this.DeviceId = deviceId;
            this.NormalizationProvider = normalizationProvider;
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
        /// Gets the audio device identifier.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        public bool IsLooped { get; set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public PlaybackStateType State { get; private set; }

        /// <summary>
        /// Gets the current and total time of the audio being played.
        /// </summary>
        public PlaybackTimeEventArgs Time
        {
            get
            {
                return this._time;
            }
            private set
            {
                if (!value.Equals(this._time))
                {
                    this._time = value;
                    if (!this.IsDisposed)
                    {
                        this.TimeChanged?.Invoke(this, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the normalization provider.
        /// </summary>
        private INormalizationProvider NormalizationProvider { get; }

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
        /// <param name="file">The file.</param>
        /// <param name="maxGain">The optional maximum gain; when null, the default volume is used.</param>
        /// <returns>The task of the audio file being played.</returns>
        public Task PlayAsync(string file, float? maxGain = null)
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
                    this.InternalPlay(file, maxGain);
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
        /// Gets the audio device this instance is associated with.
        /// </summary>
        /// <remarks>Re-selecting the device is required as execution occurs on a separate thread.</remarks>
        /// <returns>The audio device.</returns>
        private MMDevice GetDevice()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                return enumerator.GetDevice(this.DeviceId);
            }
        }

        /// <summary>
        /// Plays the audio file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="maxGain">The optional maximum gain; when null, the default volume is used.</param>
        private void InternalPlay(string file, float? maxGain = null)
        {
            using (var player = new WasapiOut(this.GetDevice(), AudioClientShareMode.Shared, false, 0))
            using (var reader = new AudioFileReader(file))
            {
                // prepare the player
                if (maxGain != null)
                {
                    this.NormalizationProvider.ApplyLoudnessNormalization(reader, maxGain.Value);
                }

                player.Init(reader);
                player.PlaybackStopped += (_, __) => this.Time = PlaybackTimeEventArgs.Zero;

                do
                {
                    this.InternalCancellationTokenSource.Token.Register(player.Stop);

                    // play the audio clip
                    reader.Seek(0, SeekOrigin.Begin);
                    player.Play();

                    this.Time = PlaybackTimeEventArgs.FromReader(reader);
                    this.State = PlaybackStateType.Playing;

                    while (player.PlaybackState != PlaybackState.Stopped && this.IsPlayableState)
                    {
                        Thread.Sleep(PLAYBACK_STATE_POLL_DELAY);
                        this.Time = PlaybackTimeEventArgs.FromReader(reader);
                    }

                    player.Stop();
                    this.State = PlaybackStateType.Stopped;
                } while (this.IsLooped && this.IsPlayableState);

                this.TimeChanged?.Invoke(this, new PlaybackTimeEventArgs(TimeSpan.Zero, TimeSpan.Zero));
            }
        }
    }
}
