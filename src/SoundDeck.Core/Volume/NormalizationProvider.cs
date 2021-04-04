namespace SoundDeck.Core.Volume
{
    using System;
    using System.IO;
    using SoundDeck.Core.Playback.Readers;

    /// <summary>
    /// Provides normalization of a <see cref="IAudioFileReader"/>.
    /// </summary>
    public class NormalizationProvider : INormalizationProvider
    {
        /// <summary>
        /// Applies the loudness normalization, based on the percent multiplier.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="maxGain">The maximum gain.</param>
        public void ApplyLoudnessNormalization(IAudioFileReader reader, float maxGain)
        {
            var peak = this.GetPeak(reader);
            if (peak >= maxGain)
            {
                reader.Volume = (1.0f / peak) * maxGain;
            }
        }

        /// <summary>
        /// Normalizes the volume of the audio file reader, based on the peak.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void ApplyPeakNormalization(IAudioFileReader reader)
        {
            var peak = this.GetPeak(reader);
            reader.Volume = 1.0f / peak;
        }

        /// <summary>
        /// Gets the peak byte from the audio file reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The peak byte as an absolute value.</returns>
        public virtual float GetPeak(IAudioFileReader reader)
        {
            float peak = 0;

            var buffer = new float[reader.WaveFormat.SampleRate];
            int read;

            // determine the peak
            do
            {
                read = reader.Read(buffer, 0, buffer.Length);
                for (int n = 0; n < read; n++)
                {
                    var abs = Math.Abs(buffer[n]);
                    if (abs > peak) peak = abs;
                }
            } while (read > 0);

            // rewind the reader and set the volume
            reader.Seek(0, SeekOrigin.Begin);
            return peak;
        }
    }
}
