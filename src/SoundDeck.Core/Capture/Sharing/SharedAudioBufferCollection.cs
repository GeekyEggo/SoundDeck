namespace SoundDeck.Core.Capture.Sharing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// A collection of shared audio buffers.
    /// </summary>
    public class SharedAudioBufferCollection : INotifyCollectionChanged
    {
        /// <summary>
        /// Provides synchronized access.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedAudioBufferCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent audio buffer.</param>
        public SharedAudioBufferCollection(IAudioBuffer parent)
            : base()
        {
            this.Parent = parent;
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets the parent audio buffer.
        /// </summary>
        public IAudioBuffer Parent { get; }

        /// <summary>
        /// Gets the shared audio buffers.
        /// </summary>
        private List<SharedAudioBuffer> SharedItems { get; } = new List<SharedAudioBuffer>();

        /// <summary>
        /// Adds an audio buffer to the collection.
        /// </summary>
        /// <param name="bufferDuration">Duration of the new audio buffer.</param>
        /// <returns>The audio buffer</returns>
        public IAudioBuffer Add(TimeSpan bufferDuration)
        {
            // create the buffer
            var item = new SharedAudioBuffer
            {
                AudioBuffer = this.Parent,
                BufferDuration = bufferDuration
            };

            // monitor changes
            item.BufferDurationChanged += this.Item_BufferDurationChanged;
            item.Disposed += this.Item_Disposed;

            lock (this._syncRoot)
            {
                // add the buffer
                this.SharedItems.Add(item);
                if (item.BufferDuration > this.Parent.BufferDuration)
                {
                    this.Parent.BufferDuration = item.BufferDuration;
                }
            }

            return item;
        }

        /// <summary>
        /// Handles the <see cref="SharedAudioBuffer.Dispose"/> event of a <see cref="SharedAudioBuffer"/> within <see cref="SharedItems"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Item_Disposed(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                // remove the buffer, and either update the parent duration, or trigger a reset event change
                this.SharedItems.Remove((SharedAudioBuffer)sender);
                if (this.SharedItems.Count > 0)
                {
                    this.RefreshBufferDuration();
                }
                else
                {
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="SharedAudioBuffer.BufferDurationChanged"/> event of a <see cref="SharedAudioBuffer"/> within <see cref="SharedItems"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Item_BufferDurationChanged(object sender, EventArgs e)
        {
            lock (this._syncRoot)
            {
                // increase the parent buffer when the shared is higher, otherwise refresh it
                var shared = (SharedAudioBuffer)sender;
                if (shared.BufferDuration > this.Parent.BufferDuration)
                {
                    this.Parent.BufferDuration = shared.BufferDuration;
                }
                else
                {
                    this.RefreshBufferDuration();
                }
            }
        }

        /// <summary>
        /// Refreshes the duration of <see cref="AudioBuffer.BufferDuration"/> of the <see cref="Parent"/> based on the shared instances.
        /// </summary>
        private void RefreshBufferDuration()
        {
            if (this.SharedItems.Count > 0)
            {
                this.Parent.BufferDuration = this.SharedItems.Max(x => x.BufferDuration);
            }
        }
    }
}
