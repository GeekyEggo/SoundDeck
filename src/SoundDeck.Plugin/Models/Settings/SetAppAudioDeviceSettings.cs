namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppAudioDeviceSettings : IProcessSelectionCriteria
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <inheritdoc/>
        public string ProcessName { get; set; }

        /// <inheritdoc/>
        public ProcessSelectionType ProcessSelectionType { get; set; }
    }
}
