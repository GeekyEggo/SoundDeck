namespace SoundDeck.Plugin.Models
{
    using System;
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides information that can be used to test the volume.
    /// </summary>
    public sealed class VolumeTester : IDisposable
    {
        /// <summary>
        /// Gets or sets the index of the file that is currently having its volume tested.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the player responsible for testing the volume.
        /// </summary>
        public IAudioPlayer Player { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Player?.Dispose();
            this.Player = null;

            GC.SuppressFinalize(this);
        }
    }
}
