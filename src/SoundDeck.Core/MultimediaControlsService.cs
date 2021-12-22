namespace SoundDeck.Core
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundDeck.Core.Extensions;
    using Windows.ApplicationModel;
    using Windows.Media.Control;

    /// <summary>
    /// Provides a service for controlling multimedia.
    /// </summary>
    public class MultimediaControlsService : IMultimediaControlsService
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// The session manager.
        /// </summary>
        private GlobalSystemMediaTransportControlsSessionManager _manager = null;

        /// <summary>
        /// Attempts to control a session with the specified matching <paramref name="searchCriteria"/>.
        /// </summary>
        /// <param name="searchCriteria">The search criteria of the session to match.</param>
        /// <param name="action">The multimedia action to apply.</param>
        /// <returns>The task of controlling the multimedia.</returns>
        public async Task TryControlAsync(string searchCriteria, MultimediaAction action)
        {
            var manager = await this.GetManagerAsync();
            foreach (var session in manager.GetSessions().Where(s => this.IsMatch(s, searchCriteria)))
            {
                switch (action)
                {
                    case MultimediaAction.SkipNext:
                        await session.TrySkipNextAsync();
                        break;

                    case MultimediaAction.SkipPrevious:
                        await session.TrySkipPreviousAsync();
                        break;

                    case MultimediaAction.Stop:
                        await session.TryStopAsync();
                        break;

                    case MultimediaAction.TogglePlayPause:
                        await session.TryTogglePlayPauseAsync();
                        break;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified <paramref name="session"/> matches the given <paramref name="searchCriteria"/>.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <returns><c>true</c> if the specified session is a match; otherwise, <c>false</c>.</returns>
        private bool IsMatch(GlobalSystemMediaTransportControlsSession session, string searchCriteria)
        {
            try
            {
                if (session.SourceAppUserModelId.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var appInfo = AppInfo.GetFromAppUserModelId(session.SourceAppUserModelId);
                return appInfo.PackageFamilyName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the session manager asynchronously.
        /// </summary>
        /// <returns>The session manager.</returns>
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetManagerAsync()
        {
            try
            {
                await _syncRoot.WaitAsync();

                if (this._manager == null)
                {
                    this._manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    if (this._manager == null)
                    {
                        throw new NullReferenceException("Failed to get session manager.");
                    }
                }

                return this._manager;
            }
            finally
            {
                _syncRoot.Release();
            }
        }
    }
}
