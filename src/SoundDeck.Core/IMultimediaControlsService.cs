namespace SoundDeck.Core
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a service for controlling multimedia.
    /// </summary>
    public interface IMultimediaControlsService
    {
        /// <summary>
        /// Attempts to control a session with the specified matching <paramref name="searchCriteria"/>.
        /// </summary>
        /// <param name="searchCriteria">The search criteria of the session to match.</param>
        /// <param name="action">The multimedia action to apply.</param>
        /// <returns>The task of controlling the multimedia.</returns>
        Task TryControlAsync(string searchCriteria, MultimediaAction action);
    }
}
