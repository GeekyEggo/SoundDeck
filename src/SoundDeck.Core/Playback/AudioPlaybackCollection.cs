namespace SoundDeck.Core.Playback
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a means of playing back a collection of files, with customised actions and order applied.
    /// </summary>
    public class AudioPlaybackCollection
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        private static readonly Random Rnd = new Random();

        /// <summary>
        /// The synchronization root
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlaybackCollection"/> class.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="options">The options.</param>
        public AudioPlaybackCollection(IAudioPlayer player, IAudioPlaybackOptions options)
        {
            this.Action = options.Action;
            this.Files = options.Files ?? new string[0];
            this.Order = options.Order;
            this.Player = player;

            this.RefreshOrder();
        }

        /// <summary>
        /// Gets the files; these are the original files, and may not represent the play order.
        /// </summary>
        public string[] Files { get; private set; }

        /// <summary>
        /// Gets or sets the player.
        /// </summary>
        public IAudioPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public PlaybackActionType Action { get; set; }

        /// <summary>
        /// Gets or sets the playback order.
        /// </summary>
        public PlaybackOrderType Order { get; set; }

        /// <summary>
        /// Gets or sets the maximum gain, i.e. the max volume.
        /// </summary>
        public float MaxGain { get; set; } = 0.35f;

        /// <summary>
        /// Gets or sets the current index.
        /// </summary>
        private int CurrentIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the items; these are the ordered <see cref="Files"/>.
        /// </summary>
        private string[] Items { get; set; }

        /// <summary>
        /// Sets the options asynchronously.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The task of updating the options.</returns>
        public void SetOptions(IAudioPlaybackOptions options)
        {
            try
            {
                this._syncRoot.Wait();

                // set the files
                if (this.TrySetFiles(options.Files))
                {
                    this.CurrentIndex = 0;
                }

                // refresh the order if its changed
                if (options.Order != this.Order)
                {
                    this.RefreshOrder();
                    this.CurrentIndex = 0;
                }

                this.Action = options.Action;
                this.Order = options.Order;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Invokes the next <see cref="Action"/>.
        /// </summary>
        /// <returns>The task of moving to the next item within the playback.</returns>
        public async Task NextAsync()
        {
            if (this.Player == null || this.Items.Length == 0)
            {
                return;
            }

            var completeAfterCancel = this.Action == PlaybackActionType.PlayStop && this.Player.State == PlaybackStateType.Playing;
            this.Player.Stop();

            if (!completeAfterCancel)
            {
                await this.PlayNextAudioAsync();
            }
        }

        /// <summary>
        /// Plays the next audio clip.
        /// </summary>
        /// <returns>The player, playing the audio.</returns>
        private async Task PlayNextAudioAsync()
        {
            try
            {
                await this._syncRoot.WaitAsync();
                if (this.CurrentIndex >= this.Items.Length)
                {
                    this.CurrentIndex = 0;
                }

                await this.Player.PlayAsync(this.Items[this.CurrentIndex], CancellationToken.None, this.MaxGain);
                this.CurrentIndex++;
            }
            finally
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Refreshes the order.
        /// </summary>
        private void RefreshOrder()
        {
            if (this.Order == PlaybackOrderType.Random)
            {
                this.Items = this.Files.OrderBy(_ => Rnd.Next()).ToArray();
            }
            else
            {
                this.Items = this.Files;
            }
        }

        /// <summary>
        /// Tries to set <see cref="Files"/>, based on the equality of <paramref name="newFiles"/>.
        /// </summary>
        /// <param name="newFiles">The new files.</param>
        /// <returns><c>true</c> when the files were updated; otherwise <c>false</c>.</returns>
        private bool TrySetFiles(string[] newFiles)
        {
            newFiles = newFiles ?? new string[0];
            if (this.Files.SequenceEqual(newFiles))
            {
                return false;
            }

            this.Files = newFiles;
            this.RefreshOrder();

            return true;
        }
    }
}
