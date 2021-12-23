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

        /// <inheritdoc/>
        public string ProcessName { get; set; }

        /// <inheritdoc/>
        public ProcessSelectionType ProcessSelectionType { get; set; }
    }
}
