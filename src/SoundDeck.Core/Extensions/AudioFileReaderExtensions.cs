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
        /// Normalizes the volume of the audio file reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public static void NormalizeVolume(this AudioFileReader reader)
        {
            float max = 0;

            var buffer = new float[reader.WaveFormat.SampleRate];
            int read;

            // determine the max volume
            do
            {
                read = reader.Read(buffer, 0, buffer.Length);
                for (int n = 0; n < read; n++)
                {
                    var abs = Math.Abs(buffer[n]);
                    if (abs > max) max = abs;
                }
            } while (read > 0);

            // rewind the reader and set the volume
            reader.Position = 0;
            reader.Volume = 1.0f / max;
        }
    }
}
