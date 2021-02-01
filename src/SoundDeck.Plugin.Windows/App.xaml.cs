namespace SoundDeck.Plugin.Windows
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Handles the Startup event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            Debugger.Launch();
#endif

            _ = SoundDeckPlugin.RunAsync(this.GetServiceProvider());
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        private IServiceProvider GetServiceProvider()
        {
            var provider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IAudioService, AudioService>()
                .AddSingleton<IAppAudioService, AppAudioService>()
                .AddSingleton<IFolderBrowserDialogProvider, FolderBrowserDialogWrapper>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
