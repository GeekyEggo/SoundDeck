namespace SoundDeck.Plugin.Windows
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using SharpDeck.Extensions.Hosting;
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
            _ = new HostBuilder()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .AddNLog(new NLogLoggingConfiguration(hostingContext.Configuration.GetSection("NLog")));
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<IAudioService, AudioService>()
                        .AddSingleton<IAppAudioService, AppAudioService>()
                        .AddSingleton<IFileDialogProvider, FileBrowserDialogWrapper>()
                        .AddSingleton<IFolderBrowserDialogProvider, FolderBrowserDialogWrapper>()
                        .AddSingleton<IHostedService, AudioBufferRestartService>();
                })
                .UseStreamDeck()
                .StartAsync();
        }
    }
}
