namespace SoundDeck.Plugin.Serialization
{
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides a <see cref="JsonConverter{T}"/> for <see cref="SetAppAudioDeviceSettings"/>.
    /// </summary>
    internal class SetAppAudioDeviceSettingsJsonConverter : ProcessSelectionCriteriaJsonConverter<SetAppAudioDeviceSettings>
    {
        protected override void ReadJsonProperty(SetAppAudioDeviceSettings settings, string propertyName, object value)
        {
            if (propertyName == "audioDeviceId")
            {
                settings.AudioDeviceId = value?.ToString();
            }
        }
    }
}
