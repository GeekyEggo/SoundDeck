namespace SoundDeck.Core
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using NAudio.MediaFoundation;
    using NAudio.Wave;
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
            this.DeviceEnumerator = new MMDeviceEnumerator();
            this.AudioBufferManager = new AudioBufferManager(logger);
        }

        /// <summary>
        /// Gets the audio buffer manager.
        /// </summary>
        private AudioBufferManager AudioBufferManager { get; }

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
        /// Gets the active audio devices.
        /// </summary>
        /// <returns>The audio devices</returns>
        public IEnumerable<AudioDevice> GetDevices()
            => this.DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).Select(m => new AudioDevice(m));

        /// <summary>
        /// Registers a new audio buffer.
        /// </summary>
        /// <param name="deviceId">The audio device identifier.</param>
        /// <param name="clipDuration">The clip duration for the buffer.</param>
        /// <returns>The registration.</returns>
        public AudioBufferRegistration RegisterBufferListener(string deviceId, TimeSpan clipDuration)
            => this.AudioBufferManager.Register(deviceId, clipDuration);

        /// <summary>
        /// Unregisters the audio buffer.
        /// </summary>
        /// <param name="registration">The registration.</param>
        public void UnregisterBufferListener(AudioBufferRegistration registration)
            => this.AudioBufferManager.Unregister(registration);
    }
}
