using SharpDeck.Manifest;
[assembly: StreamDeckPlugin(
    Name = "Sound Deck",
    Category = "Sound Deck",
    Description = "Sound capturing, buffering, and sound deck for Elgato Stream Deck.",
    Icon = "Images/Icon",
    CodePath = "SoundDeck.Plugin.exe",
    Version = "1.0.0",
    Author = "GeekyEggo",
    WindowsMinimumVersion = "10")]

namespace SoundDeck.Plugin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Actions;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            Debugger.Launch();
#endif

            if (ManifestWriter.TryWrite(e.Args, out int result))
            {
                Application.Current.Shutdown();
            }
            else
            {
                var provider = GetServiceProvider();
                using (var client = new StreamDeckClient(e.Args))
                {
                    client.RegisterAction("com.geekyEggo.soundDeckCaptureAudioBuffer", () => provider.GetInstance<CaptureAudioBuffer>());
                    await client.StartAsync(CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        private static IServiceProvider GetServiceProvider()
        {
            var provider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .AddSingleton<IAudioService, AudioService>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
