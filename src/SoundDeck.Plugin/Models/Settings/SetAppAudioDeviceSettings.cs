namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for the <see cref="SetAppAudioDevice"/> action.
    /// </summary>
    public class SetAppAudioDeviceSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the process to change.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the type that defines how the process is selected.
        /// </summary>
        public ProcessSelectionType ProcessSelectionType { get; set; } = ProcessSelectionType.Foreground;
    }
}
