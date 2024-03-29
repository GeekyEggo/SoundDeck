namespace SoundDeck.Core.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides interoperability for the "user32.dll".
    /// </summary>
    public static class User32
    {
        /// <summary>
        /// Destroys an icon and frees any memory the icon occupied.
        /// </summary>
        /// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use..</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero</returns>
        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="ProcessId">The process identifier.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
