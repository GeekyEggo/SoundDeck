namespace SoundDeck.Plugin.Windows
{
    using Microsoft.WindowsAPICodePack.Dialogs;
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
        /// <param name="title">The title of the dialog.</param>
        /// <param name="selectedPath">The selected path.</param>
        /// <returns>The result of the dialog.</returns>
        public FolderBrowserDialogResult ShowDialog(string title, string selectedPath)
        {
            return this.Dispatcher.Invoke(() =>
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.EnsurePathExists = true;
                    dialog.InitialDirectory = selectedPath;
                    dialog.IsFolderPicker = true;
                    dialog.Title = title;

                    return dialog.ShowDialog(StreamDeck.Current.MainWindowHandle) == CommonFileDialogResult.Ok
                        ? new FolderBrowserDialogResult(true, dialog.FileName)
                        : new FolderBrowserDialogResult(false, string.Empty);
                }
            });
        }
    }
}
