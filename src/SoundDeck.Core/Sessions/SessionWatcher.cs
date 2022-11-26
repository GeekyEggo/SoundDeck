namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using SoundDeck.Core.Extensions;
    using SoundDeck.Core.IO;

    /// <summary>
    /// Provides a base class for a session watcher, capable of wrapping and monitoring a session related to audio.
    /// </summary>
    /// <typeparam name="T">The type of session.</typeparam>
    public abstract class SessionWatcher<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// The synchronization root
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Private backing field for <see cref="EnableRaisingEvents"/>.
        /// </summary>
        private bool _enableRaisingEvents = true;

        /// <summary>
        /// Private backing field for <see cref="Predicate"/>.
        /// </summary>
        private ISessionPredicate _predicate;

        /// <summary>
        /// Private backing field for <see cref="SelectionCriteria"/>.
        /// </summary>
        private IProcessSelectionCriteria _selectionCriteria;

        /// <summary>
        /// Private backing field for <see cref="Session"/>.
        /// </summary>
        private T _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionWatcher{T}"/> class.
        /// </summary>
        /// <param name="selectionCriteria">The <see cref="IProcessSelectionCriteria"/>.</param>
        public SessionWatcher(IProcessSelectionCriteria selectionCriteria)
        {
            this._selectionCriteria = selectionCriteria;
            this._predicate = selectionCriteria.ToPredicate();

            lock (_syncRoot)
            {
                if (this.ProcessIconCacheFilePath is not null
                    && File.Exists(this.ProcessIconCacheFilePath))
                {
                    this.ProcessIcon = File.ReadAllText(this.ProcessIconCacheFilePath, Encoding.UTF8);
                }

                if (selectionCriteria.ProcessSelectionType == ProcessSelectionType.Foreground)
                {
                    ForegroundProcess.Changed += this.ForegroundProcessChanged;
                }
            }
        }

        /// <summary>
        /// Occurs when <see cref="ProcessIcon"/> changes.
        /// </summary>
        public event EventHandler<string> ProcessIconChanged;

        /// <summary>
        /// Occurs when <see cref="Session"/> changes.
        /// </summary>
        public event EventHandler<T> SessionChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can raise events.
        /// </summary>
        public bool EnableRaisingEvents
        {
            get => this._enableRaisingEvents;
            set
            {
                lock (_syncRoot)
                {
                    if (this._enableRaisingEvents != value)
                    {
                        this._enableRaisingEvents = value;
                        this.OnEnableRaisingEventsChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the process image, as a base64 encoded string.
        /// </summary>
        public string ProcessIcon { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IProcessSelectionCriteria"/> used to determine the <see cref="Session"/>.
        /// </summary>
        public IProcessSelectionCriteria SelectionCriteria
        {
            get => this._selectionCriteria;
            set
            {
                lock (_syncRoot)
                {
                    if (this._selectionCriteria is not null and { ProcessSelectionType: ProcessSelectionType.Foreground }
                        && value is null or { ProcessSelectionType: ProcessSelectionType.ByName })
                    {
                        ForegroundProcess.Changed -= this.ForegroundProcessChanged;
                    }

                    this._selectionCriteria = value;
                    this.Predicate = this._selectionCriteria?.ToPredicate();

                    if (this._selectionCriteria is { ProcessSelectionType: ProcessSelectionType.Foreground })
                    {
                        ForegroundProcess.Changed += this.ForegroundProcessChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current session.
        /// </summary>
        public T Session
        {
            get => this._session;
            protected set
            {
                lock (_syncRoot)
                {
                    if (!this.Equals(this._session, value))
                    {
                        var oldValue = this._session;
                        this._session = value;

                        this.OnSessionChanged(oldValue, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the predicate responsible for determining the session.
        /// </summary>
        private ISessionPredicate Predicate
        {
            get => this._predicate;
            set
            {
                lock (_syncRoot)
                {
                    if (value?.Equals(this._predicate) == false)
                    {
                        this._predicate = value;
                        this.Session = this.GetSession(this._predicate);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the file path of the cached icon.
        /// </summary>
        private string ProcessIconCacheFilePath
        {
            get
            {
                lock (_syncRoot)
                {
                    if (this.Predicate is not null
                        && this.Predicate.ProcessName is not null and not "")
                    {
                        var safeFileName = string.Concat(this.Predicate.ProcessName.Split(Path.GetInvalidFileNameChars()));
                        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SoundDeck\IconsCache\", $"{safeFileName}.txt");
                    }

                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract bool Equals(T x, T y);

        /// <inheritdoc/>
        public int GetHashCode(T obj)
            => obj?.GetHashCode() ?? 0;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            ForegroundProcess.Changed -= this.ForegroundProcessChanged;
            this.SelectionCriteria = null;
        }

        /// <summary>
        /// Gets the session that matches the <see cref="Predicate"/>.
        /// </summary>
        /// <param name="predicate">The session predicate.</param>
        /// <returns>The session.</returns>
        protected abstract T GetSession(ISessionPredicate predicate);

        /// <summary>
        /// Called when <see cref="EnableRaisingEvents"/> changes.
        /// </summary>
        protected virtual void OnEnableRaisingEventsChanged() { }

        /// <summary>
        /// Gets or sets a value indicating whether events should be suppressed, unless explicitly invoked.
        /// </summary>
        protected bool SuppressRaisingEvents { get; set; } = false;

        /// <summary>
        /// Called when <see cref="Session"/> changes, and raises <see cref="SessionChanged"/>.
        /// </summary>
        /// <param name="oldSession">The old session.</param>
        /// <param name="newSession">The new session; supplied to the <see cref="SessionChanged"/> event.</param>
        protected virtual void OnSessionChanged(T oldSession, T newSession)
            => this.SessionChanged?.Invoke(this, newSession);

        /// <summary>
        /// Refreshes the current session.
        /// </summary>
        protected void RefreshSession()
        {
            lock (_syncRoot)
            {
                this._predicate = this.SelectionCriteria.ToPredicate();
                this.Session = this.GetSession(this.Predicate);
            }
        }

        /// <summary>
        /// Sets the <see cref="ProcessIcon"/>, and raises <see cref="ProcessIconChanged"/> if the value has changed and <paramref name="suppressRaisingEvents"/> is <c>true</c>.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void SetProcessIcon(string value)
        {
            lock (_syncRoot)
            {
                if (this.ProcessIcon != value
                    && !string.IsNullOrWhiteSpace(value))
                {
                    this.ProcessIcon = value;
                    if (!this.SuppressRaisingEvents)
                    {
                        this.ProcessIconChanged?.Invoke(this, value);
                    }

                    if (this.ProcessIconCacheFilePath is string filePath and not null)
                    {
                        FileUtils.WriteAllText(filePath, value);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="ForegroundProcess.Changed"/> event, and updates the <see cref="Session"/> if one exists for the active foreground.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ForegroundProcessChanged(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                if (this.SelectionCriteria is { ProcessSelectionType: ProcessSelectionType.Foreground }
                    && this.GetSession(new ProcessIdentifierPredicate(ForegroundProcess.Id)) is T session and not null)
                {
                    this.Session = session;
                }
            }
        }
    }
}