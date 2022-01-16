namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Volume;

    /// <summary>
    /// Provides settings for an action that is capable of changing the volume.
    /// </summary>
    public class VolumeSettings : IVolumeSettings
    {
        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; set; }

        /// <inheritdoc/>
        public int VolumeValue { get; set; } = 100;
    }
}
