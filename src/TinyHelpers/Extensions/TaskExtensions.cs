﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TinyHelpers.Extensions
{
    public static class TaskExtensions
    {
        public static async Task WaitAsync(this Task task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(false);
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                await task.ConfigureAwait(false);
            }
            else
            {
                throw new TimeoutException();
            }
        }

        public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(false);
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task.ConfigureAwait(false);
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}
