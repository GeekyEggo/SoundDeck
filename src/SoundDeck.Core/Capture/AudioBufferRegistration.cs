namespace SoundDeck.Core.Capture
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Provides information about an object listening to an audio buffer.
    /// </summary>
    public class AudioBufferRegistration : INotifyPropertyChanged
    {
        /// <summary>
        /// Private member field for <see cref="ClipDuration"/>.
        /// </summary>
        private TimeSpan _clipDuration = TimeSpan.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferRegistration"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="clipDuration">Duration of the clip.</param>
        internal AudioBufferRegistration(Guid id, TimeSpan clipDuration)
        {
            this.Id = id;
            this.ClipDuration = clipDuration;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the audio buffer.
        /// </summary>
        public IAudioBuffer AudioBuffer { get; internal set; }

        /// <summary>
        /// Gets or sets the duration of a clip.
        /// </summary>
        public TimeSpan ClipDuration
        {
            get
            {
                return this._clipDuration;
            }

            set
            {
                if (this._clipDuration != value)
                {
                    this._clipDuration = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioBufferRegistration.ClipDuration)));
                }
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        internal Guid Id { get; }
    }
}
