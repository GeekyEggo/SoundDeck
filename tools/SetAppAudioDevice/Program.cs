namespace SetAppAudioDevice
{
    using System;
    using SoundDeck.Core;
    using SoundDeck.Core.Interop;

    /// <summary>
    /// The main application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                var options = new Options(args);
                var appAudioService = new AppAudioService(null);

                // Determine the device identifier.
                options.Device = string.IsNullOrWhiteSpace(options.Device)
                    ? options.Device = AudioDevices.PLAYBACK_DEFAULT
                    : GetDeviceId(options.Device);

                if (string.IsNullOrWhiteSpace(options.Process))
                {
                    // No process; assume foreground.
                    appAudioService.SetDefaultAudioDeviceForForegroundApp(AudioFlowType.Playback, options.Device);
                }
                else
                {
                    // Set the specified process.
                    appAudioService.SetDefaultAudioDevice(options.Process, AudioFlowType.Playback, options.Device);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Parses the device identifier from the <see cref="Options.Device"/>.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns>The device identifier; otherwise an <see cref="Exception"/> is thrown.</returns>
        public static string GetDeviceId(string device)
        {
            foreach (var audioDevice in AudioDevices.Current)
            {
                if (audioDevice.FriendlyName.ToLowerInvariant().Contains(device.ToLowerInvariant()))
                {
                    return audioDevice.Id;
                }
            }

            throw new Exception($"No audio device found matching \"{device}\"");
        }
    }
}
