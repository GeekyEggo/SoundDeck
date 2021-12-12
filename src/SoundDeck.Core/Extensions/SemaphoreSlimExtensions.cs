namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Waits to enter this instance; upon disposal of the result, the instance is released once.
        /// </summary>
        /// <param name="semaphore">This instance.</param>
        /// <returns>The object responsible for releasing the instance, once.</returns>
        public static IDisposable Lock(this SemaphoreSlim semaphore)
        {
            semaphore.Wait();
            return new SemaphoreSlimReleaser(semaphore);
        }

        /// <summary>
        /// Asynchronously waits to enter this instance; upon disposal of the result, the instance is released once.
        /// </summary>
        /// <param name="semaphore">This instance.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>The object responsible for releasing the instance, once.</returns>
        public static async Task<IDisposable> LockAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            return new SemaphoreSlimReleaser(semaphore);
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlim"/> upon disposal.
        /// </summary>
        private sealed class SemaphoreSlimReleaser : IDisposable
        {
            /// <summary>
            /// The semaphore.
            /// </summary>
            private SemaphoreSlim _semaphore;

            /// <summary>
            /// Initializes a new instance of the <see cref="SemaphoreSlimReleaser"/> class.
            /// </summary>
            /// <param name="semaphore">The semaphore.</param>
            public SemaphoreSlimReleaser(SemaphoreSlim semaphore)
                => this._semaphore = semaphore;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
                => Interlocked.CompareExchange(ref this._semaphore, null, null)?.Release();
        }
    }
}
