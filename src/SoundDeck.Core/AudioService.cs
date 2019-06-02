namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
    using SoundDeck.Core.Capture.Sharing;
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
            this.DeviceEnumerator = new MMDeviceEnumerator();
            this.SharedAudioBufferManager = new SharedAudioBufferManager(logger);
        }

        /// <summary>
        /// Gets the audio buffer manager.
        /// </summary>
        private SharedAudioBufferManager SharedAudioBufferManager { get; }

        /// <summary>
        /// Gets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; }
        
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
        /// Gets an audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetAudioBuffer(string deviceId, TimeSpan clipDuration)
            => this.SharedAudioBufferManager.GetOrAddAudioBuffer(deviceId, clipDuration);

        /// <summary>
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        public IEnumerable<AudioDevice> GetDevices()
            => this.DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).Select(m => new AudioDevice(m));
    }
}
