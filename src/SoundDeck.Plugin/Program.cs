﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoundDeck.Core;
using System;
using System.Threading.Tasks;

namespace SoundDeck.Plugin
{
    class Program
    {
        private const string MUSIC_ID = "{0.0.0.00000000}.{8b029122-b9f1-48a9-94ac-e2d5a718d2d4}";

        static void Main(string[] args)
        {
            var provider = GetServiceProvider();
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Main");

            logger.LogInformation("Buffering");

            var captureProvider = provider.GetRequiredService<IAudioService>();
            foreach (var dev in captureProvider.GetDevices())
            {
                logger.LogInformation($"[{dev.Flow}] {dev.FriendlyName} ({dev.Id})");
            }

            using (var capture = captureProvider.GetBuffer(MUSIC_ID))
            {
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.S:
                            Task.WaitAll(capture.SaveAsync(TimeSpan.FromSeconds(5), @"c:\temp\"));
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
                .AddSingleton<IAudioService, AudioService>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
