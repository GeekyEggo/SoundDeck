namespace SoundDeck.Core.Comparers
{
    using System;
    using System.Diagnostics;
    using SoundDeck.Core.Extensions;

    /// <summary>
    /// Provides an <see cref="IProcessPredicate"/> that loosely matches the <see cref="Process.ProcessName"/> against <see cref="ProcessName"/>.
    /// </summary>
    public class ProcessNamePredicate : IProcessPredicate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessNamePredicate"/> class.
        /// </summary>
        /// <param name="processName">Name of the process to match against.</param>
        public ProcessNamePredicate(string processName)
            => this.ProcessName = processName;

        /// <summary>
        /// Gets the name of the process to match against.
        /// </summary>
        public string ProcessName { get; }

        /// <inheritdoc/>
        public bool IsMatch(uint processId)
        {
            const string DEFAULT_PROCESS_EXTENSION = ".exe";

            try
            {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName.Equals(this.ProcessName, StringComparison.OrdinalIgnoreCase)
                    || process.ProcessName.TrimEnd(DEFAULT_PROCESS_EXTENSION, StringComparison.OrdinalIgnoreCase).Equals(this.ProcessName, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}
