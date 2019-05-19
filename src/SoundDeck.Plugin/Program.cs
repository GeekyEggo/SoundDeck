using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundDeck.Core;
using SoundDeck.Core.NAudio;
using System;
using System.Threading.Tasks;

namespace SoundDeck.Plugin
{
    class Program
    {
        static void Main(string[] args)
        {
            AudioAmplifier.Normalize(
                @"C:\Temp\2019-05-18_222200.wav",
                @"C:\Temp\2019-05-18_222200_2.wav");

            return;

            var provider = GetServiceProvider();
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Main");

            logger.LogInformation("Buffering");

            var captureProvider = provider.GetRequiredService<ICaptureProvider>();
            foreach (var dev in captureProvider.GetDevices())
            {
                logger.LogInformation($"[{dev.Flow}] {dev.FriendlyName} ({dev.Id})");
            }

            var capture = captureProvider.GetCapture("{0.0.1.00000000}.{7be9d233-fc82-4185-8fbf-c14484837ad7}");
            using (var buffer = new AudioBuffer(capture, @"c:\Temp\", TimeSpan.FromSeconds(10), logger))
            {
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.S:
                            Task.WaitAll(buffer.SaveAsync(TimeSpan.FromSeconds(5)));
                            break;
                    }
                }
                while (key.Key != ConsoleKey.X);
                logger.LogInformation("Done");
            }
        }

        private static IServiceProvider GetServiceProvider()
        {
            var provider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .AddSingleton<ICaptureProvider, CaptureProvider>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
