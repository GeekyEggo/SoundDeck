namespace SoundDeck.Core.Capture
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a manager for a collection of <see cref="IAudioBuffer"/>.
    /// </summary>
    public sealed class AudioBufferManager : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AudioBufferManager(ILogger logger)
        {
            this.Logger = logger;
        }

        private ConcurrentDictionary<string, AudioBufferListeners> AudioBuffers { get; } = new ConcurrentDictionary<string, AudioBufferListeners>();

        /// <summary>
        /// Gets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; } = new MMDeviceEnumerator();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
            => this.DeviceEnumerator.Dispose();

        /// <summary>
        /// Registers an audio buffer listener for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">The duration of a clip.</param>
        /// <returns>The registration.</returns>
        public AudioBufferRegistration Register(string deviceId, TimeSpan clipDuration)
        {
            // add the new registration, constructing a new audio buffer if required
            var newRegistration = new AudioBufferRegistration(Guid.NewGuid(), clipDuration);
            var listeners = this.AudioBuffers.GetOrAdd(deviceId, _ => new AudioBufferListeners(this.GetBuffer(deviceId, clipDuration)));
            listeners.Add(newRegistration);
            
            return newRegistration;
        }

        /// <summary>
        /// Unregisters the specified registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        public void Unregister(AudioBufferRegistration registration)
        {
            if (!this.AudioBuffers.TryGetValue(registration.AudioBuffer.DeviceId, out var observers)
                || !observers.Remove(registration.Id))
            {
                return;
            }

            if (observers.Count == 0)
            {
                // when there are no observers left, remove the audio buffer
                observers.AudioBuffer.Dispose();
                this.AudioBuffers.TryRemove(observers.AudioBuffer.DeviceId, out var _);
            }
            else
            {
                // otherwise update the buffer duration
                observers.RefreshBufferDuration();
            }
        }

        /// <summary>
        /// Attempts to get the audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="clipDuration">The duration of the buffer.</param>
        /// <returns>The audio buffer.</returns>
        private IConfigurableAudioBuffer GetBuffer(string deviceId, TimeSpan clipDuration)
        {
            var device = this.DeviceEnumerator.GetDevice(deviceId);
            if (device == null)
            {
                throw new KeyNotFoundException($"Unable to find device for the specified device identifier: {deviceId}");
            }

            return new AudioBuffer(device, clipDuration, this.Logger);
            
        }
    }
}
