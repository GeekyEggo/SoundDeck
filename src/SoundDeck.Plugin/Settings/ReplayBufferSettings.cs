namespace SoundDeck.Plugin.Settings
{
    using System;
    
    public class ReplayBufferSettings
    {
        private const string MUSIC_ID = "{0.0.0.00000000}.{8b029122-b9f1-48a9-94ac-e2d5a718d2d4}";

        public string AudioDeviceId { get; set; } = MUSIC_ID;
        public TimeSpan ClipDuration { get; set; } = TimeSpan.FromSeconds(15);
        public string OutputPath { get; set; } = @"C:\Temp\";
    }
}
