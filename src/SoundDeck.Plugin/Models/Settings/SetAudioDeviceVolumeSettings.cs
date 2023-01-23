namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Volume;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAudioDeviceVolume"/> action.
    /// </summary>
    public class SetAudioDeviceVolumeSettings : IVolumeSettings
    {
        /// <summary>
        /// Gets or sets the audio device key used to identify the audio device.
        /// </summary>
        [JsonProperty("audioDeviceId")]
        public string AudioDeviceKey { get; set; }

        /// <inheritdoc/>
        public VolumeAction VolumeAction { get; set; }

        /// <inheritdoc/>
        public int VolumeValue { get; set; } = 100;
    }
}
