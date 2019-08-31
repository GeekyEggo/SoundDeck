namespace SoundDeck.Plugin.Windows
{
    using Ookii.Dialogs.Wpf;
    using SoundDeck.Plugin.Models.UI;
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
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = description,
                    SelectedPath = selectedPath,
                    ShowNewFolderButton = true,
                    UseDescriptionForTitle = true,
                };

                var isSelected = dialog.ShowDialog() ?? false;
                return new FolderBrowserDialogResult(isSelected, dialog.SelectedPath);
            });
        }
    }
}
