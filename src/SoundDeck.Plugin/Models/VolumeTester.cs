namespace SoundDeck.Plugin.Models
{
    using System;
    using System.Threading.Tasks;
    using SoundDeck.Core.Playback;
    using SoundDeck.Plugin.Models.Payloads;

    /// <summary>
    /// Provides information that can be used to test the volume.
    /// </summary>
    public sealed class VolumeTester : IIndexedAudioPlayer, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTester"/> class.
        /// </summary>
        /// <param name="player">The player.</param>
        public VolumeTester(IAudioPlayer player)
        {
            this.AudioPlayer = player;
        }

        /// <summary>
        /// Gets or sets the player responsible for testing the volume.
        /// </summary>
        public IAudioPlayer AudioPlayer { get; set; }

        /// <summary>
        /// Gets or sets the index of the file that is currently having its volume tested.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.AudioPlayer?.Dispose();
            this.AudioPlayer = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Plays the audio file contained within the specified <paramref name="payload"/> asynchronously.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The task of playing the audio.</returns>
        public Task PlayAsync(AdjustPlaylistFileVolumePayload payload, string deviceId)
        {
            if (this.AudioPlayer != null)
            {
                // Stop any current volume tests.
                this.AudioPlayer.Stop();

                this.Index = payload.Index;
                this.AudioPlayer.DeviceId = deviceId;

                return this.AudioPlayer.PlayAsync(payload);
            }

            return Task.CompletedTask;
        }
    }
}
