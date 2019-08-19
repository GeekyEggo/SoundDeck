namespace SoundDeck.Plugin.Models.UI
{
    /// <summary>
    /// Provides information about the result of selecting a folder.
    /// </summary>
    public class FolderBrowserDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderBrowserDialogResult"/> class.
        /// </summary>
        /// <param name="isSelected"><c>true</c> when a folder was selected; otherwise <c>false</c>.</param>
        /// <param name="selectedPath">The selected path.</param>
        public FolderBrowserDialogResult(bool isSelected, string selectedPath)
        {
            this.IsSelected = isSelected;
            this.SelectedPath = selectedPath;
        }

        /// <summary>
        /// Gets a value indicating whether a folder was selected.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// Gets the selected path.
        /// </summary>
        public string SelectedPath { get; }
    }
}
