namespace SoundDeck.Core
{
    using NAudio.Wave;

    /// <summary>
    /// Provides normalization of an <see cref="AudioFileReader"/>.
    /// </summary>
    internal interface INormalizationProvider
    {
        /// <summary>
        /// Applies the loudness normalization, based on the percent multiplier.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="maxGain">The maximum gain.</param>
        void ApplyLoudnessNormalization(AudioFileReader reader, float maxGain);

        /// <summary>
        /// Gets the peak of the audio file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The peak as an absolute value of the byte.</returns>
        float GetPeak(AudioFileReader reader);
    }
}
