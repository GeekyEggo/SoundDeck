using SharpDeck.Manifest;
[assembly: StreamDeckPlugin(
    Name = "Sound Deck",
    Category = "Sound Deck",
    CategoryIcon = "Images/SoundDeck/Category",
    Description = "An advanced soundboard with support for output audio device selection. SoundDeck also supports recording audio, or clipping the last precious seconds of audio, from any of your sound devices",
    CodePath = "SoundDeck.Plugin.exe",
    Icon = "Images/SoundDeck/Icon",
    PropertyInspectorPath = "PI/index.html",
    Version = "1.0.0",
    Author = "GeekyEggo",
    WindowsMinimumVersion = "10")]

namespace SoundDeck.Plugin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Exceptions;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Actions;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
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
            if (ManifestWriter.TryWrite(e.Args, out int result))
            {
                Application.Current.Shutdown();
            }
            else
            {
#if DEBUG
                Debugger.Launch();
#endif
                var provider = this.GetServiceProvider();
                await this.RunClientIndefinitelyAsync(e.Args, provider);
            }
        }

        /// <summary>
        /// Runs the Stream Deck client indefinitely, attempting to recover from exceptions where possible.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>The task of running the client.</returns>
        private async Task RunClientIndefinitelyAsync(string[] args, IServiceProvider provider)
        {
            try
            {
                using (var client = new StreamDeckClient(args))
                {
                    client.RegisterAction(ClipAudio.UUID, args => provider.GetInstance<ClipAudio>(args));
                    client.RegisterAction(PlayAudio.UUID, args => provider.GetInstance<PlayAudio>(args));
                    client.RegisterAction(RecordAudio.UUID, args => provider.GetInstance<RecordAudio>(args));

                    client.Error += this.GetErrorEventHandler(client);

                    await client.StartAsync(CancellationToken.None);
                }
            }
            catch
            {
                // restart the client
                await this.RunClientIndefinitelyAsync(args, provider);
            }
        }

        /// <summary>
        /// Gets the error event handler used to log any errors encountered during the lifecycle of the <see cref="StreamDeckClient"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The event handler</returns>
        private EventHandler<StreamDeckClientErrorEventArgs> GetErrorEventHandler(StreamDeckClient client)
        {
            return async (sender, args) =>
            {
                await client.LogMessageAsync(args.Exception.ToString());
                await client.LogMessageAsync(args.WebSocketMessage);

                if (!string.IsNullOrWhiteSpace(args.Context))
                {
                    await client.ShowAlertAsync(args.Context);
                }
            };
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        private IServiceProvider GetServiceProvider()
        {
            var provider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .AddSingleton<IAudioService, AudioService>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
