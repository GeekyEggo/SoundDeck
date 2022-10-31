namespace SoundDeck.Plugin.Models.Payloads
{
    /// <summary>
    /// Provides a payload that represents the request for a data source.
    /// </summary>
    public class DataSourcePayload
    {
        /// <summary>
        /// Gets or sets the event of the data source.
        /// </summary>
        public string Event { get; set; } = string.Empty;
    }
}
