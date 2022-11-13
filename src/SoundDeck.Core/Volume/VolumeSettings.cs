﻿namespace SoundDeck.Core.Volume
{
    /// <summary>
    /// Provides information about a volume adjustment.
    /// </summary>
    public class VolumeSettings : IVolumeSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeSettings"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="value">The value.</param>
        public VolumeSettings(VolumeAction action, int value = 0)
        {
            this.VolumeAction = action;
            this.VolumeValue = value;
        }

        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; }

        /// <inheritdoc/>
        public int VolumeValue { get; }
    }
}
