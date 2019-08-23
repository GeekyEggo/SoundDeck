namespace SoundDeck.Plugin.Models.Settings
{
    /// <summary>
    /// The global plugin settings.
    /// </summary>
    public class PluginSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettings"/> class.
        /// </summary>
        public PluginSettings()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettings"/> class.
        /// </summary>
        /// <param name="defaultPlaybackDeviceId">The default playback device identifier.</param>
        public PluginSettings(string defaultPlaybackDeviceId)
        {
            this.DefaultPlaybackDeviceId = defaultPlaybackDeviceId;
        }

        /// <summary>
        /// Gets or sets the default playback device identifier.
        /// </summary>
        public string DefaultPlaybackDeviceId { get; set; }
    }
}
