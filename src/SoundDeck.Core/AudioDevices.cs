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
    public sealed class AudioDevices : IReadOnlyCollection<IAudioDevice>, IMMNotificationClient
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

            void AddDefault(DefaultAudioDevice device)
            {
                this.Devices.Add(device);
                this.Enumerator.RegisterEndpointNotificationCallback(device);
            }

            // Add the default playback and recording devices.
            AddDefault(new DefaultAudioDevice(PLAYBACK_DEFAULT, "Default", Role.Console, this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)));
            AddDefault(new DefaultAudioDevice(PLAYBACK_DEFAULT_COMMUNICATION, "Default (Communication)", Role.Communications, this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications)));
            AddDefault(new DefaultAudioDevice(RECORDING_DEFAULT, "Default", Role.Console, this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console)));
            AddDefault(new DefaultAudioDevice(RECORDING_DEFAULT_COMMUNICATION, "Default (Communication)", Role.Communications, this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications)));

            // Add all other audio devices.
            foreach (var device in this.Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
            {
                this.Devices.Add(new AudioDevice(device));
            }
        }

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
        private List<IAudioDevice> Devices { get; } = new List<IAudioDevice>();

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
        /// Handles an audio device being added.
        /// </summary>
        /// <param name="pwstrDeviceId">The device identifier.</param>
        public void OnDeviceAdded(string pwstrDeviceId)
        {
            lock (_syncRoot)
            {
                this.Devices.Add(new AudioDevice(this.Enumerator.GetDevice(pwstrDeviceId)));
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

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator of <see cref="AudioDevice"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        #region IMMNotificationClient
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        #endregion
    }
}
