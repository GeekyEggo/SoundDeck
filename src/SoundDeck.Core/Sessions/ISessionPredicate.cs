namespace SoundDeck.Core.Sessions
{
    using System.Collections.Generic;
    using NAudio.CoreAudioApi;
    using Windows.Media.Control;

    /// <summary>
    /// Provides a predicate that determines whether a session matches this instance.
    /// </summary>
    public interface ISessionPredicate : IEqualityComparer<ISessionPredicate>
    {
        /// <summary>
        /// Determines whether the specified <paramref name="session"/> fulfils this predicate.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns><c>true</c> when the <paramref name="session"/> fulfils this predicate; otherwise <c>false</c>.</returns>
        bool IsMatch(AudioSessionControl session);

        /// <summary>
        /// Determines whether the specified <paramref name="session"/> fulfils this predicate.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns><c>true</c> when the <paramref name="session"/> fulfils this predicate; otherwise <c>false</c>.</returns>
        bool IsMatch(GlobalSystemMediaTransportControlsSession session);
    }
}
