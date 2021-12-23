namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for <see cref="AppMultimediaControls"/>.
    /// </summary>
    public class AppMultimediaControlsSettings
    {
        /// <summary>
        /// Gets or sets the action to apply.
        /// </summary>
        public MultimediaAction Action { get; set; } = MultimediaAction.TogglePlayPause;

        /// <summary>
        /// Gets or sets the process name to search for.
        /// </summary>
        public string ProcessName { get; set; }
    }
}
