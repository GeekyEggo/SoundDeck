namespace SoundDeck.Core.Devices
{
    using NAudio.CoreAudioApi;
    using System;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioDevice"/>.
    /// </summary>
    public class AudioDevice : IAudioDevice
    {
        /// <summary>
        /// Private member field for <see cref="Id"/>.
        /// </summary>
        private string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="flow">The audio flow.</param>
        public AudioDevice(string id, string friendlyName, DataFlow flow)
        {
            this.Id = id;
            this.FriendlyName = friendlyName;
            this.Flow = flow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="device">The audio device</param>
        public AudioDevice(MMDevice device)
            : this(device.ID, device.FriendlyName, device.DataFlow)
        {
        }

        /// <inheritdoc/>
        public event EventHandler IdChanged;

        /// <inheritdoc/>
        public string FriendlyName { get; set; }

        /// <inheritdoc/>
        public DataFlow Flow { get; }

        /// <inheritdoc/>
        public string Id
        {
            get => this._id;
            set
            {
                if (this._id != value)
                {
                    this._id = value;
                    this.IdChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly { get; protected set; } = true;

        /// <inheritdoc/>
        public virtual string Key => this.Id;

        /// <inheritdoc/>
        public Role? Role { get; protected set; }

        /// <inheritdoc/>
        public MMDevice GetMMDevice()
            => AudioDevices.Current.Enumerator.GetDevice(this.Id);
    }
}
