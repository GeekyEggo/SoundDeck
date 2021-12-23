namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Diagnostics;
    using NAudio.CoreAudioApi;
    using SoundDeck.Core.Extensions;
    using Windows.ApplicationModel;
    using Windows.Media.Control;

    /// <summary>
    /// Provides an <see cref="ISessionPredicate"/> that loosely matches the <see cref="Process.ProcessName"/> against <see cref="ProcessName"/>.
    /// </summary>
    public class ProcessNamePredicate : ISessionPredicate
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
        public bool IsMatch(AudioSessionControl session)
        {
            const string DEFAULT_PROCESS_EXTENSION = ".exe";

            try
            {
                var process = Process.GetProcessById((int)session.GetProcessID);
                return process.ProcessName.Equals(this.ProcessName, StringComparison.OrdinalIgnoreCase)
                    || process.ProcessName.TrimEnd(DEFAULT_PROCESS_EXTENSION, StringComparison.OrdinalIgnoreCase).Equals(this.ProcessName, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool IsMatch(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                return session.SourceAppUserModelId.Contains(this.ProcessName, StringComparison.OrdinalIgnoreCase)
                    || AppInfo.GetFromAppUserModelId(session.SourceAppUserModelId).DisplayInfo.DisplayName.Contains(this.ProcessName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
