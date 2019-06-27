namespace SoundDeck.Core.Extensions
{
    using NAudio.Wave;
    using System;

    /// <summary>
    /// Provides extension methods for <see cref="AudioFileReader"/>.
    /// </summary>
    public static class AudioFileReaderExtensions
    {
        /// <summary>
        /// Normalizes the volume of the audio file reader, based on the peak.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public static void ApplyPeakNormalization(this AudioFileReader reader)
        {
            var peak = reader.GetPeak();
            reader.Volume = 1.0f / peak;
        }

        /// <summary>
        /// Gets the peak byte from the audio file reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The peak byte as an absolute value.</returns>
        public static float GetPeak(this AudioFileReader reader)
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
            reader.Position = 0;
            return peak;
        }
    }
}
