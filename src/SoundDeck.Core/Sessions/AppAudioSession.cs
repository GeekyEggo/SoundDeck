/**
 * TODO:
 * - Fix display name
 * - Fix process icon
 * - Monitor sessions being created.
 */

namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Linq;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;

    /// <summary>
    /// Provides a <see cref="SessionWatcher{T}"/> for <see cref="AudioSessionControl"/>, capable of monitoring the volume of an application.
    /// </summary>
    public sealed class AppAudioSession : SessionWatcher<AudioSessionControl>, IAudioSessionEventsHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppAudioSession"/> class.
        /// </summary>
        /// <param name="appAudioService">The application audio service.</param>
        /// <param name="selectionCriteria">The selection criteria.</param>
        public AppAudioSession(IAppAudioService appAudioService, IProcessSelectionCriteria selectionCriteria)
            : base (selectionCriteria)
        {
            this.AppAudioService = appAudioService;
            this.RefreshSession();
        }

        /// <summary>
        /// Occurs when the session state changes; this can occur due to the volume state or display name changing, or the session disconnects, etc.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Gets the display name of the audio session.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the audio session is muted.
        /// </summary>
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Gets the volume of the audio session.
        /// </summary>
        public float Volume { get; private set; }

        /// <summary>
        /// Gets the <see cref="IAppAudioService"/>.
        /// </summary>
        private IAppAudioService AppAudioService { get; }

        /// <inheritdoc/>
        public override bool Equals(AudioSessionControl x, AudioSessionControl y)
            => x?.GetProcessID == y?.GetProcessID
            || x is null && y is null;

        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnDisplayNameChanged(string displayName)
        {
            this.DisplayName = displayName;
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnSessionDisconnected(AudioSessionDisconnectReason disconnectReason)
            => this.Session = null;

        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnStateChanged(AudioSessionState state)
        {
            // todo: ?
        }

        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnVolumeChanged(float volume, bool isMuted)
        {
            this.Volume = volume;
            this.IsMuted = isMuted;
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        protected override AudioSessionControl GetSession(ISessionPredicate predicate)
            => predicate is null
            ? null
            : this.AppAudioService.GetAudioSessions().FirstOrDefault(predicate.IsMatch);

        /// <inheritdoc/>
        protected override void OnEnableRaisingEventsChanged()
        {
            base.OnEnableRaisingEventsChanged();
            if (this.EnableRaisingEvents)
            {
                this.Session?.RegisterEventClient(this);
            }
            else
            {
                this.Session?.UnRegisterEventClient(this);
            }
        }

        /// <inheritdoc/>
        protected override void OnSessionChanged(AudioSessionControl oldSession, AudioSessionControl newSession)
        {
            base.OnSessionChanged(oldSession, newSession);
            oldSession?.UnRegisterEventClient(this);
            newSession?.RegisterEventClient(this);

            if (newSession is not null)
            {
                this.Volume = newSession.SimpleAudioVolume.Volume;
                this.IsMuted = newSession.SimpleAudioVolume.Mute;
                this.DisplayName = newSession.DisplayName;

                /*
                if (Process.GetProcessById((int)newSession.GetProcessID) is Process process)
                {
                    this.SetProcessIcon(process.GetIconAsBase64());
                }
                else
                {
                    this.SetProcessIcon(string.Empty);
                }
                */
            }
            else
            {
                this.Volume = 0;
                this.IsMuted = false;
                this.DisplayName = null;
                this.SetProcessIcon(null);
            }

            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        #region IAudioSessionEventsHandler
        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnChannelVolumeChanged(uint channelCount, IntPtr newVolumes, uint channelIndex) { }
        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnGroupingParamChanged(ref Guid groupingId) { }
        /// <inheritdoc/>
        void IAudioSessionEventsHandler.OnIconPathChanged(string iconPath) { }
        #endregion
    }
}
