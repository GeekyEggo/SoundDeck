namespace SoundDeck.Plugin.Windows
{
    using SoundDeck.Plugin.Models.UI;
    using System.Windows.Forms;
    using System.Windows.Threading;

    /// <summary>
    /// Provides a Windows implementation of a <see cref="IFolderBrowserDialogProvider"/>
    /// </summary>
    public class FolderBrowserDialogWrapper : IFolderBrowserDialogProvider
    {
        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        private Dispatcher Dispatcher => System.Windows.Application.Current.Dispatcher;

        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="selectedPath">The selected path.</param>
        /// <returns>The result of the dialog.</returns>
        public FolderBrowserDialogResult ShowDialog(string description, string selectedPath)
        {
            return this.Dispatcher.Invoke(() =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = description;
                    dialog.SelectedPath = selectedPath;

                    return new FolderBrowserDialogResult(
                        dialog.ShowDialog() == DialogResult.OK,
                        dialog.SelectedPath);
                }
            });
        }
    }
}
