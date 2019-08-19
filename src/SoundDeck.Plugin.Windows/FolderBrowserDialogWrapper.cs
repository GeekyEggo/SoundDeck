namespace SoundDeck.Plugin.Windows
{
    using System.Windows.Forms;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides a Windows implementation of a <see cref="IFolderBrowserDialogProvider"/>
    /// </summary>
    public class FolderBrowserDialogWrapper : IFolderBrowserDialogProvider
    {
        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="selectedPath">The selected path.</param>
        /// <returns>The result of the dialog.</returns>
        public FolderBrowserDialogResult ShowDialog(string description, string selectedPath)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.SelectedPath = selectedPath;

                return new FolderBrowserDialogResult(
                    dialog.ShowDialog() == DialogResult.OK,
                    dialog.SelectedPath);
            }
        }
    }
}
