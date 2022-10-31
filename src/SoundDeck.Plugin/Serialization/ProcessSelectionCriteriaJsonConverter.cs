namespace SoundDeck.Plugin.Serialization
{
    using System;
    using Newtonsoft.Json;
    using SoundDeck.Core.Sessions;

    /// <summary>
    /// Provides a base <see cref="JsonConverter{T}"/> that handles the conversion of classes that inherit <see cref="IProcessSelectionCriteria"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class being converted.</typeparam>
    internal abstract class ProcessSelectionCriteriaJsonConverter<T> : JsonConverter<T>
        where T : IProcessSelectionCriteria, new()
    {
        /// <inheritdoc/>
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var settings = new T();
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

                    default:
                        this.ReadJsonProperty(settings, propertyName?.ToString(), reader.Value);
                        break;
                }
            }

            return settings;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            => throw new NotSupportedException();

        /// <summary>
        /// Reads the JSON <paramref name="value"/>, and assign it to the <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        protected abstract void ReadJsonProperty(T settings, string propertyName, object value);
    }
}
