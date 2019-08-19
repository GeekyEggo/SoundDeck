namespace SoundDeck.Plugin.Models.Settings
{
    using System;
    using Newtonsoft.Json;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Serialization;

    /// <summary>
    /// Provides settings for <see cref="Actions.ClipAudio"/>.
    /// </summary>
    public class ClipAudioSettings : RecordAudioSettings, ISaveBufferSettings
    {
        /// <summary>
        /// Gets or sets the duration of the clip.
        /// </summary>
        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan Duration { get; set; }
    }
}
