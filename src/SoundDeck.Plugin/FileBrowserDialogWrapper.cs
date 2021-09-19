namespace SoundDeck.Plugin.Windows
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Provides a Windows implementation of <see cref="IFileDialogProvider"/>.
    /// </summary>
    public class FileBrowserDialogWrapper : IFileDialogProvider
    {
        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        private Dispatcher Dispatcher => Application.Current.Dispatcher;

        /// <summary>
        /// Shows a file browser dialog.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="filter">The file filter.</param>
        /// <returns>The selected files names.</returns>
        public string[] ShowOpenDialog(string title, string filter)
        {
            return this.Dispatcher.Invoke(() =>
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    this.AddFilters(dialog.Filters, filter);
                    dialog.EnsurePathExists = true;
                    dialog.Multiselect = true;
                    dialog.Title = title;

                    return dialog.ShowDialog(StreamDeck.Current.MainWindowHandle) == CommonFileDialogResult.Ok
                        ? dialog.FileNames.ToArray()
                        : new string[0];
                }
            });
        }

        /// <summary>
        /// Parses the <see cref="CommonFileDialogFilter"/> from the specified <paramref name="filter"/> and adds them to the <see cref="filters"/> collection.
        /// </summary>
        /// <param name="filters">The filters collection to add the parsed filters to.</param>
        /// <param name="filter">The filter to parse.</param>
        private void AddFilters(CommonFileDialogFilterCollection filters, string filter)
        {
            var segments = filter.Split('|');
            for (var i = 0; i < segments.Length; i += 2)
            {
                filters.Add(new CommonFileDialogFilter(segments[i], segments[i + 1]));
            }
        }
    }
}
