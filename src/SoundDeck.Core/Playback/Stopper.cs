namespace SoundDeck.Core.Playback
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides a base class that allows for stopping via a <see cref="CancellationToken"/> and <see cref="IStopper.Stop"/>.
    /// </summary>
    public class Stopper : IStopper
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Private member field that supports <see cref="ActiveCancellationToken"/>.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <inheritdoc/>
        public event EventHandler Disposed;

        /// <summary>
        /// Gets the active cancellation token.
        /// </summary>
        protected CancellationToken ActiveCancellationToken
        {
            get
            {
                lock (this._syncRoot)
                {
                    if (this._cancellationTokenSource.IsCancellationRequested
                        && !this.IsDisposed)
                    {
                        this._cancellationTokenSource = new CancellationTokenSource();
                    }

                    return this._cancellationTokenSource.Token;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        private bool IsDisposed { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Stop()
            => this._cancellationTokenSource.Cancel();

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            lock (this._syncRoot)
            {
                this._cancellationTokenSource.Cancel();
                if (!this.IsDisposed)
                {
                    this.Disposed?.Invoke(this, EventArgs.Empty);
                }

                this.IsDisposed = true;
            }
        }
    }
}
