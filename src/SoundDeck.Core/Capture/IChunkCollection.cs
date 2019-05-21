namespace SoundDeck.Core.Capture
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a means of storing chunks of data.
    /// </summary>
    public interface IChunkCollection : IDisposable
    {
        /// <summary>
        /// Adds the chunk of data asynchronously.
        /// </summary>
        /// <param name="chunk">The chunk of data.</param>
        /// <returns>The task of adding the chunk.</returns>
        Task AddAsync(Chunk chunk);

        /// <summary>
        /// Gets the chunks of data from with the last <paramref name="duration"/> time span asynchronously.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>The chunks of data.</returns>
        Task<Chunk[]> GetAsync(TimeSpan duration);
    }
}
