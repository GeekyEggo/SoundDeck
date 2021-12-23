namespace SoundDeck.Core.Comparers
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Provides an <see cref="IEquatable{T}"/> that matches the <see cref="Process.Id"/> against <see cref="ProcessId"/>.
    /// </summary>
    public class IdentifiedProcessPredicate : IProcessPredicate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiedProcessPredicate"/> class.
        /// </summary>
        /// <param name="processId">The process identifier to match against.</param>
        public IdentifiedProcessPredicate(uint processId)
        {
            try
            {
                var process = Process.GetProcessById((int)processId);

                this.ProcessIds = Process.GetProcessesByName(process.ProcessName)
                    .Select(p => (uint)p.Id)
                    .ToArray();
            }
            catch
            {
                this.ProcessIds = new[] { processId };
            }
        }

        /// <summary>
        /// Gets the process identifiers to match against.
        /// </summary>
        public uint[] ProcessIds { get; }

        /// <inheritdoc/>
        public bool IsMatch(uint processId)
            => this.ProcessIds.Contains(processId);
    }
}
