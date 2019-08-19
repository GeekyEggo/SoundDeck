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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Exceptions;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Actions;

    /// <summary>
    /// The main de-coupled entry point for the Sound Deck plugin.
    /// </summary>
    public static class SoundDeckPlugin
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly static object _syncRoot = new object();

        /// <summary>
        /// Gets or sets the running plugin.
        /// </summary>
        private static Task Plugin { get; set; }

        /// <summary>
        /// Runs the Sound Deck plugin asynchronously.
        /// </summary>
        /// <param name="args">The arguments supplied by the console or entry point.</param>
        /// <param name="provider">The service provider.</param>
        /// <returns>The task of running the Sound Deck.</returns>
        public static Task RunAsync(string[] args, IServiceProvider provider)
        {
            lock (_syncRoot)
            {
                if (Plugin == null)
                {
                    Plugin = RunClientIndefinitelyAsync(args, provider);
                }
            }

            return Plugin;
        }

        /// <summary>
        /// Runs the Stream Deck client indefinitely, attempting to recover from exceptions where possible.
        /// </summary>
        /// <param name="clientArgs">The arguments.</param>
        /// <param name="provider">The service provider.</param>
        /// <returns>The task of running the client.</returns>
        private static async Task RunClientIndefinitelyAsync(string[] clientArgs, IServiceProvider provider)
        {
            try
            {
                using (var client = new StreamDeckClient(clientArgs))
                {
                    client.RegisterAction(ClipAudio.UUID, args => provider.GetInstance<ClipAudio>(args));
                    client.RegisterAction(PlayAudio.UUID, args => provider.GetInstance<PlayAudio>(args));
                    client.RegisterAction(RecordAudio.UUID, args => provider.GetInstance<RecordAudio>(args));

                    client.Error += GetErrorEventHandler(client);

                    await client.StartAsync(CancellationToken.None);
                }
            }
            catch
            {
                // restart the client
                await RunClientIndefinitelyAsync(clientArgs, provider);
            }
        }

        /// <summary>
        /// Gets the error event handler used to log any errors encountered during the lifecycle of the <see cref="StreamDeckClient"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The event handler</returns>
        private static EventHandler<StreamDeckClientErrorEventArgs> GetErrorEventHandler(StreamDeckClient client)
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
    }
}
