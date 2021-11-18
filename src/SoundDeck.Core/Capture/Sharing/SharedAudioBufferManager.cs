namespace SoundDeck.Core.Capture.Sharing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides a manager for sharing <see cref="IAudioBuffer"/>.
    /// </summary>
    public class SharedAudioBufferManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public SharedAudioBufferManager(ILoggerFactory loggerFactory)
            => this.LoggerFactory = loggerFactory;

        /// <summary>
        /// Gets the audio buffers.
        /// </summary>
        private ConcurrentDictionary<string, SharedAudioBufferCollection> AudioBuffers { get; } = new ConcurrentDictionary<string, SharedAudioBufferCollection>();

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        private ILoggerFactory LoggerFactory { get; }

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
        }

        /// <summary>
        /// Gets the audio buffers that are currently being managed.
        /// </summary>
        /// <returns>The audio buffers.</returns>
        public IEnumerable<IAudioBuffer> GetAudioBuffers()
        {
            foreach (var sharedAudioBuffer in this.AudioBuffers.Values)
            {
                yield return sharedAudioBuffer.Parent;
            }
        }

        /// <summary>
        /// Gets the or add an audio buffer for the specified device key, and buffer duration.
        /// </summary>
        /// <param name="deviceKey">The device key.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer GetOrAddAudioBuffer(string deviceKey, TimeSpan bufferDuration)
        {
            var collection = this.AudioBuffers.GetOrAdd(deviceKey, _ =>
            {
                var coll = new SharedAudioBufferCollection(this.CreateAudioBuffer(deviceKey, bufferDuration));
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
                && this.AudioBuffers.TryRemove(((SharedAudioBufferCollection)sender).Parent.Device.Key, out var removedCollection))
            {
                removedCollection.Parent.Dispose();
            }
        }

        /// <summary>
        /// Create an audio buffer for the specified device key.
        /// </summary>
        /// <param name="deviceKey">The device key.</param>
        /// <param name="bufferDuration">Duration of the buffer.</param>
        /// <returns>The audio buffer.</returns>
        private IAudioBuffer CreateAudioBuffer(string deviceKey, TimeSpan bufferDuration)
        {
            var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
            if (device == null)
            {
                throw new KeyNotFoundException($"Unable to find device for the specified device key: {deviceKey}");
            }

            return new CircularAudioBuffer(device, bufferDuration, this.LoggerFactory.CreateLogger<CircularAudioBuffer>());
        }
    }
}
