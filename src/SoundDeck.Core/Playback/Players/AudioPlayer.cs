namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public class AudioPlayer : Stopper, IAudioPlayer
    {
        /// <summary>
        /// The playback state polling delay, in milliseconds.
        /// </summary>
        internal const int PLAYBACK_STATE_POLL_DELAY = 150;

        /// <summary>
        /// Private member field for <see cref="Time"/>.
        /// </summary>
        private PlaybackTimeEventArgs _time;

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private member field for <see cref="Volume"/>.
        /// </summary>
        private float _volume = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal AudioPlayer(IAudioDevice device, ILogger<IAudioPlayer> logger)
        {
            this.Device = device;
            this.Logger = logger;
        }

        /// <summary>
        /// Occurs when the time of the current audio being played, changed.
        /// </summary>
        public event EventHandler<PlaybackTimeEventArgs> TimeChanged;

        /// <summary>
        /// Gets or sets the audio device.
        /// </summary>
        public IAudioDevice Device { get; set; }

        /// <summary>
        /// Gets the name of the file being played.
        /// </summary>
        public string FileName => this.ActiveReader?.FileName;

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
                    using (this._syncRoot.Lock())
                    {
                        if (this.ActiveReader != null)
                        {
                            this.ActiveReader.Volume = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the active reader.
        /// </summary>
        private IAudioFileReader ActiveReader { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<IAudioPlayer> Logger { get; }

        /// <inheritdoc/>
        public IAudioPlayer Clone()
            => new AudioPlayer(this.Device, this.Logger);

        /// <inheritdoc/>
        public Task PlayAsync(AudioFileInfo file, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            this.Stop();
            return Task.Factory.StartNew(async (state) =>
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(this.ActiveCancellationToken, (CancellationToken)state))
                {
                    var reader = this.GetAudioReader(file.Path);
                    reader.Volume = file.Volume;

                    using (await this._syncRoot.LockAsync(cts.Token))
                    {
                        this.ActiveReader = reader;
                    }

                    await this.PlayAsync(reader, cts.Token);
                }
            },
            cancellationToken,
            TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Plays the specified <paramref name="reader"/> asynchronously.
        /// </summary>
        /// <param name="reader">The reader to play.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PlayAsync(IAudioFileReader reader, CancellationToken cancellationToken)
        {
            try
            {
                var playbackStoppedTcs = new TaskCompletionSource<object>();

                using (var device = this.Device.GetMMDevice())
                using (var player = new WasapiOut(device, AudioClientShareMode.Shared, false, PLAYBACK_STATE_POLL_DELAY))
                {
                    try
                    {
                        this.Logger.LogTrace($"Playing \"{reader.FileName}\" on \"{device.FriendlyName}\".");

                        // Registers the handlers responsible for stopping the player safely.
                        cancellationToken.Register(state => ((WasapiOut)state)?.Stop(), player, useSynchronizationContext: true);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        bool CanPlay() => this.IsLooped && !cancellationToken.IsCancellationRequested;
                        void PlaybackStopped(object sender, EventArgs e)
                        {
                            if (!CanPlay())
                            {
                                player.Dispose();
                                playbackStoppedTcs.TrySetResult(true);
                            }
                        }

                        // Initialise the player.
                        player.Init(reader);
                        player.PlaybackStopped += PlaybackStopped;
                        this.WithActiveReader(reader, () => this.IsPlaying = true);

                        do
                        {
                            await this.PlayOnceAsync(reader, player, cancellationToken);
                        } while (CanPlay());

                        // Reset the reader when it is the active reader.
                        this.WithActiveReader(reader, () =>
                        {
                            this.ActiveReader = null;
                            this.IsPlaying = false;

                            this.OnTimeChanged(PlaybackTimeEventArgs.Zero);
                        });

                        await playbackStoppedTcs.Task;
                        player.PlaybackStopped -= PlaybackStopped;
                    }
                    finally
                    {
                        player.Stop();
                    }
                }
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Audio playback error.");
            }
            finally
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Plays the specified <paramref name="player"/> once, and awaits for playback to completely stop, asynchronously.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="player">The player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PlayOnceAsync(IAudioFileReader reader, WasapiOut player, CancellationToken cancellationToken)
        {
            var playbackStoppedTcs = new TaskCompletionSource<bool>();
            player.PlaybackStopped += PlaybackStopped;

            // Play the audio from the start.
            reader.Seek(0, SeekOrigin.Begin);
            player.Play();

            // Continually update the current time whilst the player is not stopped.
            this.WithActiveReader(reader, () => this.OnTimeChanged(PlaybackTimeEventArgs.Zero));
            while (player.PlaybackState != PlaybackState.Stopped && !cancellationToken.IsCancellationRequested)
            {
                this.WithActiveReader(reader, () => this.OnTimeChanged(PlaybackTimeEventArgs.FromReader(reader)));
                Thread.Sleep(PLAYBACK_STATE_POLL_DELAY);
            }

            await playbackStoppedTcs.Task;

            void PlaybackStopped(object sender, StoppedEventArgs e)
            {
                player.PlaybackStopped -= PlaybackStopped;
                playbackStoppedTcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Gets the audio reader for the specified <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The audio reader capable of reading the file.</returns>
        private IAudioFileReader GetAudioReader(string file)
        {
            if (VorbisFileReader.CanReadFile(file))
            {
                return new VorbisFileReader(file);
            }
            else if (StreamDeckAudioPlayer.CanReadFile(file))
            {
                return new StreamDeckAudioPlayer(file);
            }

            return new AudioFileReaderWrapper(file);
        }

        /// <summary>
        /// Raises the <see cref="E:TimeChanged" /> event when the <paramref name="value"/> differs to <see cref="_time"/>.
        /// </summary>
        /// <param name="value">The <see cref="PlaybackTimeEventArgs"/> instance containing the event data.</param>
        private void OnTimeChanged(PlaybackTimeEventArgs value)
        {
            if (this._time?.Equals(value) != true)
            {
                this._time = value;
                this.TimeChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Invokes the <paramref name="action"/> when the <paramref name="reader"/> is the <see cref="ActiveReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="action">The action.</param>
        private void WithActiveReader(IAudioFileReader reader, Action action)
        {
            using (this._syncRoot.Lock())
            {
                if (this.ActiveReader?.Equals(reader) == true)
                {
                    action();
                }
            }
        }
    }
}
