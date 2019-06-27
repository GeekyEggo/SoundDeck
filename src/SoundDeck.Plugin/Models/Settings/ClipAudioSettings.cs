namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.Serialization;
    using System;

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
