using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TabApp;

public static class TaskTimer
{
    public static async Task<TaskTimerResult<TReturn>> Start<TReturn>(Func<Task<TReturn>> function)
    {
        var stopWatch = Stopwatch.StartNew();
        try
        {
            var task = function();
            await task;
            TimeSpan elapsed = stopWatch.Elapsed;
            return new TaskTimerResult<TReturn>(task, elapsed);
        }
        catch (Exception ex)
        {
            return new TaskTimerResult<TReturn>(Task.FromException<TReturn>(ex), stopWatch.Elapsed);
        }
    }

    public static async Task<TaskTimerResult> Start(Func<Task> function)
    {
        var stopWatch = Stopwatch.StartNew();
        try
        {
            var task = function();
            await task;
            TimeSpan elapsed = stopWatch.Elapsed;
            return new TaskTimerResult(task, elapsed);
        }
        catch (Exception ex)
        {
            return new TaskTimerResult(Task.FromException(ex), stopWatch.Elapsed);
        }
    }
}

public class TaskTimerResult
{
    protected readonly Task _task;
    private readonly TimeSpan _duration;
    public Task Task { get { return _task; } }
    public TimeSpan Duration { get { return _duration; } }

    public TaskTimerResult(Task task, TimeSpan duration)
    {
        _task = task;
        _duration = duration;
    }
}

public class TaskTimerResult<T> : TaskTimerResult
{
    public TaskTimerResult(Task<T> task, TimeSpan duration) : base(task, duration)
    {
    }

    /// <summary>
    /// NOTE: You should never attempt to access the result for a task that has not completed.
    /// </summary>
    public T Result { get { return ((Task<T>)_task).Result; } }
}
