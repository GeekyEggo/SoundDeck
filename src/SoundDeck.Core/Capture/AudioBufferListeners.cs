namespace SoundDeck.Core.Capture
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// A collection of listeners for a specific <see cref="IAudioBuffer" />.
    /// </summary>
    public class AudioBufferListeners
    {
        /// <summary>
        /// Provides synchronized acces.
        /// </summary>
        private object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferListeners"/> class.
        /// </summary>
        /// <param name="audioBuffer">The audio buffer.</param>
        public AudioBufferListeners(IConfigurableAudioBuffer audioBuffer)
        {
            this.AudioBuffer = audioBuffer;
        }

        /// <summary>
        /// Gets the audio buffer.
        /// </summary>
        public IConfigurableAudioBuffer AudioBuffer { get; }

        /// <summary>
        /// Gets the number of listeners count.
        /// </summary>
        public int Count => this.Listeners.Count;

        /// <summary>
        /// Gets the listeners associated with <see cref="AudioBuffer"/>.
        /// </summary>
        private IDictionary<Guid, AudioBufferRegistration> Listeners { get; } = new Dictionary<Guid, AudioBufferRegistration>();
        
        /// <summary>
        /// Adds the specified registration as a listener.
        /// </summary>
        /// <param name="item">The listener information.</param>
        public void Add(AudioBufferRegistration item)
        {
            item.AudioBuffer = this.AudioBuffer;
            item.PropertyChanged += this.Listener_PropertyChanged;

            lock (this._syncRoot)
            {
                // add the item and update the buffer if the items duration is longer
                this.Listeners.Add(item.Id, item);
                if (item.ClipDuration > this.AudioBuffer.BufferDuration)
                {
                    this.AudioBuffer.BufferDuration = item.ClipDuration;
                }
            }
        }

        /// <summary>
        /// Removes the listener matching the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if a listener was removed; otherwise <c>false</c>.</returns>
        public bool Remove(Guid id)
        {
            lock (this._syncRoot)
            {
                if (this.Listeners.TryGetValue(id, out var item))
                {
                    item.PropertyChanged -= this.Listener_PropertyChanged;
                    this.Listeners.Remove(id);
                    this.RefreshBufferDuration();

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Handles the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of items within <see cref="Listeners"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Listener_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AudioBufferRegistration.ClipDuration))
            {
                this.RefreshBufferDuration();
            }
        }

        /// <summary>
        /// Refreshes the duration of <see cref="AudioBuffer.BufferDuration"/> based on the listeners.
        /// </summary>
        private void RefreshBufferDuration()
        {
            lock (this._syncRoot)
            {
                if (this.Listeners.Count > 0)
                {
                    this.AudioBuffer.BufferDuration = this.Listeners.Values.Max(x => x.ClipDuration);
                }
            }
        }
    }
}
