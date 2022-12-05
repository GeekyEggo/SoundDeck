namespace SoundDeck.Core.Devices
{
    using System;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioDevice"/>.
    /// </summary>
    public class AudioDevice : IAudioDevice
    {
        /// <summary>
        /// Private backing field for <see cref="Device"/>.
        /// </summary>
        private MMDevice _device = null;

        /// <summary>
        /// Private backing field for <see cref="Id"/>.
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
            : this(device.ID, device.FriendlyName, device.DataFlow) => this.Device = device;

        /// <inheritdoc/>
        public event EventHandler<IAudioDevice, AudioVolumeNotificationData> VolumeChanged;

        /// <summary>
        /// Handles the <see cref="AudioEndpointVolume.OnVolumeNotification"/> event for the underlying <see cref="MMDevice"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
            => this.VolumeChanged?.Invoke(this, data);

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
        public bool IsDynamic { get; protected set; } = false;

        /// <inheritdoc/>
        public bool IsMuted => this.Device?.AudioEndpointVolume?.Mute ?? true;

        /// <inheritdoc/>
        public virtual string Key => this.Id;

        /// <inheritdoc/>
        public Role? Role { get; protected set; }

        /// <inheritdoc/>
        public float Volume => this.Device?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 0;

        /// <summary>
        /// Gets or sets the underlying device.
        /// </summary>
        public MMDevice Device
        {
            get => this._device;
            set
            {
                if (this._device?.ID == value?.ID)
                {
                    return;
                }

                if (this._device is not null)
                {
                    this._device.AudioEndpointVolume.OnVolumeNotification -= this.AudioEndpointVolume_OnVolumeNotification;
                }

                this._device = value;
                if (this._device is not null)
                {
                    this._device.AudioEndpointVolume.OnVolumeNotification += this.AudioEndpointVolume_OnVolumeNotification;
                }
            }
        }

        /// <inheritdoc/>
        public MMDevice GetMMDevice()
            => AudioDevices.Current.Enumerator.GetDevice(this.Id);
    }
}
