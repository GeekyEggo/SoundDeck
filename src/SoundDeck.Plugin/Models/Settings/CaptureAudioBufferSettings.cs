namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Serialization;
    using System;

    /// <summary>
    /// Provides settings for <see cref="Actions.CaptureAudioBuffer"/>.
    /// </summary>
    public class CaptureAudioBufferSettings
    {
        /// <summary>
        /// Gets or sets the audio device identifier to capture.
        /// </summary>
        public string AudioDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the duration of the clip.
        /// </summary>
        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan ClipDuration { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        public string OutputPath { get; set; }
    }
}
