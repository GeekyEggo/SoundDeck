namespace SoundDeck.Plugin.Models.Payloads
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides information about an item within a data source payload.
    /// </summary>
    /// <remarks>
    /// Read more about data sources <see href="https://sdpi-components.dev/docs/helpers/data-source#payload-structure" />.
    /// </remarks>
    public struct DataSourceItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceItem"/> struct.
        /// </summary>
        public DataSourceItem()
            => this.Value = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceItem"/> class as an item.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="label">The label.</param>
        /// <param name="disabled">The value indicating whether this instance is disabled.</param>
        public DataSourceItem(string value, string label = null, bool disabled = false)
        {
            this.Disabled = disabled;
            this.Label = label;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceItem"/> class as an item group.
        /// </summary>
        /// <param name="children">The children.</param>
        public DataSourceItem(IEnumerable<DataSourceItem> children)
            : this(null!, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceItem"/> class as an item group.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="children">The children.</param>
        public DataSourceItem(string label, IEnumerable<DataSourceItem> children)
        {
            this.Children = children;
            this.Label = label;
        }

        /// <summary>
        /// Gets the optional children associated with the item.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DataSourceItem> Children { get; } = null;

        /// <summary>
        /// Gets a value indicating whether this instance is disabled.
        /// </summary>
        public bool Disabled { get; } = default;

        /// <summary>
        /// Gets the label.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; } = null;

        /// <summary>
        /// Gets the value.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; } = null;
    }
}
