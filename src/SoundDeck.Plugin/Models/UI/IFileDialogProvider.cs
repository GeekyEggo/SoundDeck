namespace SoundDeck.Plugin.Models.UI
{
    /// <summary>
    /// Provides methods for showing a file browser dialog.
    /// </summary>
    public interface IFileDialogProvider
    {
        /// <summary>
        /// Shows a file browser dialog.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="filter">The file filter.</param>
        /// <returns>The selected files names.</returns>
        string[] ShowOpenDialog(string title, string filter);
    }
}
