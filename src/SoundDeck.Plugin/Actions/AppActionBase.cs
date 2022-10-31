namespace SoundDeck.Plugin.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SoundDeck.Core;
    using SoundDeck.Plugin.Models.Payloads;

    /// <summary>
    /// Provides a base class for actions that interact with applications.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public class AppActionBase<TSettings> : ActionBase<TSettings>
        where TSettings : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppActionBase{TSettings}"/> class.
        /// </summary>
        /// <param name="audioService">The audio service.</param>
        public AppActionBase(IAudioService audioService)
            : base(audioService)
        {
        }

        /// <summary>
        /// Sends the process options to the property inspector.
        /// </summary>
        /// <param name="eventName">Name of the event that requested the data source.</param>
        /// <param name="sessionsLabel">The sessions label.</param>
        /// <param name="sessions">The sessions.</param>
        protected async Task SendProcessOptions(string eventName, string sessionsLabel, IReadOnlyList<DataSourceItem> sessions)
        {
            // Add the default items.
            var items = new List<DataSourceItem>
            {
                new DataSourceItem("0", "Foreground (Active)"),
                new DataSourceItem("1", "By Name")
            };

            // Add the active sessions if we have any.
            if (sessions.Count > 0)
            {
                items.Add(new DataSourceItem(sessionsLabel, sessions));
            }

            // Return the items.
            var response = new DataSourceResponse(eventName, items);
            await this.SendToPropertyInspectorAsync(response);
        }
    }
}
