namespace SoundDeck.Core
{
    using System;

    public interface IAudioCapture : IDisposable
    {   
        event EventHandler<Chunk> DataAvailable;
        IAudioFileWriter FileWriter { get; }
        void Start();
        void Stop();
    }
}
