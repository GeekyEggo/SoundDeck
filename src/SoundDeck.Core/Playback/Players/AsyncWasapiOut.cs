namespace SoundDeck.Core.Playback
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Volume;

    /// <summary>
    /// Provides an asynchronous wrapper for <see cref="WasapiOut"/>.
    /// </summary>
    public class AsyncWasapiOut : WasapiOut
    {
        /// <summary>
        /// The playback state polling delay, in milliseconds.
        /// </summary>
        internal const int PLAYBACK_STATE_POLL_DELAY = 200;

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="Time"/>.
        /// </summary>
        private PlaybackTimeEventArgs _time = PlaybackTimeEventArgs.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncWasapiOut" /> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="file">The file.</param>
        public AsyncWasapiOut(MMDevice device, string file)
            : base(device, AudioClientShareMode.Shared, false, PLAYBACK_STATE_POLL_DELAY)
        {
            this.Reader = new AudioFileReader(file);
            this.PlaybackStopped += this.AsyncWasapiOut_PlaybackStopped;
        }

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        public event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets or sets volume.
        /// </summary>
        public float FileVolume
        {
            get => this.Reader.Volume;
            set
            {
                if (!this.IsDisposed)
                {
                    this.Reader.Volume = value;
                }
            }
        }

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
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Gets the audio file reader.
        /// </summary>
        private AudioFileReader Reader { get; }

        /// <summary>
        /// Gets or sets cancellation token used to detect when the player has truly finished.
        /// </summary>
        private TaskCompletionSource<bool> StopTaskCompletionSource { get; set; }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();

            if (!this.IsDisposed)
            {
                this.Reader?.Dispose();
                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
            => this.Init(this.Reader);

        /// <summary>
        /// Initializes this instance with gain applied.
        /// </summary>
        /// <param name="maxGain">The maximum gain.</param>
        /// <param name="normalizationProvider">The normalization provider.</param>
        public void Init(float maxGain, INormalizationProvider normalizationProvider)
        {
            normalizationProvider.ApplyLoudnessNormalization(this.Reader, maxGain);
            this.Init();
        }

        /// <summary>
        /// Plays the audio asynchronously, and waits for it to fully stop.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task of playing the audio.</returns>
        public async Task PlayAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this._syncRoot.WaitAsync();

                this.StopTaskCompletionSource?.TrySetResult(false);
                this.StopTaskCompletionSource = new TaskCompletionSource<bool>(); ;
            }
            finally
            {
                this._syncRoot.Release();
            }

            // play the audio clip
            this.Reader.Seek(0, SeekOrigin.Begin);
            this.Play();

            this.Time = PlaybackTimeEventArgs.FromReader(this.Reader);
            while (this.PlaybackState != PlaybackState.Stopped && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(PLAYBACK_STATE_POLL_DELAY);
                this.Time = PlaybackTimeEventArgs.FromReader(this.Reader);
            }

            await this.StopAsync();
        }

        /// <summary>
        /// Stops any audio playing asynchronously.
        /// </summary>
        public async Task StopAsync()
        {
            this.Stop();

            try
            {
                await this._syncRoot.WaitAsync();
                if (this.StopTaskCompletionSource != null)
                {
                    await this.StopTaskCompletionSource.Task;
                }
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Handles the <see cref="WasapiOut.PlaybackStopped"/> of this instance.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoppedEventArgs"/> instance containing the event data.</param>
        private void AsyncWasapiOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.Time = PlaybackTimeEventArgs.Zero;
            this.StopTaskCompletionSource?.TrySetResult(true);
        }
    }
}
