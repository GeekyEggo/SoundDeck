namespace SoundDeck.Core.Sessions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using SoundDeck.Core.Extensions;
    using Windows.ApplicationModel;
    using Windows.Media.Control;

    /// <summary>
    /// Provides a session watcher for <see cref="GlobalSystemMediaTransportControlsSession"/>.
    /// </summary>
    public class MediaSessionWatcher : SessionWatcher<GlobalSystemMediaTransportControlsSession>
    {
        /// <summary>
        /// Private backing field for <see cref="ThumbnailAsBase64"/>.
        /// </summary>
        private string _thumbnailAsBase64;

        /// <summary>
        /// Private backing field for <see cref="TimelineTicker"/>.
        /// </summary>
        private MediaSessionTimelineTicker _timelineTicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSessionWatcher"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="GlobalSystemMediaTransportControlsSessionManager"/>.</param>
        /// <param name="predicate">The <see cref="ISessionPredicate"/>.</param>
        public MediaSessionWatcher(GlobalSystemMediaTransportControlsSessionManager manager, ISessionPredicate predicate)
            : base()
        {
            this.Manager = manager;
            this.Predicate = predicate;

            this.Manager.SessionsChanged += this.OnMediaSessionsChanged;
        }

        /// <summary>
        /// Occurs when session timeline changes.
        /// </summary>
        public event EventHandler<TimelineEventArgs> TimelineChanged;

        /// <summary>
        /// Occurs when <see cref="ThumbnailAsBase64"/> changes.
        /// </summary>
        public event EventHandler ThumbnailChanged;

        /// <summary>
        /// Gets the thumbnail associated with the media, in base64 format.
        /// </summary>
        public string ThumbnailAsBase64
        {
            get => this._thumbnailAsBase64;
            private set
            {
                if (this._thumbnailAsBase64 != value)
                {
                    this._thumbnailAsBase64 = value;
                    this.ThumbnailChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the track position.
        /// </summary>
        public TimeSpan TrackPosition { get; private set; }

        /// <summary>
        /// Gets the track end time.
        /// </summary>
        public TimeSpan TrackEndTime { get; private set; }

        /// <summary>
        /// Gets the active media session manager.
        /// </summary>
        private GlobalSystemMediaTransportControlsSessionManager Manager { get; }

        /// <summary>
        /// Gets or sets the <see cref="MediaSessionTimelineTicker"/> associated with the <see cref="GlobalSystemMediaTransportControlsSession"/>.
        /// </summary>
        private MediaSessionTimelineTicker TimelineTicker
        {
            get => this._timelineTicker;
            set
            {
                if (this._timelineTicker is not null)
                {
                    this._timelineTicker.Dispose();
                    this._timelineTicker.TimelineChanged -= this.OnTimelineChanged;
                }

                this._timelineTicker = value;
                if (this._timelineTicker is not null)
                {
                    this._timelineTicker.EnableRaisingEvents = this.EnableRaisingEvents;
                    this._timelineTicker.TimelineChanged += this.OnTimelineChanged;
                }
            }
        }

        /// <inheritdoc/>
        public override bool Equals(GlobalSystemMediaTransportControlsSession x, GlobalSystemMediaTransportControlsSession y)
            => x?.SourceAppUserModelId == y?.SourceAppUserModelId
            || x is null && y is null;

        /// <inheritdoc/>
        protected override GlobalSystemMediaTransportControlsSession GetSession()
            => this.Predicate is not null
            ? this.Manager.GetSessions().FirstOrDefault(this.Predicate.IsMatch)
            : null;

        /// <inheritdoc/>
        protected override void OnEnableRaisingEventsChanged()
        {
            base.OnEnableRaisingEventsChanged();

            // Propagate event raising to the timeline ticker.
            if (this.TimelineTicker != null
                && this.TimelineTicker.EnableRaisingEvents != this.EnableRaisingEvents)
            {
                this.TimelineTicker.EnableRaisingEvents = this.EnableRaisingEvents;
            }

            // Update the event handlers on the session.
            if (this.Session != null)
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
            if (oldSession != null)
            {
                oldSession.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
            }

            // Register new session events, when we have one.
            if (newSession != null)
            {
                this.TimelineTicker = new MediaSessionTimelineTicker(newSession);

                newSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                this.OnMediaPropertiesChangedAsync(newSession).Forget();
                this.RefreshLogo(newSession.SourceAppUserModelId).Forget();
            }
            else
            {
                this.TimelineTicker = null;
                this.ProcessImageAsBase64 = string.Empty;
            }

            // Reset the track timeline info.
            this.TrackPosition = TimeSpan.Zero;
            this.TrackEndTime = TimeSpan.Zero;

            base.OnSessionChanged(oldSession, newSession);
        }

        private async Task RefreshLogo(string sourceAppUserModelId)
        {
            if (string.IsNullOrWhiteSpace(sourceAppUserModelId))
            {
                this.ProcessImageAsBase64 = string.Empty;
            }
            else
            {
                var appInfo = AppInfo.GetFromAppUserModelId(sourceAppUserModelId);
                if (appInfo is null)
                {
                    this.ProcessImageAsBase64 = string.Empty;
                }

                using (var stream = await appInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(144, 144)).OpenReadAsync())
                using (var cryptoStream = new CryptoStream(stream.AsStream(), new ToBase64Transform(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    this.ProcessImageAsBase64 = $"data:image/png;base64,{reader.ReadToEnd()}";
                }
            }
        }

        /// <summary>
        /// Propagates the <see cref="MediaSessionTimelineTicker.TimelineChanged"/> to <see cref="TimelineChanged"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TimelineEventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
        {
            this.TrackEndTime = e.EndTime;
            this.TrackPosition = e.Position;

            this.TimelineChanged?.Invoke(sender, e);
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
        private async Task OnMediaPropertiesChangedAsync(GlobalSystemMediaTransportControlsSession session)
        {
            if (session is null)
            {
                this.ThumbnailAsBase64 = string.Empty;
                return;
            }

            var props = await session.TryGetMediaPropertiesAsync().AsTask();
            if (props.Thumbnail is null)
            {
                this.ThumbnailAsBase64 = string.Empty;
                return;
            }

            using (var stream = await props.Thumbnail.OpenReadAsync())
            using (var cryptoStream = new CryptoStream(stream.AsStream(), new ToBase64Transform(), CryptoStreamMode.Read))
            using (var reader = new StreamReader(cryptoStream))
            {
                this.ThumbnailAsBase64 = $"data:image/png;base64,{reader.ReadToEnd()}";
            }
        }

        /// <summary>
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/> event, updating the current session.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SessionsChangedEventArgs"/> instance containing the event data.</param>
        private void OnMediaSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            if (this.Predicate is ISessionPredicate predicate)
            {
                this.Session = sender.GetSessions().FirstOrDefault(predicate.IsMatch);
            }
        }
    }
}
