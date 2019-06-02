namespace SoundDeck.Core.Serialization
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Provides conversion from <see cref="TimeSpan"/> to JSON, and back. The conversion is in seconds.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return TimeSpan.FromSeconds((long)reader.Value);
            }
            else if (reader.TokenType == JsonToken.String
                && long.TryParse(reader.Value.ToString(), out var val))
            {
                return TimeSpan.FromSeconds(val);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
            => writer.WriteValue(value.Seconds);
    }
}
