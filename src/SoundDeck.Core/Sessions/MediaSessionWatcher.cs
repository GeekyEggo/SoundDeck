namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Nito.AsyncEx;
    using SoundDeck.Core.Extensions;
    using Windows.Media.Control;

    /// <summary>
    /// Provides a session watcher for <see cref="GlobalSystemMediaTransportControlsSession"/>.
    /// </summary>
    public sealed class MediaSessionWatcher : SessionWatcher<GlobalSystemMediaTransportControlsSession>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Private backing field for <see cref="Timeline"/>.
        /// </summary>
        private MediaSessionTimelineTicker _timeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSessionWatcher"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="GlobalSystemMediaTransportControlsSessionManager"/>.</param>
        /// <param name="selectionCriteria">The <see cref="IProcessSelectionCriteria"/>.</param>
        public MediaSessionWatcher(GlobalSystemMediaTransportControlsSessionManager manager, IProcessSelectionCriteria selectionCriteria)
            : base(selectionCriteria)
        {
            this.Manager = manager;
            this.Manager.SessionsChanged += this.OnMediaSessionsChanged;

            this.RefreshSession();
        }

        /// <summary>
        /// Occurs when <see cref="MediaSessionTimelineTicker.TimelineChanged"/> occurs; this is not raised when <see cref="Timeline"/> changes.
        /// </summary>
        public event EventHandler<TimelineEventArgs> TimelineChanged;

        /// <summary>
        /// Occurs when <see cref="Thumbnail"/> or <see cref="Title"/> changes.
        /// </summary>
        public event EventHandler MediaPropertiesChanged;

        /// <summary>
        /// Gets the thumbnail associated with the media, in base64 format.
        /// </summary>
        public string Thumbnail { get; private set; }

        /// <summary>
        /// Gets the title associated with the media.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the active media session manager.
        /// </summary>
        private GlobalSystemMediaTransportControlsSessionManager Manager { get; }

        /// <summary>
        /// Gets or sets the <see cref="MediaSessionTimelineTicker"/> associated with the <see cref="GlobalSystemMediaTransportControlsSession"/>.
        /// </summary>
        public MediaSessionTimelineTicker Timeline
        {
            get => this._timeline;
            private set
            {
                if (this._timeline is not null)
                {
                    this._timeline.Dispose();
                    this._timeline.TimelineChanged -= this.OnTimelineChanged;
                }

                this._timeline = value;
                if (this._timeline is not null)
                {
                    this._timeline.EnableRaisingEvents = this.EnableRaisingEvents;
                    this._timeline.TimelineChanged += this.OnTimelineChanged;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.Manager.SessionsChanged -= this.OnMediaSessionsChanged;
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override bool Equals(GlobalSystemMediaTransportControlsSession x, GlobalSystemMediaTransportControlsSession y)
            => x?.SourceAppUserModelId == y?.SourceAppUserModelId
            || x is null && y is null;

        /// <inheritdoc/>
        protected override GlobalSystemMediaTransportControlsSession GetSession(ISessionPredicate predicate)
            => predicate is not null
            ? this.Manager.GetSessions().FirstOrDefault(predicate.IsMatch)
            : null;

        /// <inheritdoc/>
        protected override void OnEnableRaisingEventsChanged()
        {
            base.OnEnableRaisingEventsChanged();

            // Propagate event raising to the timeline ticker.
            if (this.Timeline is not null
                && this.Timeline.EnableRaisingEvents != this.EnableRaisingEvents)
            {
                this.Timeline.EnableRaisingEvents = this.EnableRaisingEvents;
            }

            // Update the event handlers on the session.
            if (this.Session is not null)
            {
                if (this.EnableRaisingEvents)
                {
                    this.Session.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                }
                else
                {
                    this.Session.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSessionChanged(GlobalSystemMediaTransportControlsSession oldSession, GlobalSystemMediaTransportControlsSession newSession)
        {
            // Unregister old session events.
            if (oldSession is not null)
            {
                oldSession.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
            }

            // Register new session events, when we have one.
            if (newSession is not null)
            {
                this.SuppressRaisingEvents = true;
                this.Timeline = new MediaSessionTimelineTicker(newSession);
                newSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;

                Task.Run(async () =>
                {
                    try
                    {
                        await this.OnMediaPropertiesChangedAsync(newSession);
                        await this.SetProcessIconFromAsync(newSession.SourceAppUserModelId);
                        base.OnSessionChanged(oldSession, newSession);
                    }
                    finally
                    {
                        this.SuppressRaisingEvents = false;
                    }
                }).Forget();
            }
            else
            {
                this.Thumbnail = null;
                this.Timeline = null;
                this.Title = null;

                base.OnSessionChanged(oldSession, newSession);
            }
        }

        /// <summary>
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSession.MediaPropertiesChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="MediaPropertiesChangedEventArgs"/> instance containing the event data.</param>
        private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
            => this.OnMediaPropertiesChangedAsync(sender).Forget();

        /// <summary>
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSession.MediaPropertiesChanged"/> event asynchronously.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>The task of handling the changing of the media properties.</returns>
        private async Task OnMediaPropertiesChangedAsync(GlobalSystemMediaTransportControlsSession session)
        {
            using (await this._syncRoot.LockAsync())
            {
                if (session is null)
                {
                    this.Thumbnail = null;
                    this.Title = null;

                    return;
                }

                var props = await session.TryGetMediaPropertiesAsync().AsTask();
                this.Title = props?.Title;

                if (props?.Thumbnail is null)
                {
                    this.Thumbnail = null;
                }
                else
                {
                    using (var stream = await props.Thumbnail.OpenReadAsync())
                    using (var img = Image.FromStream(stream.AsStream()))
                    using (var cropped = img.ToSquare())
                    {
                        this.Thumbnail = cropped.ToBase64();
                    }
                }

                if (!this.SuppressRaisingEvents)
                {
                    this.MediaPropertiesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/> event, updating the current session.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SessionsChangedEventArgs"/> instance containing the event data.</param>
        private void OnMediaSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
            => this.RefreshSession();

        /// <summary>
        /// Propagates the <see cref="MediaSessionTimelineTicker.TimelineChanged"/> to <see cref="TimelineChanged"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TimelineEventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
            => this.TimelineChanged?.Invoke(sender, e);

        /// <summary>
        /// Sets the <see cref="SessionWatcher{T}.ProcessIcon"/> from the specified <paramref name="sourceAppUserModelId"/>.
        /// </summary>
        /// <param name="sourceAppUserModelId">The source application user model identifier.</param>
        /// <returns>The task of setting the <see cref="SessionWatcher{T}.ProcessIcon"/>.</returns>
        private async Task SetProcessIconFromAsync(string sourceAppUserModelId)
        {
            if (string.IsNullOrWhiteSpace(sourceAppUserModelId))
            {
                return;
            }

            if (AppInfoUtils.TryGet(sourceAppUserModelId, out var appInfo))
            {
                using (var stream = await appInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(144, 144)).OpenReadAsync())
                using (var cryptoStream = new CryptoStream(stream.AsStream(), new ToBase64Transform(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    this.SetProcessIcon($"data:image/png;base64,{reader.ReadToEnd()}");
                }
            }
            else if (Process.GetProcessesByName(sourceAppUserModelId) is Process[] processes and { Length: > 0 })
            {
                this.SetProcessIcon(processes[0].GetIconAsBase64());
            }
        }
    }
}
