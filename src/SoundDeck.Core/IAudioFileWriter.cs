namespace SoundDeck.Core
{
    using System.Threading.Tasks;

    public interface IAudioFileWriter
    {
        string Extension { get; }
        Task WriteAsync(string path, Chunk[] data);
    }
}
