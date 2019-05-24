namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Capture;
    using System;
    using System.Collections.Concurrent;
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
        /// Gets the buffers.
        /// </summary>
        private ConcurrentDictionary<string, IAudioBuffer> Buffers { get; } = new ConcurrentDictionary<string, IAudioBuffer>();

        /// <summary>
        /// Gets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger<AudioService> Logger { get; }

        /// <summary>
        /// Determines whether encoding to MP3 is possible based on the current environment.
        /// </summary>
        /// <returns><c>true</c> when encoding is possible; otherwise <c>false</c>.</returns>
        public bool CanEncodeToMP3()
            => MediaFoundationEncoder.SelectMediaType(AudioSubtypes.MFAudioFormat_MP3, Constants.DefaultWaveFormat, Constants.DesiredBitRate) != null;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
            => this.DeviceEnumerator.Dispose();

        /// <summary>
        /// Attempts to get the audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetBuffer(string deviceId)
        {
            return this.Buffers.GetOrAdd(deviceId, _ =>
            {
                var device = this.DeviceEnumerator.GetDevice(deviceId);
                if (device == null)
                {
                    throw new KeyNotFoundException($"Unable to find device for the specified device identifier: {deviceId}");
                }

                return new AudioBuffer(device, TimeSpan.FromSeconds(45), this.Logger);
            });
        }

        /// <summary>
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        public IEnumerable<AudioDevice> GetDevices()
            => this.DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).Select(m => new AudioDevice(m));
    }
}
