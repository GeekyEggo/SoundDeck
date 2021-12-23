namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for <see cref="AppMultimediaControls"/>.
    /// </summary>
    public class AppMultimediaControlsSettings : IProcessSelectionCriteria
    {
        /// <summary>
        /// Gets or sets the action to apply.
        /// </summary>
        public MultimediaAction Action { get; set; } = MultimediaAction.TogglePlayPause;

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
