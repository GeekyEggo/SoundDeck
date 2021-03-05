namespace SoundDeck.Core
{
    using System;

    /// <summary>
    /// Defines an enumeration of possible defaults an audio device can be assigned to.
    /// </summary>
    [Flags]
    public enum DefaultAudioDeviceType
    {
        /// <summary>
        /// The device is not a default.
        /// </summary>
        None = 0,

        /// <summary>
        /// The device is the default system audio device.
        /// </summary>
        System = 1,

        /// <summary>
        /// The device is the default communication audio device.
        /// </summary>
        Communication = 2
    }
}
