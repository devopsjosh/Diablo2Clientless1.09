using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleBot.Helpers;

public static class BotRunControl
{
    private static readonly object LockObject = new();
    private static bool _isStopped;
    private static TaskCompletionSource<bool> _resumeSignal = CompletedSignal();

    public static bool IsStopped
    {
        get
        {
            lock (LockObject)
            {
                return _isStopped;
            }
        }
    }

    public static bool Stop()
    {
        lock (LockObject)
        {
            if (_isStopped)
            {
                return false;
            }

            _isStopped = true;
            _resumeSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return true;
        }
    }

    public static bool Start()
    {
        lock (LockObject)
        {
            if (!_isStopped)
            {
                return false;
            }

            _isStopped = false;
            _resumeSignal.TrySetResult(true);
            return true;
        }
    }

    public static Task WaitIfStoppedAsync(CancellationToken cancellationToken = default)
    {
        Task waitTask;
        lock (LockObject)
        {
            if (!_isStopped)
            {
                return Task.CompletedTask;
            }

            waitTask = _resumeSignal.Task;
        }

        return waitTask.WaitAsync(cancellationToken);
    }

    public static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (delay <= TimeSpan.Zero)
        {
            await WaitIfStoppedAsync(cancellationToken);
            return;
        }

        var deadline = DateTime.UtcNow + delay;
        while (true)
        {
            await WaitIfStoppedAsync(cancellationToken);

            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                return;
            }

            var slice = remaining > TimeSpan.FromMilliseconds(500)
                ? TimeSpan.FromMilliseconds(500)
                : remaining;

            await Task.Delay(slice, cancellationToken);
        }
    }

    private static TaskCompletionSource<bool> CompletedSignal()
    {
        var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        signal.TrySetResult(true);
        return signal;
    }
}