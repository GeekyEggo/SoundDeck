namespace SoundDeck.Core.Devices
{
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioDevice"/> that represents a default device.
    /// </summary>
    public class DefaultAudioDevice : AudioDevice, IAudioDevice, IMMNotificationClient
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="key">The unique key that represents the audio device.</param>
        /// <param name="friendlyName">The friendly name of the device.</param>
        /// <param name="role">The role of the default device.</param>
        /// <param name="device">The audio device</param>
        public DefaultAudioDevice(string key, string friendlyName, Role role, MMDevice device)
            : base(device)
        {
            this.IsDynamic = true;
            this.FriendlyName = friendlyName;
            this.Key = key;
            this.Role = role;
        }

        /// <inheritdoc/>
        public override string Key { get; }

        /// <summary>
        /// Called when the default device changes.
        /// </summary>
        /// <param name="flow">The audio flow.</param>
        /// <param name="role">The role of the default device.</param>
        /// <param name="defaultDeviceId">The default device identifier.</param>
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            lock (_syncRoot)
            {
                if (this.Flow == flow
                    && this.Role == role)
                {
                    this.Id = defaultDeviceId;
                }
            }
        }

        #region IMMNotificationClient
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        #endregion
    }
}
