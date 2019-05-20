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
        /// Synchronously waits for the semaphore to be free, and then executes <paramref name="func"/>.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="func">The action.</param>
        /// <returns>The result of <paramref name="func"/>.</returns>
        public static void Wait(this SemaphoreSlim semaphore, Action action)
        {
            semaphore.Wait();
            try
            {
                action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Asynchronously waits for the semaphore to be free, and then executes <paramref name="action"/>.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="action">The action.</param>
        /// <returns>The task of the semaphore executing the <paramref name="action"/>.</returns>
        public static Task WaitAsync(this SemaphoreSlim semaphore, Action action)
            => semaphore.WaitAsync<object>(() => Task.Run(action));

        /// <summary>
        /// Asynchronously waits for the semaphore to be free, and then executes <paramref name="func"/>, returning the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="func">The action.</param>
        /// <returns>The result of <paramref name="func"/>.</returns>
        public static Task<TResult> WaitAsync<TResult>(this SemaphoreSlim semaphore, Func<TResult> func)
            => semaphore.WaitAsync(() => Task.Run(func));

        /// <summary>
        /// Asynchronously waits for the semaphore to be free, and then executes <paramref name="func"/>, returning the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="func">The action.</param>
        /// <returns>The result of <paramref name="func"/>.</returns>
        public static async Task<TResult> WaitAsync<TResult>(this SemaphoreSlim semaphore, Func<Task<TResult>> func)
        {
            await semaphore.WaitAsync();
            try
            {
                var result = await func();
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
