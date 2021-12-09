namespace SoundDeck.Core.Capture.Sharing
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a wrapper for a shared <see cref="IAudioBuffer"/>.
    /// </summary>
    public sealed class SharedAudioBufferConsumer : IAudioBuffer
    {
        /// <summary>
        /// Private field for <see cref="BufferDuration"/>.
        /// </summary>
        private TimeSpan _bufferDuration;

        /// <summary>
        /// Initializes a new instance of class <see cref="SharedAudioBufferConsumer"/>
        /// </summary>
        /// <param name="audioBuffer">The audio buffer.</param>
        /// <param name="bufferDuration">The buffer duration.</param>
        public SharedAudioBufferConsumer(IAudioBuffer audioBuffer, TimeSpan bufferDuration)
        {
            this.AudioBuffer = audioBuffer;
            this.BufferDuration = bufferDuration;
        }

        /// <summary>
        /// Occurs when the buffer duration has changed.
        /// </summary>
        public event EventHandler BufferDurationChanged;

        /// <summary>
        /// Occurs when this instance is disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Gets the audio buffer.
        /// </summary>
        public IAudioBuffer AudioBuffer { get; }

        /// <summary>
        /// Gets or sets the duration of the buffer.
        /// </summary>
        public TimeSpan BufferDuration
        {
            get => this._bufferDuration;
            set
            {
                if (this._bufferDuration != value)
                {
                    this._bufferDuration = value;
                    this.BufferDurationChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the audio device.
        /// </summary>
        public IAudioDevice Device => this.AudioBuffer.Device;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // as the audio buffer is shared, we dont dispose of it, but instead rely on the manager to dispose if it is no longer in use
            if (!this.IsDisposed)
            {
                this.Disposed?.Invoke(this, EventArgs.Empty);
                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// Saves an audio file of the current buffer.
        /// </summary>
        /// <param name="settings">The settings containing information about how, and where to save the capture.</param>
        /// <returns>The file path.</returns>
        public Task<string> SaveAsync(ISaveBufferSettings settings)
            => this.AudioBuffer.SaveAsync(settings);
    }
}
