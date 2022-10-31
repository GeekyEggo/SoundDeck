namespace SoundDeck.Plugin.Serialization
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides a <see cref="JsonConverter{T}"/> for <see cref="AppMultimediaControlsSettings"/>.
    /// </summary>
    internal class AppMultimediaControlsSettingsJsonConverter : ProcessSelectionCriteriaJsonConverter<AppMultimediaControlsSettings>
    {
        /// <inheritdoc/>
        protected override void ReadJsonProperty(AppMultimediaControlsSettings settings, string propertyName, object value)
        {
            if (propertyName == "action"
                && int.TryParse(value?.ToString(), out var action))
            {
                settings.Action = (MultimediaAction)action;
            }
        }
    }
}
