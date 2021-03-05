namespace SoundDeck.Core
{
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Interop;

    /// <summary>
    /// Provides information about a audio device.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal AudioDevice(MMDevice device)
            : this(device.ID, device.FriendlyName, device.DataFlow == DataFlow.Capture ? AudioFlowType.Recording : AudioFlowType.Playback, device.State == DeviceState.Active)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="flow">The flow of the auudio.</param>
        /// <param name="enabled">Whether the audio device is enabled..</param>
        internal AudioDevice(string id, string friendlyName, AudioFlowType flow, bool enabled = true)
        {
            this.Enabled = enabled;
            this.Flow = flow;
            this.FriendlyName = friendlyName;
            this.Id = id;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AudioDevice"/> is enabled.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Gets the flow of the audio.
        /// </summary>
        public AudioFlowType Flow { get; }

        /// <summary>
        /// Gets or sets the friendly name of the device.
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get; }
    }
}
