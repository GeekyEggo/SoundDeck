namespace SoundDeck.Plugin.Serialization
{
    using System;
    using Newtonsoft.Json;
    using SoundDeck.Core.Sessions;
    using SoundDeck.Plugin.Models.Settings;

    /// <summary>
    /// Provides a <see cref="JsonConverter{T}"/> for <see cref="AppMultimediaControlsSettings"/>.
    /// </summary>
    internal class AppMultimediaControlsSettingsJsonConverter : JsonConverter<AppMultimediaControlsSettings>
    {
        /// <inheritdoc/>
        public override AppMultimediaControlsSettings ReadJson(JsonReader reader, Type objectType, AppMultimediaControlsSettings existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var settings = new AppMultimediaControlsSettings();
            if (reader.TokenType != JsonToken.StartObject)
            {
                return settings;
            }

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                var propertyName = reader.Value;
                if (!reader.Read())
                {
                    continue;
                }

                switch (propertyName)
                {
                    case string name when name == "action" && reader.TokenType == JsonToken.Integer:
                        settings.Action = (MultimediaAction)(int)(long)reader.Value;
                        break;
                    case string name when name == "processName" && reader.TokenType == JsonToken.String:
                        settings.ProcessName = reader.Value?.ToString();
                        break;
                    case string name when name == "processSelectionType" && reader.TokenType == JsonToken.Integer:
                        settings.ProcessSelectionType = (ProcessSelectionType)reader.ReadAsInt32();
                        break;
                    case string name when name == "processSelectionType" && reader.TokenType == JsonToken.String:
                        if (int.TryParse(reader.Value?.ToString(), out var val))
                        {
                            settings.ProcessSelectionType = (ProcessSelectionType)val;
                        }
                        else
                        {
                            settings.ProcessSelectionType = ProcessSelectionType.ByName;
                            settings.ProcessName = reader.Value?.ToString();
                        }

                        break;
                }
            }

            return settings;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, AppMultimediaControlsSettings value, JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
