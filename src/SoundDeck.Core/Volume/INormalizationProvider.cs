namespace SoundDeck.Core.Volume
{
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides normalization of an <see cref="IAudioFileReader"/>.
    /// </summary>
    public interface INormalizationProvider
    {
        /// <summary>
        /// Applies the loudness normalization, based on the percent multiplier.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="maxGain">The maximum gain.</param>
        void ApplyLoudnessNormalization(IAudioFileReader reader, float maxGain);

        /// <summary>
        /// Normalizes the volume of the audio file reader, based on the peak.
        /// </summary>
        /// <param name="reader">The reader.</param>
        void ApplyPeakNormalization(IAudioFileReader reader);

        /// <summary>
        /// Gets the peak of the audio file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The peak as an absolute value of the byte.</returns>
        float GetPeak(IAudioFileReader reader);
    }
}
