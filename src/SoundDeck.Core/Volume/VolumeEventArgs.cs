namespace SoundDeck.Core.Volume
{
    using System;

    /// <summary>
    /// Provides information about a volume of a device or application.
    /// </summary>
    public class VolumeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeEventArgs"/> class.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="isMuted">Value indicating whether the volume is muted.</param>
        public VolumeEventArgs(float volume, bool isMuted)
        {
            this.Volume = volume;
            this.IsMuted = isMuted;
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        public float Volume { get; }

        /// <summary>
        /// Gets a value indicating whether the volume is muted.
        /// </summary>
        public bool IsMuted { get; }
    }
}
