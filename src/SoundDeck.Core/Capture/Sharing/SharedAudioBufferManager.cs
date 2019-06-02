namespace SoundDeck.Core.Capture.Sharing
{
    using Microsoft.Extensions.Logging;
    using NAudio.CoreAudioApi;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// Provides a manager for sharing <see cref="IAudioBuffer"/>.
    /// </summary>
    public class SharedAudioBufferManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SharedAudioBufferManager(ILogger logger)
        {
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; } = new MMDeviceEnumerator();

        /// <summary>
        /// Gets the audio buffers.
        /// </summary>
        private ConcurrentDictionary<string, SharedAudioBufferCollection> AudioBuffers { get; } = new ConcurrentDictionary<string, SharedAudioBufferCollection>();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private ILogger Logger { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var coll in this.AudioBuffers)
            {
                coll.Value.Parent.Dispose();
            }

            this.AudioBuffers.Clear();
            this.DeviceEnumerator.Dispose();
        }

        /// <summary>
        /// Gets the or add an audio buffer for the specified device identifier, and buffer duration.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetOrAddAudioBuffer(string deviceId, TimeSpan bufferDuration)
        {
            var collection = this.AudioBuffers.GetOrAdd(deviceId, _ =>
            {
                var coll = new SharedAudioBufferCollection(this.CreateAudioBuffer(deviceId, bufferDuration));
                coll.CollectionChanged += this.AudioBuffers_CollectionChanged;

                return coll;
            });

            return collection.Add(bufferDuration);
        }

        /// <summary>
        /// Handles the <see cref="INotifyCollectionChanged.CollectionChanged" /> event of the <see cref="AudioBuffers"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void AudioBuffers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // when a child collection has been reset, dispose of the parent
            if (e.Action == NotifyCollectionChangedAction.Reset
                && this.AudioBuffers.TryRemove(((SharedAudioBufferCollection)sender).Parent.DeviceId, out var removedCollection))
            {
                removedCollection.Parent.Dispose();
            }
        }

        /// <summary>
        /// Create an audio buffer for the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <returns>The audio buffer.</returns>
        private AudioBuffer CreateAudioBuffer(string deviceId, TimeSpan bufferDuration)
        {
            var device = this.DeviceEnumerator.GetDevice(deviceId);
            if (device == null)
            {
                throw new KeyNotFoundException($"Unable to find device for the specified device identifier: {deviceId}");
            }

            return new AudioBuffer(device, bufferDuration, this.Logger);
        }
    }
}
