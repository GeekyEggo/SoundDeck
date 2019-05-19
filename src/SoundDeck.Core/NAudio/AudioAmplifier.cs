using NAudio.Wave;
using System;

namespace SoundDeck.Core.NAudio
{
    public static class AudioAmplifier
    {
        public static void Normalize(string inputPath, string outputPath)
        {
            float max = 0;

            using (var reader = new AudioFileReader(inputPath))
            {
                // find the max peak
                float[] buffer = new float[reader.WaveFormat.SampleRate];
                int read;
                do
                {
                    read = reader.Read(buffer, 0, buffer.Length);
                    for (int n = 0; n < read; n++)
                    {
                        var abs = Math.Abs(buffer[n]);
                        if (abs > max) max = abs;
                    }
                } while (read > 0);
                Console.WriteLine($"Max sample value: {max}");

                if (max == 0 || max > 1.0f)
                    throw new InvalidOperationException("File cannot be normalized");

                // rewind and amplify
                reader.Position = 0;
                reader.Volume = 1.0f / max;

                // write out to a new WAV file
                global::NAudio.Wave.WaveFileWriter.CreateWaveFile(outputPath, reader);
            }
        }
    }
}
