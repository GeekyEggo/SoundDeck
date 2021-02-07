namespace SoundDeck.Plugin.Models.Payloads
{
    using SoundDeck.Core.Playback;

    /// <summary>
    /// Provides a payload containing the index of the file, and its new volume.
    /// </summary>
    public class AdjustPlaylistFileVolumePayload : AudioFileInfo
    {
        /// <summary>
        /// Gets or sets the index of the file whos volume is being adjusted.
        /// </summary>
        public int Index { get; set; }
    }
}
