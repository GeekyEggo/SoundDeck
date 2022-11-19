namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Threading;
    using SoundDeck.Core.Interop;

    /// <summary>
    /// Provides information about the foreground process.
    /// </summary>
    public class ForegroundProcess
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes the <see cref="ForegroundProcess"/> class.
        /// </summary>
        static ForegroundProcess()
        {
            Id = GetForegroundProcessId();
            Timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// Occurs when the <see cref="Id"/> changes.
        /// </summary>
        public static event EventHandler Changed;

        /// <summary>
        /// Gets the foreground process identifier.
        /// </summary>
        public static int Id { get; private set; }

        /// <summary>
        /// Gets the timer.
        /// </summary>
        private static Timer Timer { get; }

        /// <summary>
        /// Monitors the current foreground process.
        /// </summary>
        /// <param name="state">The state.</param>
        private static void OnTick(object state)
        {
            lock (_syncRoot)
            {
                var processId = GetForegroundProcessId();
                if (processId != Id)
                {
                    Id = processId;

                    try
                    {
                        Changed?.Invoke(null, EventArgs.Empty);
                    }
                    catch
                    {
                        // I hate empty catches... but we should never throw if the event handler fails.
                    }
                }
            }
        }

        /// <summary>
        /// Gets the foreground process id.
        /// </summary>
        /// <returns>the process id.</returns>
        private static int GetForegroundProcessId()
        {
            var hwnd = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(hwnd, out var processId);

            return (int)processId;
        }
    }
}
