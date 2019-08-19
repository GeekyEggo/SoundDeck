namespace SoundDeck.Plugin.Windows
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Extensions.DependencyInjection;
    using SharpDeck.Manifest;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.UI;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The Windows entry point for the application.
        /// </summary>
        /// <param name="args">The entry arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if (ManifestWriter.TryWrite(args, out int result))
            {
                return;
            }
            else
            {
#if DEBUG
                Debugger.Launch();
#endif

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var plugin = SoundDeckPlugin.RunAsync(args, GetServiceProvider());
                Application.Run(new ApplicationContext());

                Task.WaitAll(plugin);
            }
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        private static IServiceProvider GetServiceProvider()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IAudioService, AudioService>()
                .AddSingleton<IFolderBrowserDialogProvider, FolderBrowserDialogWrapper>()
                .BuildServiceProvider();

            return provider;
        }
    }
}
