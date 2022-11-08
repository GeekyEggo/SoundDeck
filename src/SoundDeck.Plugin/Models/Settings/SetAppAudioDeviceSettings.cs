namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppAudioDeviceSettings : ProcessSelectionCriteriaSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }
    }
}
