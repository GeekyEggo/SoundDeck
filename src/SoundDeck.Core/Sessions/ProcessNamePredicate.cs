namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Collections.Concurrent;
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
        /// Provides a cache of invalid <see cref="AppInfo.GetFromAppUserModelId(string)"/> elements.
        /// </summary>
        private static readonly ConcurrentDictionary<string, bool> INVALID_APP_USER_MODEL_IDS = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessNamePredicate"/> class.
        /// </summary>
        /// <param name="processName">Name of the process to match against.</param>
        public ProcessNamePredicate(string processName)
            => this.ProcessName = processName;

        /// <inheritdoc/>
        public string ProcessName { get; }

        /// <inheritdoc/>
        public bool Equals(ISessionPredicate x, ISessionPredicate y)
        {
            if (x is ProcessNamePredicate a
                && y is ProcessNamePredicate b)
            {
                return a.ProcessName.Equals(b.ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            return x == null && y == null;
        }

        /// <inheritdoc/>
        public int GetHashCode(ISessionPredicate obj)
            => this.ProcessName.GetHashCode();

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
                    || (!INVALID_APP_USER_MODEL_IDS.TryGetValue(session.SourceAppUserModelId, out var _) && AppInfo.GetFromAppUserModelId(session.SourceAppUserModelId).DisplayInfo.DisplayName.Contains(this.ProcessName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                INVALID_APP_USER_MODEL_IDS.TryAdd(session.SourceAppUserModelId, false);
                return false;
            }
        }
    }
}
