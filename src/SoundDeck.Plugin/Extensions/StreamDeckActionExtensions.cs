namespace SoundDeck.Plugin.Extensions
{
    using System;
    using System.Threading.Tasks;
    using SharpDeck;

    /// <summary>
    /// Provides extension methods for <see cref="StreamDeckAction{TSettings}"/>.
    /// </summary>
    public static class StreamDeckActionExtensions
    {
        /// <summary>
        /// Gets the latest settings for this instance, updates them, and then persists them.
        /// </summary>
        /// <typeparam name="TSettings">The type of the settings.</typeparam>
        /// <param name="action">The action whos settings to update.</param>
        /// <param name="update">The update delegate used to apply changes to the settings.</param>
        /// <returns>The task of updating the settings.</returns>
        public static async Task UpdateSettingsAsync<TSettings>(this StreamDeckAction<TSettings> action, Action<TSettings> update)
            where TSettings : class
        {
            var settings = await action.GetSettingsAsync<TSettings>();

            update(settings);
            await action.SetSettingsAsync(settings);
        }
    }
}
