namespace SoundDeck.Plugin.Models.Payloads
{
    /// <summary>
    /// Provides a payload containing the index of an audio file to remove from a playlist
    /// </summary>
    public class RemovePlaylistFilePayload
    {
        /// <summary>
        /// Gets or sets the index of the item to remove.
        /// </summary>
        public int Index { get; set; }
    }
}
