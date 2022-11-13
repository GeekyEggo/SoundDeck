namespace SoundDeck.Core.Sessions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using SoundDeck.Core.Extensions;
    using Windows.Media.Control;

    /// <summary>
    /// Provides methods for monitoring the timeline, thumbnail, and playback information associated with a media session.
    /// </summary>
    public sealed class MediaSessionWatcher : IDisposable
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private backing field for <see cref="EnableRaisingEvents"/>.
        /// </summary>
        private bool _enableRaisingEvents = true;

        /// <summary>
        /// Private backing field for <see cref="ThumbnailAsBase64"/>.
        /// </summary>
        private string _thumbnail;

        /// <summary>
        /// Private backing field for <see cref="Predicate"/>.
        /// </summary>
        private ISessionPredicate _predicate;

        /// <summary>
        /// Private backing field for <see cref="MediaSession"/>.
        /// </summary>
        private GlobalSystemMediaTransportControlsSession _mediaSession;

        /// <summary>
        /// Private backing field for <see cref="GetTimelineTicker()"/>.
        /// </summary>
        private MediaSessionTimelineTicker _timelineTicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSessionWatcher"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="predicate">The predicate.</param>
        public MediaSessionWatcher(GlobalSystemMediaTransportControlsSessionManager manager, ISessionPredicate predicate)
        {
            this.Manager = manager;
            this.Predicate = predicate;
            this.Manager.SessionsChanged += this.ManagerSessionChanged;
        }

        /// <summary>
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/> event, updating the current session.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SessionsChangedEventArgs"/> instance containing the event data.</param>
        private void ManagerSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            lock (this._syncRoot)
            {
                if (this.Predicate != null)
                {
                    this.MediaSession = sender
                        .GetSessions()
                        .FirstOrDefault(this.Predicate.IsMatch);
                }
            }
        }

        /// <summary>
        /// Occurs when the media session changes, i.e. it becomes another media session.
        /// </summary>
        public event EventHandler MediaSessionChanged;

        /// <summary>
        /// Occurs when session timeline changes.
        /// </summary>
        public event EventHandler<TimelineEventArgs> TimelineChanged;

        /// <summary>
        /// Occurs when <see cref="ThumbnailAsBase64"/> changes.
        /// </summary>
        public event EventHandler ThumbnailChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can raise events.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => this._enableRaisingEvents;
            set
            {
                if (this._timelineTicker != null
                    && this._timelineTicker.EnableRaisingEvents != value)
                {
                    this._timelineTicker.EnableRaisingEvents = value;
                }

                if (this._enableRaisingEvents == value)
                {
                    return;
                }

                this._enableRaisingEvents = value;

                var session = this._mediaSession;
                if (session != null)
                {
                    if (value)
                    {
                        session.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                    }
                    else
                    {
                        session.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the predicate responsible for determining the session.
        /// </summary>
        public ISessionPredicate Predicate
        {
            get => this._predicate;
            set
            {
                lock (this._syncRoot)
                {
                    if (value?.Equals(this._predicate) == false)
                    {
                        this._predicate = value;
                        this.MediaSession = value == null ? null : this.Manager.GetSessions().FirstOrDefault(value.IsMatch);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the thumbnail associated with the media, in base64 format.
        /// </summary>
        public string ThumbnailAsBase64
        {
            get => this._thumbnail;
            private set
            {
                if (this._thumbnail != value)
                {
                    this._thumbnail = value;
                    this.ThumbnailChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the current media session.
        /// </summary>
        public GlobalSystemMediaTransportControlsSession MediaSession
        {
            get => this._mediaSession;
            private set
            {
                if (this._mediaSession != null)
                {
                    this._mediaSession.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;

                    if (this._timelineTicker != null)
                    {
                        this._timelineTicker.Dispose();
                        this._timelineTicker.TimelineChanged -= this.OnTimelineChanged;
                        this._timelineTicker = null;
                    }
                }

                this._mediaSession = value;
                if (this._mediaSession != null)
                {
                    this._mediaSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;

                    this._timelineTicker = new MediaSessionTimelineTicker(value);
                    this._timelineTicker.EnableRaisingEvents = this._enableRaisingEvents;
                    this._timelineTicker.TimelineChanged += this.OnTimelineChanged;

                    this.OnMediaPropertiesChangedAsync(value).Forget();
                }

                this.MediaSessionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the active media session manager.
        /// </summary>
        private GlobalSystemMediaTransportControlsSessionManager Manager { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Predicate = null;
            GC.SuppressFinalize(this);
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
            if (session == null)
            {
                this.ThumbnailAsBase64 = string.Empty;
                return;
            }

            var props = await session.TryGetMediaPropertiesAsync().AsTask();
            if (props.Thumbnail == null)
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
        /// Propagates the <see cref="MediaSessionTimelineTicker.TimelineChanged"/> to <see cref="TimelineChanged"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TimelineEventArgs"/> instance containing the event data.</param>
        private void OnTimelineChanged(object sender, TimelineEventArgs e)
            => this.TimelineChanged?.Invoke(sender, e);
    }
}
