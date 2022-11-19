namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using Windows.Media.Control;

    /// <summary>
    /// Provides an <see cref="IEquatable{T}"/> that matches the <see cref="Process.Id"/> against <see cref="ProcessId"/>.
    /// </summary>
    public class ProcessIdentifierPredicate : ISessionPredicate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIdentifierPredicate"/> class.
        /// </summary>
        /// <param name="processId">The process identifier to match against.</param>
        public ProcessIdentifierPredicate(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);

                this.SourceProciessId = processId;
                this.ProcessIds = Process.GetProcessesByName(process.ProcessName)
                    .Select(p => p.Id)
                    .ToArray();
            }
            catch
            {
                this.ProcessIds = new[] { processId };
            }
        }

        /// <inheritdoc/>
        public string ProcessName => null;

        /// <summary>
        /// Gets the process identifiers to match against.
        /// </summary>
        public int[] ProcessIds { get; }

        /// <summary>
        /// Gets the source prociess identifier.
        /// </summary>
        public int SourceProciessId { get; }

        /// <inheritdoc/>
        public bool Equals(ISessionPredicate x, ISessionPredicate y)
        {
            if (x is ProcessIdentifierPredicate a
                && y is ProcessIdentifierPredicate b)
            {
                return a.SourceProciessId == b.SourceProciessId;
            }

            return x == null && y == null;
        }

        /// <inheritdoc/>
        public int GetHashCode(ISessionPredicate obj)
            => this.SourceProciessId.GetHashCode();

        /// <inheritdoc/>
        public bool IsMatch(AudioSessionControl session)
            => this.ProcessIds.Contains((int)session.GetProcessID);

        /// <inheritdoc/>
        public bool IsMatch(GlobalSystemMediaTransportControlsSession session)
            => Process.GetProcessById(this.SourceProciessId) is Process process and not null
            && new ProcessNamePredicate(process.ProcessName).IsMatch(session);
    }
}
