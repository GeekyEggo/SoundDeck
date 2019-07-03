namespace SoundDeck.Core.Extensions
{
    using SoundDeck.Core.Capture;
    using SoundDeck.Core.IO;

    /// <summary>
    /// Provides extensions for <see cref="ISaveAudioSettings"/>.
    /// </summary>
    public static class SaveAudioSettingsExtensions
    {
        /// <summary>
        /// Gets the path for this instance.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The file path.</returns>
        public static string GetPath(this ISaveAudioSettings settings)
        {
            var ext = settings.EncodeToMP3 ? ".mp3" : ".wav";
            return FileUtils.GetTimeStampPath(settings.OutputPath, $"{{0}}{ext}");
        }
    }
}
