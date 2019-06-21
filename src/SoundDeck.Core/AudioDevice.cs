namespace SoundDeck.Core
{
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Enums;

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
        {
            this.Flow = device.DataFlow == DataFlow.Capture ? AudioFlow.Recording : AudioFlow.Playback;
            this.FriendlyName = device.FriendlyName;
            this.Id = device.ID;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the friendly name of the device.
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// Gets the flow of the audio.
        /// </summary>
        public AudioFlow Flow { get; }

        /// <summary>
        /// Performs an implicit conversion from <see cref="MMDevice"/> to <see cref="AudioDevice"/>.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator AudioDevice(MMDevice device)
            => new AudioDevice(device);
    }
}
