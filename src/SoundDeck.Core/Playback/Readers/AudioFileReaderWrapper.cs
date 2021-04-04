namespace SoundDeck.Core.Playback.Readers
{
    using NAudio.Wave;

    /// <summary>
    /// Provides an implementation of <see cref="IAudioFileReader"/> for <see cref="AudioFileReader"/>.
    /// </summary>
    public class AudioFileReaderWrapper : AudioFileReader, IAudioFileReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFileReaderWrapper"/> class.
        /// </summary>
        /// <param name="fileName">The file to open</param>
        public AudioFileReaderWrapper(string fileName)
            : base(fileName)
        {
        }
    }
}
