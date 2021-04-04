namespace SoundDeck.Plugin.Windows
{
    using System;
    using System.Diagnostics;
    using System.Management;

    /// <summary>
    /// Provides information about the Stream Deck process.
    /// </summary>
    internal class StreamDeck
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static readonly Lazy<StreamDeck> _instance = new(() => new StreamDeck(), true);

        /// <summary>
        /// Gets the singleton instance that represents the Stream Deck.
        /// </summary>
        internal static StreamDeck Current => _instance.Value;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamDeck"/> class from being created.
        /// </summary>
        private StreamDeck()
        {
            TryGetParentProcess(out var process);
            this.Process = process;
        }

        /// <summary>
        /// Gets the main window handle for the Stream Deck process.
        /// </summary>
        public IntPtr MainWindowHandle
        {
            get
            {
                try
                {
                    return this.Process.MainWindowHandle;
                }
                catch
                {
                    return IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Gets the underlying process of the Stream Deck.
        /// </summary>
        private Process Process { get; }

        /// <summary>
        /// Tries to get the parent process of the current running process; this should result in the Stream Deck process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns><c>true</c> when the parent process was found; otherwise <c>false</c>.</returns>
        private static bool TryGetParentProcess(out Process process)
        {
            try
            {
                using (var mo = new ManagementObject($"win32_process.handle='{Process.GetCurrentProcess().Id}'"))
                {
                    mo.Get();
                    process = Process.GetProcessById(Convert.ToInt32(mo["ParentProcessId"]));
                }

                return true;
            }
            catch
            {
                process = null;
                return false;
            }
        }
    }
}
