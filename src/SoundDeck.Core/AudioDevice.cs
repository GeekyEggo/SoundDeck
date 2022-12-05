namespace SoundDeck.Core
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
        private MMDevice _device;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="friendlyName">The friendly name associated with this instance; otherwise the <see cref="MMDevice.FriendlyName"/>.</param>
        /// <param name="isDynamic">Indicate whether this instance is dynamic, and the underlying <see cref="MMDevice"/> can change.</param>
        /// <param name="key">The key used to identify this instance.</param>
        /// <param name="role">The role of the device.</param>
        public AudioDevice(MMDevice device, string friendlyName = default, bool isDynamic = false, string key = default, Role? role = default)
        {
            this.Device = device;
            this.FriendlyName = friendlyName ?? device.FriendlyName;
            this.IsDynamic = isDynamic;
            this.Key = key ?? device.ID;
            this.Role = role;
        }

        /// <inheritdoc/>
        public event EventHandler DeviceChanged;

        /// <inheritdoc/>
        public event EventHandler<IAudioDevice, AudioVolumeNotificationData> VolumeChanged;

        /// <inheritdoc/>
        public string DeviceName { get; private set; }

        /// <inheritdoc/>
        public string FriendlyName { get; private set; }

        /// <inheritdoc/>
        public DataFlow Flow { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public bool IsDynamic { get; } = false;

        /// <inheritdoc/>
        public bool IsMuted => this.Device.AudioEndpointVolume?.Mute ?? true;

        /// <inheritdoc/>
        public string Key { get; private set; }

        /// <inheritdoc/>
        public Role? Role { get; }

        /// <inheritdoc/>
        public float Volume => this.Device.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 0;

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

                this.DeviceName = this._device.FriendlyName;
                this.Id = this._device.ID;
                this.Flow = this._device.DataFlow;

                this.DeviceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public MMDevice GetMMDevice()
            => AudioDevices.Current.Enumerator.GetDevice(this.Id);

        /// <summary>
        /// Handles the <see cref="AudioEndpointVolume.OnVolumeNotification"/> event for the underlying <see cref="MMDevice"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
            => this.VolumeChanged?.Invoke(this, data);
    }
}
