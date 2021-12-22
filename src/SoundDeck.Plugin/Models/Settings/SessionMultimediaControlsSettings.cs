namespace SoundDeck.Plugin.Models.Settings
{
    using SoundDeck.Core;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// Provides settings for <see cref="SessionMultimediaControls"/>.
    /// </summary>
    public class SessionMultimediaControlsSettings
    {
        /// <summary>
        /// Gets or sets the action to apply.
        /// </summary>
        public MultimediaAction Action { get; set; } = MultimediaAction.TogglePlayPause;

        /// <summary>
        /// Gets or sets the search criteria to match against the session.
        /// </summary>
        public string SearchCriteria { get; set; }
    }
}
