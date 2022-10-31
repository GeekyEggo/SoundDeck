namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Actions;
    using SoundDeck.Plugin.Serialization;

    /// <summary>
    /// Provides settings for <see cref="AppMultimediaControls"/>.
    /// </summary>
    [JsonConverter(typeof(AppMultimediaControlsSettingsJsonConverter))]
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
