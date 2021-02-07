namespace SoundDeck.Plugin.Models.Payloads
{
    /// <summary>
    /// Provides a payload containing an array of files to be added to a playlist.
    /// </summary>
    public class AddPlaylistFilesPayload
    {
        /// <summary>
        /// Gets or sets the files to be added to the playlist.
        /// </summary>
        public string[] Files { get; set; }
    }
}
