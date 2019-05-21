namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Capture;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides a service for interacting with local audio devices.
    /// </summary>
    public sealed class AudioService : IAudioService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AudioService(ILogger<AudioService> logger)
        {
            this.Logger = logger;
            this.DeviceEnumerator = new MMDeviceEnumerator();
        }

        /// <summary>
        /// Gets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<AudioService> Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.DeviceEnumerator.Dispose();
        }

        /// <summary>
        /// Attempts to get the audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetBuffer(string deviceId)
        {
            try
            {
                var device = this.DeviceEnumerator.GetDevice(deviceId);
                return new AudioBuffer(device, TimeSpan.FromSeconds(10), this.Logger);
            }
            catch
            {
                throw new KeyNotFoundException($"Unable to find device for the specified device identifier: {deviceId}");
            }
        }

        /// <summary>
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        public IEnumerable<AudioDevice> GetDevices()
            => this.DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).Select(m => new AudioDevice(m));
    }
}
