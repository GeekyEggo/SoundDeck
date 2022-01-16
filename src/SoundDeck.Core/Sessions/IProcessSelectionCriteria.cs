namespace SoundDeck.Core.Sessions
{
    /// <summary>
    /// Provides information about selecting a process.
    /// </summary>
    public interface IProcessSelectionCriteria
    {
        /// <summary>
        /// Gets the name of the process to change.
        /// </summary>
        string ProcessName { get; }

        /// <summary>
        /// Gets the type that defines how the process is selected.
        /// </summary>
        ProcessSelectionType ProcessSelectionType { get; }
    }
}
