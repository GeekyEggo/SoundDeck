namespace SoundDeck.Core.Playback.Readers
{
    using System;
    using System.IO;
    using NAudio.Wave;

    /// <summary>
    /// Provides an interface required to fulfil functionality provided by Sound Deck; this allows for NAudio implementations to be wrapped to ensure a standard structure.
    /// </summary>
    public interface IAudioFileReader : IWaveProvider, IDisposable
    {
        /// <summary>
        /// Gets the current time of the audio.
        /// </summary>
        TimeSpan CurrentTime { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the Length of this stream (in bytes)
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets the total time of the audio.
        /// </summary>
        TimeSpan TotalTime { get; }

        /// <summary>
        /// Gets or sets the volume of the audio .
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        int Read(float[] buffer, int offset, int count);

        /// <summary>
        /// Seeks to the specified <paramref name="offset"/>, based on the <paramref name="origin"/>.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="origin">The origin.</param>
        /// <returns>The new position.</returns>
        long Seek(long offset, SeekOrigin origin);
    }
}
