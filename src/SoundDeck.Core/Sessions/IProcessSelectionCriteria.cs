﻿namespace SoundDeck.Core.Sessions
{
    using System;

    /// <summary>
    /// Provides information about selecting a process.
    /// </summary>
    public interface IProcessSelectionCriteria : IEquatable<IProcessSelectionCriteria>
    {
        /// <summary>
        /// Gets or sets the name of the process to change.
        /// </summary>
        string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the type that defines how the process is selected.
        /// </summary>
        ProcessSelectionType ProcessSelectionType { get; set; }
    }
}
