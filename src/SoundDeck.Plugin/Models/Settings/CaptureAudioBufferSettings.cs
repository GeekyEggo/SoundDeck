namespace SoundDeck.Plugin.Models.Settings
{
    using Newtonsoft.Json;
    using SoundDeck.Core.Serialization;
    using System;

    public class CaptureAudioBufferSettings
    {
        private const string MUSIC_ID = "{0.0.0.00000000}.{8b029122-b9f1-48a9-94ac-e2d5a718d2d4}";
        private const string SAMPLE_IN = "{0.0.1.00000000}.{7be9d233-fc82-4185-8fbf-c14484837ad7}";
        private const string SAMPLE_OUT = "{0.0.0.00000000}.{4c654909-8c5f-45ed-b8af-cc761fd206fe}";
        private const string REALTEK = "{0.0.0.00000000}.{25600823-42b8-4e6b-a945-dfb335ba5b7f}";

        public string AudioDeviceId { get; set; } = SAMPLE_IN;

        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan ClipDuration { get; set; } = TimeSpan.FromSeconds(15);

        public string OutputPath { get; set; } = @"C:\Temp\";
    }
}
