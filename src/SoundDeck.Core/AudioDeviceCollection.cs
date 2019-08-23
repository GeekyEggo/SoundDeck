namespace SoundDeck.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;

    /// <summary>
    /// Provides a self monitoring collection of <see cref="AudioDevice"/>.
    /// </summary>
    internal sealed class AudioDeviceCollection : IAudioDeviceCollection, IMMNotificationClient
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// The <see cref="DataFlow"/> that defines the default playback device.
        /// </summary>
        private const DataFlow PLAYBACK_FLOW = DataFlow.Render;

        /// <summary>
        /// The <see cref="Role"/> that defines the default playback device.
        /// </summary>
        private const Role PLAYBACK_ROLE = Role.Multimedia;

        /// <summary>
        /// Private member filed for <see cref="DefaultPlaybackDevice"/>.
        /// </summary>
        private AudioDevice _defaultPlaybackDevice = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDeviceCollection"/> class.
        /// </summary>
        public AudioDeviceCollection()
        {
            this.Enumerator = new MMDeviceEnumerator();
            this.Enumerator.RegisterEndpointNotificationCallback(this);

            this.SetDefaultPlaybackDevice();
            foreach (var device in this.Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
            {
                this.InternalCollection.Add(device);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="DefaultPlaybackDevice"/> changed.
        /// </summary>
        public event EventHandler DefaultPlaybackDeviceChanged;

        /// <summary>
        /// Gets the <see cref="AudioDevice"/> at the specified index.
        /// </summary>
        /// <value>The <see cref="AudioDevice"/>.</value>
        /// <param name="index">The index.</param>
        /// <returns>The audio device at the specified index.</returns>
        public AudioDevice this[int index] => this.InternalCollection[index];

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => this.InternalCollection.Count;

        /// <summary>
        /// Gets the default playback device.
        /// </summary>
        public AudioDevice DefaultPlaybackDevice
        {
            get
            {
                return this._defaultPlaybackDevice;
            }

            private set
            {
                if (this._defaultPlaybackDevice != value)
                {
                    this._defaultPlaybackDevice = value;
                    this.DefaultPlaybackDeviceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the internal COM enumerator.
        /// </summary>
        private MMDeviceEnumerator Enumerator { get; set; }

        /// <summary>
        /// Gets or sets the internal collection.
        /// </summary>
        private List<AudioDevice> InternalCollection { get; set; } = new List<AudioDevice>();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Enumerator.UnregisterEndpointNotificationCallback(this);
            this.Enumerator.Dispose();
            this.Enumerator = null;
            this.InternalCollection = null;

            GC.SuppressFinalize(this);
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
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == PLAYBACK_FLOW
                && role == PLAYBACK_ROLE)
            {
                this.SetDefaultPlaybackDevice();
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
                this.InternalCollection.Add(this.Enumerator.GetDevice(pwstrDeviceId));
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
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            // do nothing
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator of <see cref="AudioDevice"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        /// <summary>
        /// Sets the default playback device.
        /// </summary>
        private void SetDefaultPlaybackDevice()
        {
            var device = this.Enumerator.GetDefaultAudioEndpoint(PLAYBACK_FLOW, PLAYBACK_ROLE);
            if (device != null)
            {
                this.DefaultPlaybackDevice = new AudioDevice(device);
            }
        }
    }
}
