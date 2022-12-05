namespace SoundDeck.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;
    using SoundDeck.Core.Devices;

    /// <summary>
    /// Provides a singleton responsible for traversing the available <see cref="AudioDevice"/> and selecting <see cref="MMDevice"/>.
    /// </summary>
    public sealed class AudioDevices : IMMNotificationClient, IReadOnlyCollection<IAudioDevice>
    {
        /// <summary>
        /// /// The identifier used to determine the default playback device.
        /// </summary>
        public const string PLAYBACK_DEFAULT = "PLAYBACK_DEFAULT";

        /// <summary>
        /// The identifier used to determine the default playback communication device.
        /// </summary>
        public const string PLAYBACK_DEFAULT_COMMUNICATION = "PLAYBACK_DEFAULT_COMMUNICATION";

        /// <summary>
        /// The identifier used to determine the default recording device.
        /// </summary>
        public const string RECORDING_DEFAULT = "RECORDING_DEFAULT";

        /// <summary>
        /// The identifier used to determine the default recording communication device.
        /// </summary>
        public const string RECORDING_DEFAULT_COMMUNICATION = "RECORDING_DEFAULT_COMMUNICATION";

        /// <summary>
        /// Private member field for <see cref="Current"/>.
        /// </summary>
        private static readonly Lazy<AudioDevices> _current = new Lazy<AudioDevices>(() => new AudioDevices(), true);

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevices"/> class.
        /// </summary>
        private AudioDevices()
        {
            this.Enumerator = new MMDeviceEnumerator();
            this.Enumerator.RegisterEndpointNotificationCallback(this);

            // Add all devices.
            foreach (var device in this.Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
            {
                this.Devices.Add(new AudioDevice(device));
            }

            // Add the default playback and recording devices.
            AddDefault(PLAYBACK_DEFAULT, "Default", DataFlow.Render, Role.Console);
            AddDefault(PLAYBACK_DEFAULT_COMMUNICATION, "Default (Communication)", DataFlow.Render, Role.Communications);
            AddDefault(RECORDING_DEFAULT, "Default", DataFlow.Capture, Role.Console);
            AddDefault(RECORDING_DEFAULT_COMMUNICATION, "Default (Communication)", DataFlow.Capture, Role.Communications);

            void AddDefault(string key, string friendlyName, DataFlow dataFlow, Role role)
            {
                using (var defaultEndpoint = this.Enumerator.GetDefaultAudioEndpoint(dataFlow, role))
                {
                    var sharedMMDevice = this.Devices.FirstOrDefault(item => item.Device.ID == defaultEndpoint.ID);
                    var defaultAudioDevice = new DefaultAudioDevice(key, friendlyName, role, sharedMMDevice.Device);

                    this.Devices.Add(defaultAudioDevice);
                    this.Enumerator.RegisterEndpointNotificationCallback(defaultAudioDevice);
                }
            }
        }

        /// <summary>
        /// Occurs when the default device changed.
        /// </summary>
        public event EventHandler DefaultDeviceChanged;

        /// <summary>
        /// Occurs when the devices changed.
        /// </summary>
        public event EventHandler DevicesChanged;

        /// <summary>
        /// Gets the current instance of <see cref="AudioDevices"/>.
        /// </summary>
        public static AudioDevices Current => _current.Value;

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => this.Devices.Count;

        /// <summary>
        /// Gets or sets the internal COM enumerator.
        /// </summary>
        internal MMDeviceEnumerator Enumerator { get; set; }

        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        private List<AudioDevice> Devices { get; } = new List<AudioDevice>();

        /// <summary>
        /// Gets the <see cref="MMDevice"/> by its unique key identifier.
        /// </summary>
        /// <param name="deviceId">The key.</param>
        /// <returns>The audio device.</returns>
        public IAudioDevice GetDeviceByKey(string key)
        {
            lock (_syncRoot)
            {
                if (string.IsNullOrEmpty(key))
                {
                    key = PLAYBACK_DEFAULT;
                }

                return this.Devices.FirstOrDefault(d => d.Key == key);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator of <see cref="AudioDevice"/>.</returns>
        public IEnumerator<IAudioDevice> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return this.Devices.ToList().GetEnumerator();
            }
        }

        /// <summary>
        /// Called when a default device changes.
        /// </summary>
        /// <param name="flow">The flow.</param>
        /// <param name="role">The role.</param>
        /// <param name="defaultDeviceId">The default device identifier.</param>
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            => this.DefaultDeviceChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Handles an audio device being added.
        /// </summary>
        /// <param name="pwstrDeviceId">The device identifier.</param>
        public void OnDeviceAdded(string pwstrDeviceId)
        {
            lock (_syncRoot)
            {
                this.Devices.Add(new AudioDevice(this.Enumerator.GetDevice(pwstrDeviceId)));
                this.DevicesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles an audio device being removed.
        /// </summary>
        /// <param name="deviceId">The audio device identifier.</param>
        public void OnDeviceRemoved(string deviceId)
        {
            lock (_syncRoot)
            {
                this.Devices.RemoveAll(device => device.Id == deviceId && !device.IsDynamic);
                this.DevicesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the state of an audio device changing.
        /// </summary>
        /// <param name="deviceId">The audio device identifier.</param>
        /// <param name="newState">The new state.</param>
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            switch (newState)
            {
                case DeviceState.Active:
                    this.OnDeviceAdded(deviceId);
                    break;

                default:
                    this.OnDeviceRemoved(deviceId);
                    break;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        #region IMMNotificationClient
        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        #endregion
    }
}
