namespace SoundDeck.Core.Sessions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.Volume;
    using Windows.Media.Control;

    /// <summary>
    /// Provides methods for monitoring audio and media playback associated with an application.
    /// </summary>
    public sealed class AppAudioSession : IDisposable, IAudioSessionEventsHandler
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private backing field for <see cref="Audio"/>.
        /// </summary>
        private AudioSessionControl _audioSession;

        /// <summary>
        /// Private backing field for <see cref="EnableRaisingEvents"/>.
        /// </summary>
        private bool _enableRaisingEvents = true;

        /// <summary>
        /// Private backing field for <see cref="Predicate"/>.
        /// </summary>
        private ISessionPredicate _predicate;

        /// <summary>
        /// Private backing field for <see cref="Media"/>.
        /// </summary>
        private GlobalSystemMediaTransportControlsSession _mediaSession;

        /// <summary>
        /// Private backing field for <see cref="ThumbnailAsBase64"/>.
        /// </summary>
        private string _thumbnail;

        /// <summary>
        /// Private backing field for <see cref="GetTimelineTicker()"/>.
        /// </summary>
        private MediaSessionTimelineTicker _timelineTicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppAudioSession"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="predicate">The predicate.</param>
        public AppAudioSession(GlobalSystemMediaTransportControlsSessionManager manager, ISessionPredicate predicate)
        {
            this.DeviceEnumerator = new MMDeviceEnumerator();
            this.Manager = manager;
            this.Predicate = predicate;

            this.Manager.SessionsChanged += this.OnMediaSessionsChanged;
            foreach (var device in this.DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                device.AudioSessionManager.OnSessionCreated += this.OnAudioSessionCreated;
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
        /// Occurs when the volume changes.
        /// </summary>
        public event EventHandler<VolumeEventArgs> VolumeChanged;

        public AudioSessionControl Audio
        {
            get => this._audioSession;
            private set
            {
                lock (this._syncRoot)
                {
                    this._audioSession?.UnRegisterEventClient(this);

                    this._audioSession = value;
                    if (this.EnableRaisingEvents)
                    {
                        this._audioSession?.RegisterEventClient(this);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can raise events.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => this._enableRaisingEvents;
            set
            {
                lock (this._syncRoot)
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
                    if (this._enableRaisingEvents)
                    {
                        this._audioSession?.RegisterEventClient(this);
                        if (this._mediaSession != null)
                        {
                            this._mediaSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                        }
                    }
                    else
                    {
                        this._audioSession?.UnRegisterEventClient(this);
                        if (this._mediaSession != null)
                        {
                            this._mediaSession.MediaPropertiesChanged -= this.OnMediaPropertiesChanged;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current media session.
        /// </summary>
        public GlobalSystemMediaTransportControlsSession Media
        {
            get => this._mediaSession;
            private set
            {
                lock (this._syncRoot)
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
                        this._timelineTicker = new MediaSessionTimelineTicker(value);
                        this._timelineTicker.EnableRaisingEvents = this._enableRaisingEvents;

                        if (this.EnableRaisingEvents)
                        {
                            this._mediaSession.MediaPropertiesChanged += this.OnMediaPropertiesChanged;
                            this._timelineTicker.TimelineChanged += this.OnTimelineChanged;
                        }

                        this.OnMediaPropertiesChangedAsync(value).Forget();
                    }

                    this.TrackPosition = TimeSpan.Zero;
                    this.TrackEndTime = TimeSpan.Zero;

                    this.MediaSessionChanged?.Invoke(this, EventArgs.Empty);
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

                        this.Audio = value == null ? null : this.DeviceEnumerator.GetAudioSessions(DataFlow.Render).FirstOrDefault(value.IsMatch);
                        this.Media = value == null ? null : this.Manager.GetSessions().FirstOrDefault(value.IsMatch);
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
        /// Gets or sets the device enumerator.
        /// </summary>
        private MMDeviceEnumerator DeviceEnumerator { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Predicate = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles <see cref="AudioSessionManager.OnSessionCreated"/> event, updating the <see cref="Audio"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="newSession">The new session.</param>
        private void OnAudioSessionCreated(object sender, IAudioSessionControl newSession)
        {
            lock (this._syncRoot)
            {
                this.Audio = this.Predicate == null
                    ? null
                    : this.DeviceEnumerator.GetAudioSessions(DataFlow.Render).FirstOrDefault(this.Predicate.IsMatch);
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
        /// Handles the <see cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/> event, updating the current session.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SessionsChangedEventArgs"/> instance containing the event data.</param>
        private void OnMediaSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            lock (this._syncRoot)
            {
                if (this.Predicate != null)
                {
                    this.Media = sender
                        .GetSessions()
                        .FirstOrDefault(this.Predicate.IsMatch);
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
        /// Propages <see cref="IAudioSessionEventsHandler.OnVolumeChanged(float, bool)"/>
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="isMuted">Value indicating whether the volume is muted.</param>
        void IAudioSessionEventsHandler.OnVolumeChanged(float volume, bool isMuted)
            => this.VolumeChanged?.Invoke(this, new VolumeEventArgs(volume, isMuted));

        #region IAudioSessionEventsHandler
        void IAudioSessionEventsHandler.OnDisplayNameChanged(string displayName) { }
        void IAudioSessionEventsHandler.OnIconPathChanged(string iconPath) { }
        void IAudioSessionEventsHandler.OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex) { }
        void IAudioSessionEventsHandler.OnGroupingParamChanged(ref Guid groupingId) { }
        void IAudioSessionEventsHandler.OnStateChanged(AudioSessionState state) { }
        void IAudioSessionEventsHandler.OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason) { }
        #endregion
    }
}
