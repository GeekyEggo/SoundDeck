namespace SoundDeck.Plugin.Models.Payloads
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A payload that provides a data source to the property inspector.
    /// </summary>
    public class DataSourceResponse : DataSourcePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceResponse"/> class.
        /// </summary>
        /// <param name="event">The event that the payload is associated with.</param>
        /// <param name="items">The items.</param>
        public DataSourceResponse(string @event, params DataSourceItem[] items)
            : this(@event, items.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceResponse"/> class.
        /// </summary>
        /// <param name="event">The event that the payload is associated with.</param>
        /// <param name="items">The items.</param>
        public DataSourceResponse(string @event, IEnumerable<DataSourceItem> items)
        {
            this.Event = @event;
            this.Items = items.ToList();
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<DataSourceItem> Items { get; }
    }
}
