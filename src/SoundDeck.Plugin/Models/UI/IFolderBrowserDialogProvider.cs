namespace SoundDeck.Plugin.Models.UI
{
    /// <summary>
    /// A folder browser dialog provider.
    /// </summary>
    public interface IFolderBrowserDialogProvider
    {
        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="selectedPath">The selected path.</param>
        /// <returns>The result of the dialog.</returns>
        FolderBrowserDialogResult ShowDialog(string title, string selectedPath);
    }
}
