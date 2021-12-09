namespace SoundDeck.Core.Capture.Sharing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides information and methods for controlling a shared instance of <see cref="IAudioBuffer"/>.
    /// </summary>
    public class SharedAudioBuffer
    {
        /// <summary>
        /// The synchrinization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of class <see cref="SharedAudioBuffer"/>.
        /// </summary>
        /// <param name="deviceKey">The device key of the audio device.</param>
        /// <param name="bufferDuration">The initial buffer duration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public SharedAudioBuffer(string deviceKey, TimeSpan bufferDuration, ILoggerFactory loggerFactory)
        {
            var device = AudioDevices.Current.GetDeviceByKey(deviceKey);
            if (device == null)
            {
                throw new KeyNotFoundException($"Unable to find device for the specified device key: {deviceKey}");
            }

            this.AudioBuffer = new CircularAudioBuffer(device, bufferDuration, loggerFactory.CreateLogger<CircularAudioBuffer>());
        }

        /// <summary>
        /// Gets the underlying audio buffer.
        /// </summary>
        public CircularAudioBuffer AudioBuffer { get; }

        /// <summary>
        /// Gets the consumers currently consuming the audio buffer.
        /// </summary>
        private List<SharedAudioBufferConsumer> Consumers { get; } = new List<SharedAudioBufferConsumer>();

        /// <summary>
        /// Registers a new consumer with the required duration.
        /// </summary>
        /// <param name="bufferDuration">The required buffer duration.</param>
        /// <returns>The audio buffer for the consumer to manage.</returns>
        public IAudioBuffer Register(TimeSpan bufferDuration)
        {
            lock (this._syncRoot)
            {
                var consumer = new SharedAudioBufferConsumer(this.AudioBuffer, bufferDuration);
                consumer.BufferDurationChanged += this.Consumer_BufferDurationChanged;
                consumer.Disposed += this.Consumer_Disposed;
                this.Consumers.Add(consumer);

                this.SetBufferDuration();
                return consumer;
            }
        }

        /// <summary>
        /// Handles <see cref="SharedAudioBufferConsumer.Disposed"/>; handlers and the consumer is removed from this instance, and the underlying buffer duration is updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Consumer_Disposed(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                if (sender is SharedAudioBufferConsumer consumer)
                {
                    consumer.BufferDurationChanged -= this.Consumer_BufferDurationChanged;
                    consumer.Disposed -= this.Consumer_Disposed;
                    this.Consumers.Remove(consumer);

                    this.SetBufferDuration();
                }
            }
        }

        /// <summary>
        /// Handles <see cref="SharedAudioBufferConsumer.BufferDurationChanged"/>, updating the underlying buffer duration if required.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Consumer_BufferDurationChanged(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                this.SetBufferDuration();
            }
        }

        /// <summary>
        /// Refreshes the <see cref="IAudioBuffer.BufferDuration"/> of the <see cref="AudioBuffer"/> based on the consumers requirements.
        /// </summary>
        private void SetBufferDuration()
        {
            this.AudioBuffer.BufferDuration = this.Consumers.Count > 0
                ? this.Consumers.Max(c => c.BufferDuration)
                : TimeSpan.Zero;
        }
    }
}
