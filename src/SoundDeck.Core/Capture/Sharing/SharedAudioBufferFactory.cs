namespace SoundDeck.Core.Capture.Sharing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides a managed factory for <see cref="SharedAudioBuffer"/>.
    /// </summary>
    public class SharedAudioBufferFactory
    {
        /// <summary>
        /// The synchrinization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of class <see cref="SharedAudioBufferFactory"/>.
        /// </summary>
        /// <param name="deviceKey">The device key of the audio device.</param>
        /// <param name="bufferDuration">The initial buffer duration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public SharedAudioBufferFactory(string deviceKey, TimeSpan bufferDuration, ILoggerFactory loggerFactory)
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
        /// Gets the audio buffers currently consuming the central audio buffer.
        /// </summary>
        private List<SharedAudioBuffer> AudioBuffers { get; } = new List<SharedAudioBuffer>();

        /// <summary>
        /// Registers a new shared audio buffer with the required duration.
        /// </summary>
        /// <param name="bufferDuration">The required buffer duration.</param>
        /// <returns>The audio buffer.</returns>
        public IAudioBuffer Create(TimeSpan bufferDuration)
        {
            lock (this._syncRoot)
            {
                var sharedAudioBuffer = new SharedAudioBuffer(this.AudioBuffer, bufferDuration);
                sharedAudioBuffer.BufferDurationChanged += this.SharedAudioBuffer_BufferDurationChanged;
                sharedAudioBuffer.Disposed += this.SharedAudioBuffer_Disposed;

                this.AudioBuffers.Add(sharedAudioBuffer);
                this.SetBufferDuration();

                return sharedAudioBuffer;
            }
        }

        /// <summary>
        /// Handles <see cref="SharedAudioBuffer.Disposed"/> event, updating the <see cref="IAudioBuffer.BufferDuration"/> of the controlling audio buffer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SharedAudioBuffer_Disposed(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                if (sender is SharedAudioBuffer consumer)
                {
                    consumer.BufferDurationChanged -= this.SharedAudioBuffer_BufferDurationChanged;
                    consumer.Disposed -= this.SharedAudioBuffer_Disposed;
                    this.AudioBuffers.Remove(consumer);

                    this.SetBufferDuration();
                }
            }
        }

        /// <summary>
        /// Handles <see cref="SharedAudioBuffer.BufferDurationChanged"/> event, updating the <see cref="IAudioBuffer.BufferDuration"/> of the controlling audio buffer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SharedAudioBuffer_BufferDurationChanged(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                this.SetBufferDuration();
            }
        }

        /// <summary>
        /// Refreshes the <see cref="IAudioBuffer.BufferDuration"/> of the <see cref="AudioBuffer"/> based on the shared audio buffers requirements.
        /// </summary>
        private void SetBufferDuration()
        {
            this.AudioBuffer.BufferDuration = this.AudioBuffers.Count > 0
                ? this.AudioBuffers.Max(c => c.BufferDuration)
                : TimeSpan.Zero;
        }
    }
}
