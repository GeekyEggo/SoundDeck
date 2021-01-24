namespace SoundDeck.Plugin.Models.Settings
{
    /// <summary>
    /// Defines an enumeration of ways a process can be selected.
    /// </summary>
    public enum ProcessSelectionType
    {
        /// <summary>
        /// The foreground process is selected.
        /// </summary>
        Foreground = 0,

        /// <summary>
        /// Matches the process by the name.
        /// </summary>
        ByName = 1
    }
}
