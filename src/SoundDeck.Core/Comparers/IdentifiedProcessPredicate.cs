namespace SoundDeck.Core.Comparers
{
    using System;
    using System.Diagnostics;

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
            => this.ProcessId = processId;

        /// <summary>
        /// Gets the process identifier to match against.
        /// </summary>
        public uint ProcessId { get; }

        /// <inheritdoc/>
        public bool IsMatch(uint processId)
            => processId == this.ProcessId;
    }
}
