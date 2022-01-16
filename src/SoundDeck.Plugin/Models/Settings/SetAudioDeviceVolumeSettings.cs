namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAudioDeviceVolume"/> action.
    /// </summary>
    public class SetAudioDeviceVolumeSettings : VolumeSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }
    }
}
