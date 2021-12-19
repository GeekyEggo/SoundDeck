namespace SoundDeck.Core.Playback.Players
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides an audio player for an audio device.
    /// </summary>
    public class AudioPlayer : Stopper, IAudioPlayer
    {
        /// <summary>
        /// The playback state polling delay, in milliseconds.
        /// </summary>
        private const int PLAYBACK_STATE_POLL_DELAY = 100;

        /// <summary>
        /// Private member field for <see cref="ReaderState"/>.
        /// </summary>
        private AudioReaderStateInfo _readerState = AudioReaderStateInfo.Default;

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
        public string FileName => this.ReaderState.FileName;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is looped.
        /// </summary>
        public bool IsLooped { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        public bool IsPlaying => this.ReaderState.IsPlaying;

        /// <summary>
        /// Gets or sets the volume of the audio being played; this can be between 0 and 1.
        /// </summary>
        public float Volume
        {
            get => this.ReaderState.Volume;
            set => this.ReaderState.Volume = value;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<IAudioPlayer> Logger { get; }

        /// <summary>
        /// Gets or sets the audio reader state information.
        /// </summary>
        private AudioReaderStateInfo ReaderState => this._readerState;

        /// <inheritdoc/>
        public IAudioPlayer Clone()
            => new AudioPlayer(this.Device, this.Logger);

        /// <inheritdoc/>
        public Task PlayAsync(AudioFileInfo file, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(async (state) =>
            {
                if (((CancellationToken)state).IsCancellationRequested)
                {
                    return;
                }

                this.Stop();

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(this.ActiveCancellationToken, (CancellationToken)state))
                using (var reader = this.GetAudioReader(file.Path))
                using (var readerState = new AudioReaderStateInfo(reader))
                {
                    try
                    {
                        reader.Volume = file.Volume;
                        readerState.TimeChanged += this.TimeChanged;

                        var oldReaderState = Interlocked.Exchange(ref this._readerState, readerState);
                        oldReaderState.TimeChanged -= this.TimeChanged;

                        await this.PlayAsync(reader, readerState, cts.Token);
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref this._readerState, AudioReaderStateInfo.Default, readerState);
                        readerState.TimeChanged -= this.TimeChanged;
                    }
                }
            },
            cancellationToken,
            TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Plays the specified <paramref name="reader"/> asynchronously.
        /// </summary>
        /// <param name="reader">The reader to play.</param>
        /// <param name="readerState">The audio reader state information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PlayAsync(IAudioFileReader reader, AudioReaderStateInfo readerState, CancellationToken cancellationToken)
        {
            try
            {
                using (var device = this.Device.GetMMDevice())
                using (var player = new WasapiOut(device, AudioClientShareMode.Shared, false, PLAYBACK_STATE_POLL_DELAY))
                {
                    try
                    {
                        this.Logger.LogTrace($"Playing \"{reader.FileName}\" on \"{device.FriendlyName}\".");
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Initialise the player.
                        player.Init(reader);
                        readerState.IsPlaying = true;

                        do
                        {
                            await this.PlayOnceAsync(reader, readerState, player, cancellationToken);
                        } while (this.IsLooped && !cancellationToken.IsCancellationRequested);

                        readerState.IsPlaying = false;
                        readerState.Time = PlaybackTimeEventArgs.Zero;
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
        /// <param name="readerState">The audio reader state information.</param>
        /// <param name="player">The player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PlayOnceAsync(IAudioFileReader reader, AudioReaderStateInfo readerState, WasapiOut player, CancellationToken cancellationToken)
        {
            var playbackStoppedTcs = new TaskCompletionSource<bool>();
            player.PlaybackStopped += PlaybackStopped;

            // Play the audio from the start.
            reader.Seek(0, SeekOrigin.Begin);
            player.Play();

            // Continually update the current time whilst the player is not stopped.
            readerState.Time = PlaybackTimeEventArgs.Zero;
            while (player.PlaybackState != PlaybackState.Stopped && !cancellationToken.IsCancellationRequested)
            {
                readerState.Time = PlaybackTimeEventArgs.FromReader(reader);
                Thread.Sleep(PLAYBACK_STATE_POLL_DELAY);
            }

            // Important; this allows us to stop playback on the executing thread early in the event cancellation was requested.
            player.Stop();
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
    }
}
