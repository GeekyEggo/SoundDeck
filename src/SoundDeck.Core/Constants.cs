using NAudio.Wave;

namespace SoundDeck.Core
{
    /// <summary>
    /// Global application constants.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Gets the desired bit rate.
        /// </summary>
        public static int DesiredBitRate => 192000;

        /// <summary>
        /// Gets the default wave format.
        /// </summary>
        public static WaveFormat DefaultWaveFormat { get; } = new WaveFormat();
    }
}
