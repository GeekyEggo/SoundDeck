namespace SoundDeck.Core.Volume
{
    /// <summary>
    /// Provides an enumeation of possible actions to apply to the volume of an audio device or application.
    /// </summary>
    public enum VolumeAction
    {
        /// <summary>
        /// Toggles mute.
        /// </summary>
        ToggleMute = 0,

        /// <summary>
        /// Mutes the volume.
        /// </summary>
        Mute = 1,

        /// <summary>
        /// Unmutes the volume.
        /// </summary>
        Unmute = 2,

        /// <summary>
        /// Sets the volume to a specific value.
        /// </summary>
        Set = 3,

        /// <summary>
        /// Increases the volume by a specific value.
        /// </summary>
        IncreaseBy = 4,

        /// <summary>
        /// Decreases the volume by a specific value.
        /// </summary>
        DecreaseBy = 5
    }
}
