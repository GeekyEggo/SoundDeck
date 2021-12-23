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
        public ProcessIdentifierPredicate(uint processId)
        {
            try
            {
                var process = Process.GetProcessById((int)processId);

                this.SourceProciessId = processId;
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

        /// <summary>
        /// Gets the source prociess identifier.
        /// </summary>
        public uint SourceProciessId { get; }

        /// <inheritdoc/>
        public bool IsMatch(AudioSessionControl session)
            => this.ProcessIds.Contains(session.GetProcessID);

        /// <inheritdoc/>
        public bool IsMatch(GlobalSystemMediaTransportControlsSession session)
        {
            var process = Process.GetProcessById((int)this.SourceProciessId);
            if (process == null)
            {
                return false;
            }

            return new ProcessNamePredicate(process.ProcessName)
                .IsMatch(session);
        }
    }
}
