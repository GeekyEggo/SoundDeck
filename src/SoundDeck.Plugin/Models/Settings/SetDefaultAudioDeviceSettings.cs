namespace SoundDeck.Plugin.Models.Settings
{
    using NAudio.CoreAudioApi;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for <see cref="SetDefaultAudioDevice"/>.
    /// </summary>
    public class SetDefaultAudioDeviceSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the role to update.
        /// </summary>
        public Role Role { get; set; }
    }
}
