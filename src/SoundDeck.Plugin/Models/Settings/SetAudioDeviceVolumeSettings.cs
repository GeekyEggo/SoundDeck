namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAudioDeviceVolume"/> action.
    /// </summary>
    public class SetAudioDeviceVolumeSettings : IVolumeSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; set; }

        /// <inheritdoc/>
        public int VolumeValue { get; set; } = 100;
    }
}
