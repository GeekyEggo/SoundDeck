namespace SoundDeck.Core.Extensions
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides extension methods for <see cref="Task"/>.
    /// </summary>
    /// <remarks>Credit: https://stackoverflow.com/a/67877165/259656</remarks>
    public static class TaskExtensions
    {
        /// <summary>
        /// Safely executes the task asynchronously without awaiting the result.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to execute.</param>
        /// <param name="logger">The <see cref="ILogger"/> responsible for logging exceptions.</param>
        /// <param name="callingMethodName">Name of the calling method.</param>
        public static void Forget(this Task task, ILogger logger = null, [CallerMemberName] string callingMethodName = "")
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            // Allocate the async/await state machine only when needed for performance reasons.
            // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/?WT.mc_id=DT-MVP-5003978
            // Pass params explicitly to async local function or it will allocate to pass them
            static async Task ForgetAwaited(Task task, ILogger logger = null, string callingMethodName = "")
            {
                try
                {
                    await task.ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (TaskCanceledException tce)
                {
                    logger?.LogError(tce, $"Fire and forget task was canceled for calling method: {callingMethodName}");
                }
                catch (Exception e)
                {
                    logger?.LogError(e, $"Fire and forget task failed for calling method: {callingMethodName}");
                }
            }

            // Note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
            // Only care about tasks that may fault (not completed) or are faulted, so fast-path for SuccessfullyCompleted and Canceled tasks.
            if (!task.IsCanceled && (!task.IsCompleted || task.IsFaulted))
            {
                _ = ForgetAwaited(task, logger, callingMethodName);
            }
        }
    }
}
