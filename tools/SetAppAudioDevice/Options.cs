namespace SetAppAudioDevice
{
    /// <summary>
    /// Provides options parsed from the command line arguments.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public Options(string[] args)
        {
            for (var i = 0; i < args.Length; i += 2)
            {
                // Ensure we have both the key, and value.
                if (i + 1 >= args.Length)
                {
                    break;
                }

                switch (args[i].ToLowerInvariant())
                {
                    case "-d":
                    case "/d":
                        this.Device = args[i + 1];
                        break;

                    case "-p":
                    case "/p":
                        this.Process = args[i + 1];
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the device; when empty or null, the default playback device is used.
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// Gets or sets the process; when empty or null, the foreground process is used.
        /// </summary>
        public string Process { get; set; }
    }
}