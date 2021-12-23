namespace SoundDeck.Core.Comparers
{
    /// <summary>
    /// Provides a predicate that determines a match based on the process' identifier.
    /// </summary>
    public interface IProcessPredicate
    {
        /// <summary>
        /// Determines whether the specified process identifier matches this predicate.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <returns><c>true</c> if the specified process identifier is matches this predicate; otherwise, <c>false</c>.</returns>
        bool IsMatch(uint processId);
    }
}
