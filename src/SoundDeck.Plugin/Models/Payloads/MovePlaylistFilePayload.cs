namespace SoundDeck.Plugin.Models.Payloads
{
    /// <summary>
    /// Provides a payload containing the old and new indexes of an item being moved within a playlist.
    /// </summary>
    public class MovePlaylistFilePayload
    {
        /// <summary>
        /// Gets or sets the old index of the item being moved.
        /// </summary>
        public int OldIndex { get; set; }

        /// <summary>
        /// Gets or sets the new index of the item being moved.
        /// </summary>
        public int NewIndex { get; set; }
    }
}
