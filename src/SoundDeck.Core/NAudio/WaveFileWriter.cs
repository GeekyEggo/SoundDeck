using NAudio.Wave;
using System.Threading.Tasks;

namespace SoundDeck.Core.NAudio
{
    public class WaveFileWriter : IAudioFileWriter
    {
        public string Extension { get; } = ".wav";
        public WaveFormat WaveFormat { get; set; }

        public async Task WriteAsync(string path, Chunk[] data)
        {
            using (var writer = new global::NAudio.Wave.WaveFileWriter(path, this.WaveFormat))
            {
                foreach (var chunk in data)
                {
                    await writer.WriteAsync(chunk.Buffer, 0, chunk.BytesRecorded);
                }
            }
        }
    }
}
