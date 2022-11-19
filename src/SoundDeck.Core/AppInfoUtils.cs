namespace SoundDeck.Core
{
    using System.Collections.Concurrent;
    using Windows.ApplicationModel;

    /// <summary>
    /// Provides utilities for <see cref="AppInfo"/>.
    /// </summary>
    public static class AppInfoUtils
    {
        /// <summary>
        /// Provides a cache of invalid <see cref="AppInfo.GetFromAppUserModelId(string)"/> elements; this prevents exceptions from being thrown multiple times.
        /// </summary>
        private static readonly ConcurrentDictionary<string, bool> INVALID_APP_USER_MODEL_IDS = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Attempts to get the <see cref="AppInfo"/> from the specified <paramref name="appUserModelId"/>.
        /// </summary>
        /// <param name="appUserModelId">The application user model identifier.</param>
        /// <param name="appInfo">The application information.</param>
        /// <returns><c>true</c> when the <see cref="AppInfo"/> was found; otherwise <c>false</c>.</returns>
        public static bool TryGet(string appUserModelId, out AppInfo appInfo)
        {
            appInfo = default;
            if (INVALID_APP_USER_MODEL_IDS.TryGetValue(appUserModelId, out var _))
            {
                return false;
            }

            try
            {
                appInfo = AppInfo.GetFromAppUserModelId(appUserModelId);
                return true;
            }
            catch
            {
                INVALID_APP_USER_MODEL_IDS.TryAdd(appUserModelId, true);
            }

            return false;
        }
    }
}
