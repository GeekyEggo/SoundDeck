namespace SoundDeck.Core.Extensions
{
    using System;
    using SoundDeck.Core.Interop;
    using SoundDeck.Core.Sessions;

    /// <summary>
    /// Provides extension methods for <see cref="IProcessSelectionCriteria"/>.
    /// </summary>
    public static class ProcessSelectionCriteriaExtensions
    {
        /// <summary>
        /// Converts this instance to a <see cref="ISessionPredicate"/>.
        /// </summary>
        /// <param name="criteria">The criteria; this instance.</param>
        /// <returns>The <see cref="ISessionPredicate"/>.</returns>
        /// <exception cref="ArgumentNullException">Cannot select process as process name has not been specified.</exception>
        public static ISessionPredicate ToPredicate(this IProcessSelectionCriteria criteria)
        {
            // Select the process by name.
            if (criteria.ProcessSelectionType == ProcessSelectionType.ByName)
            {
                if (string.IsNullOrWhiteSpace(criteria.ProcessName))
                {
                    throw new ArgumentNullException("Cannot select process as process name has not been specified");
                }

                return new ProcessNamePredicate(criteria.ProcessName);
            }

            // Select the foreground process predicate.
            var hwnd = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(hwnd, out var processId);

            return new ProcessIdentifierPredicate(processId);
        }
    }
}
