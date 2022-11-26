namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for <see cref="AppMultimediaControls"/>.
    /// </summary>
    public class AppMultimediaControlsSettings : ProcessSelectionCriteriaSettings
    {
        /// <summary>
        /// Gets or sets the action to apply.
        /// </summary>
        public MultimediaAction Action { get; set; } = MultimediaAction.TogglePlayPause;

        /// <summary>
        /// Gets or sets the preferred icon.
        /// </summary>
        public MediaSessionIconType PreferredIcon { get; set; } = MediaSessionIconType.App;
    }
}
