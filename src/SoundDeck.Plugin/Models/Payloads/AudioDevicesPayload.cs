namespace SoundDeck.Plugin.Models.Payloads
{
    using SharpDeck.PropertyInspectors;
    using SoundDeck.Core;

    /// <summary>
    /// Provides information about audio devices, as a payload.
    /// </summary>
    public class AudioDevicesPayload : PropertyInspectorPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevicesPayload"/> class.
        /// </summary>
        /// <param name="devices">The devices.</param>
        public AudioDevicesPayload(AudioDevice[] devices)
        {
            this.Devices = devices;
        }

        /// <summary>
        /// Gets or sets the audio devices.
        /// </summary>
        public AudioDevice[] Devices { get; set; }
    }
}
