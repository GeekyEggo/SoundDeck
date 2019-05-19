namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extensions for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Asynchronously waits for the semaphore to be free, and then executes <paramref name="action"/>.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="action">The action.</param>
        /// <returns>The task of the semaphore executing the <paramref name="action"/>.</returns>
        public static Task WaitAsync(this SemaphoreSlim semaphore, Action action)
        {
            return semaphore.WaitAsync<object>(() =>
            {
                action();
                return null;
            });
        }

        /// <summary>
        /// Asynchronously waits for the semaphore to be free, and then executes <paramref name="func"/>, returning the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="func">The action.</param>
        /// <returns>The result of <paramref name="func"/>.</returns>
        public static async Task<TResult> WaitAsync<TResult>(this SemaphoreSlim semaphore, Func<TResult> func)
        {
            await semaphore.WaitAsync();
            try
            {
                return func();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
