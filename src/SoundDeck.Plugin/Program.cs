namespace SoundDeck.Plugin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SoundDeck.Core;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Plugin.Actions;
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class Program
    {
        public class SomeSettings
        {
            public AudioDevice[] Devices { get; set; }
        }

        public static void Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif

            var provider = GetServiceProvider();
            using (var client = new StreamDeckClient(args))
            {
                client.RegisterAction("com.geekyEggo.soundDeckCaptureAudioBuffer", () => provider.GetInstance<CaptureAudioBuffer>());
                client.Registered += (s, o) =>
                {
                    var settings = new SomeSettings
                    {
                        Devices = provider.GetRequiredService<IAudioService>().GetDevices().ToArray()
                    };

                    client.SetGlobalSettingsAsync(settings);
                };

                client.Start();
            }

            /*                                 
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Main");
            logger.LogInformation("Buffering");

            var captureProvider = provider.GetRequiredService<IAudioService>();
            foreach (var dev in captureProvider.GetDevices())
            {
                logger.LogInformation($"[{dev.Flow}] {dev.FriendlyName} ({dev.Id})");
            }

            using (var capture = captureProvider.GetBuffer(SAMPLE_IN))
            {
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.S:
                            Task.WaitAll(capture.SaveAsync(TimeSpan.FromSeconds(15), @"c:\temp\"));
                            break;
                    }
                }
                while (key.Key != ConsoleKey.X);
                logger.LogInformation("Done");
            }
            */
        }

        private static void Client_ApplicationDidLaunch(object sender, StreamDeckEventArgs<ApplicationPayload> e)
        {
        }

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
