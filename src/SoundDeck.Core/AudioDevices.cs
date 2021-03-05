namespace SoundDeck.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;
    using SoundDeck.Core.Interop;

    /// <summary>
    /// Provides a singleton responsible for traversing the available <see cref="AudioDevice"/> and selecting <see cref="MMDevice"/>.
    /// </summary>
    internal sealed class AudioDevices : IReadOnlyCollection<AudioDevice>, IMMNotificationClient
    {
        /// <summary>
        /// /// The identifier used to determine the default playback device.
        /// </summary>
        private const string PLAYBACK_DEFAULT = "PLAYBACK_DEFAULT";

        /// <summary>
        /// The identifier used to determine the default playback communication device.
        /// </summary>
        private const string PLAYBACK_DEFAULT_COMMUNICATION = "PLAYBACK_DEFAULT_COMMUNICATION";

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

            // Add the default playback devices.
            this.InternalCollection.Add(new AudioDevice(PLAYBACK_DEFAULT, "Default", AudioFlowType.Playback, assignedDefault: DefaultAudioDeviceType.System));
            this.InternalCollection.Add(new AudioDevice(PLAYBACK_DEFAULT_COMMUNICATION, "Default (Communication)", AudioFlowType.Playback, assignedDefault: DefaultAudioDeviceType.Communication));

            // Add all other audio devices.
            foreach (var device in this.Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
            {
                this.InternalCollection.Add(new AudioDevice(device));
            }
        }

        /// <summary>
        /// Gets the current instance of <see cref="AudioDevices"/>.
        /// </summary>
        public static AudioDevices Current => _current.Value;

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => this.InternalCollection.Count;

        /// <summary>
        /// Gets or sets the internal COM enumerator.
        /// </summary>
        private MMDeviceEnumerator Enumerator { get; set; }

        /// <summary>
        /// Gets or sets the internal collection.
        /// </summary>
        private List<AudioDevice> InternalCollection { get; set; } = new List<AudioDevice>();

        /// <summary>
        /// Determines whether the specified device identifier is the default playback device.
        /// </summary>
        /// <param name="id">The deviceidentifier.</param>
        /// <returns><c>true</c> when the specified <paramref name="id"/> represents the default playback device.</returns>
        public bool IsDefaultPlaybackDevice(string id)
            => id == PLAYBACK_DEFAULT || string.IsNullOrEmpty(id);

        /// <summary>
        /// Gets the device with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The device identifier.</param>
        /// <returns>The device.</returns>
        public MMDevice GetDevice(string id)
        {
            if (this.IsDefaultPlaybackDevice(id))
            {
                return this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                    ?? this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }
            else if (id == PLAYBACK_DEFAULT_COMMUNICATION)
            {
                return this.Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
            }

            return this.Enumerator.GetDevice(id);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator of <see cref="AudioDevice"/>.</returns>
        public IEnumerator<AudioDevice> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return this.InternalCollection.ToList().GetEnumerator();
            }
        }

        /// <summary>
        /// Handles the default audio device change.
        /// </summary>
        /// <param name="flow">The data flow.</param>
        /// <param name="role">The role.</param>
        /// <param name="defaultDeviceId">The default device identifier.</param>
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        /// <summary>
        /// Handles an audio device being added.
        /// </summary>
        /// <param name="pwstrDeviceId">The device identifier.</param>
        public void OnDeviceAdded(string pwstrDeviceId)
        {
            lock (_syncRoot)
            {
                this.InternalCollection.Add(new AudioDevice(this.Enumerator.GetDevice(pwstrDeviceId)));
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
                this.InternalCollection.RemoveAll(a => a.Id == deviceId);
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
        /// Handles the property value changing on an audio device.
        /// </summary>
        /// <param name="pwstrDeviceId">The audio device identifier.</param>
        /// <param name="key">The property key.</param>
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator of <see cref="AudioDevice"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
