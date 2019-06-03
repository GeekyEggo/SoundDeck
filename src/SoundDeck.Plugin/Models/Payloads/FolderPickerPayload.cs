namespace SoundDeck.Plugin.Models.Payloads
{
    using SharpDeck.PropertyInspectors;

    /// <summary>
    /// Provides payload information for the selection of a folder.
    /// </summary>
    public class FolderPickerPayload : PropertyInspectorPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPickerPayload"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public FolderPickerPayload(string path = "", bool success = true)
        {
            this.Path = path;
            this.Success = success;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a folder was picked.
        /// </summary>
        public bool Success { get; set; }
    }
}
