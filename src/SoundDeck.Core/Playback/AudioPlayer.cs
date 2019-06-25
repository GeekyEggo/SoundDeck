namespace SoundDeck.Core.Playback
{
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    internal class AudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// The playback state polling delay, in milliseconds.
        /// </summary>
        private const int PLAYBACK_STATE_POLL_DELAY = 250;

        /// <summary>
        /// The synchronization root object.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        internal AudioPlayer(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Gets the audio device identifier.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public PlaybackStateType State { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Gets or sets the internal cancellation token source; this is used to cancel all audio on the player.
        /// </summary>
        private CancellationTokenSource InternalCancellationTokenSource { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.InternalCancellationTokenSource?.Cancel();
            this.IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Plays the audio file asynchronously.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The task of the audio file being played.</returns>
        public Task PlayAsync(string file, CancellationToken token)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("The audio player has been disposed.");
            }

            return Task.Run(() =>
            {
                lock (this._syncRoot)
                {
                    this.InternalCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    this.InternalPlay(file);
                    this.InternalCancellationTokenSource = null;
                }
            });
        }

        /// <summary>
        /// Stops any audio being played on this player.
        /// </summary>
        public void Stop()
            => this.InternalCancellationTokenSource?.Cancel();

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
        private void InternalPlay(string file)
        {
            using (var player = new WasapiOut(this.GetDevice(), AudioClientShareMode.Shared, false, 0))
            using (var stream = new AudioFileReader(file))
            {
                player.Init(stream);

                player.Play();
                this.State = PlaybackStateType.Playing;

                while (player.PlaybackState != PlaybackState.Stopped
                    && !this.InternalCancellationTokenSource.IsCancellationRequested
                    && !this.IsDisposed)
                {
                    Thread.Sleep(PLAYBACK_STATE_POLL_DELAY);
                }

                player.Stop();
                this.State = PlaybackStateType.Stopped;
            }
        }
    }
}
