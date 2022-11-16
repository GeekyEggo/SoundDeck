namespace SoundDeck.Core.Sessions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a base class for a session watcher, capable of wrapping and monitoring a session related to audio.
    /// </summary>
    /// <typeparam name="T">The type of session.</typeparam>
    public abstract class SessionWatcher<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// The synchronization root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private backing field for <see cref="EnableRaisingEvents"/>.
        /// </summary>
        private bool _enableRaisingEvents = true;

        /// <summary>
        /// Private backing field for <see cref="Predicate"/>.
        /// </summary>
        private ISessionPredicate _predicate;

        /// <summary>
        /// Private backing field for <see cref="ProcessImageAsBase64"/>.
        /// </summary>
        private string _processImageAsBase64;

        /// <summary>
        /// Private backing field for <see cref="Session"/>.
        /// </summary>
        private T _session;

        /// <summary>
        /// Occurs when <see cref="ProcessImageAsBase64"/> changes.
        /// </summary>
        public event EventHandler<string> ProcessImageChanged;

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
                lock (this._syncRoot)
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
                        this.Session = this.GetSession();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the process image, as a base64 encoded string.
        /// </summary>
        public string ProcessImageAsBase64
        {
            get => this._processImageAsBase64;
            protected set
            {
                if (this._processImageAsBase64 != value
                    && !string.IsNullOrWhiteSpace(value))
                {
                    this._processImageAsBase64 = value;
                    this.ProcessImageChanged?.Invoke(this, value);
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
                lock (this._syncRoot)
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
            => this.Predicate = null;

        /// <summary>
        /// Gets the session that matches the <see cref="Predicate"/>.
        /// </summary>
        /// <returns>The session.</returns>
        protected abstract T GetSession();

        /// <summary>
        /// Called when <see cref="EnableRaisingEvents"/> changes.
        /// </summary>
        protected virtual void OnEnableRaisingEventsChanged() { }

        /// <summary>
        /// Called when <see cref="Session"/> changes, and raises <see cref="SessionChanged"/>.
        /// </summary>
        /// <param name="oldSession">The old session.</param>
        /// <param name="newSession">The new session; supplied to the <see cref="SessionChanged"/> event.</param>
        protected virtual void OnSessionChanged(T oldSession, T newSession)
            => this.SessionChanged?.Invoke(this, newSession);
    }
}