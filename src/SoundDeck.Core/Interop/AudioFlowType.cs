namespace SoundDeck.Core.Interop
{
    /// <summary>
    /// An enumeration that defines the flow of audio.
    /// </summary>
    public enum AudioFlowType
    {
        /// <summary>
        /// A playback device, e.g. speakers.
        /// </summary>
        Playback = 0,

        /// <summary>
        /// A recording device, e.g. microphone.
        /// </summary>
        Recording = 1,
    }
}
