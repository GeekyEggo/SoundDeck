namespace SoundDeck.Plugin.Windows
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using SharpDeck.Extensions;
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
            _ = ActivatorUtilities.CreateInstance<SoundDeckPlugin>(this.GetServiceProvider())
                .RunAsync();
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        private IServiceProvider GetServiceProvider()
        {
            // Setup the configuration to read from the App.config file.
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // Build the service provider.
            return new ServiceCollection()
                .AddScoped<IConfiguration>(_ => config)
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders()
                        .AddConfiguration(config.GetSection("Logging"))
                        .AddNLog(new NLogLoggingConfiguration(config.GetSection("NLog")));
                })
                .AddSingleton<IAudioService, AudioService>()
                .AddSingleton<IAppAudioService, AppAudioService>()
                .AddSingleton<IFileDialogProvider, FileBrowserDialogWrapper>()
                .AddSingleton<IFolderBrowserDialogProvider, FolderBrowserDialogWrapper>()
                .AddStreamDeckPlugin(plugin => plugin.Assembly = Assembly.GetAssembly(typeof(SoundDeckPlugin)))
                .BuildServiceProvider();
        }
    }
}
