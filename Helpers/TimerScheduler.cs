using System;
using System.Collections.Generic;

namespace TabApp;

public class TimerScheduler : IDisposable
{
    System.Timers.Timer _timer;
    IEnumerator<TimerSchedule> _schedules;
    int _currentTryCount;
    string? _currentId = string.Empty;

    public delegate void TimerElapsedEventHandler(object? sender, TimerElapsedEventArgs e);
    public event TimerElapsedEventHandler? Elapsed;
    public bool Enabled { get => _timer.Enabled; }

    /// <summary>
    /// Initialize a new instance of <see cref="DynamicTimer"/>
    /// </summary>
    /// <param name="schedules">Collection of <see cref="TimerSchedule"/> to follow. 
    /// The timer will iterate to next schedule if the current schedule has completed. 
    /// </param>
    public TimerScheduler(IEnumerable<TimerSchedule> schedules)
    {
        _timer = new System.Timers.Timer();
        _schedules = schedules.GetEnumerator();
        _currentTryCount = 0;
    }

    public void Start()
    {
        if (_schedules.MoveNext())
        {
            TimerSchedule schedule = _schedules.Current;
            _timer.Interval = schedule.Interval.TotalMilliseconds;
            _currentTryCount = schedule.TryCount;
            _currentId = schedule.Id;
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }
    }

    void OnElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    { 
        DateTime? nextTrigger = null;
        if (_currentTryCount > 0)
        {
            if (_currentTryCount == 1)
            {
                if (_schedules.MoveNext())
                {
                    // We'll only get here when there are multiple schedules.
                    TimerSchedule schedule = _schedules.Current;
                    _currentId = schedule.Id;
                    _timer.Interval = schedule.Interval.TotalMilliseconds;
                    _currentTryCount = schedule.TryCount;
                    nextTrigger = DateTime.Now + TimeSpan.FromMilliseconds(_timer.Interval);
                }
                else
                {
                    nextTrigger = null;
                    _timer.Stop();
                }
            }
            else
            {
                _currentTryCount--;
                nextTrigger = DateTime.Now + TimeSpan.FromMilliseconds(_timer.Interval);
            }
        }
        Elapsed?.Invoke(this, new TimerElapsedEventArgs(e.SignalTime, nextTrigger, _currentId));
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}

#region [Supporting Classes]
public class TimerElapsedEventArgs : EventArgs
{
    /// <summary>
    /// The signal identifier.
    /// </summary>
    public string? SignalId { get; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> when the <see cref="TimerScheduler.Elapsed"/> event was triggered.
    /// </summary>
    public DateTime SignalTime { get; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> when the next <see cref="TimerScheduler.Elapsed"/> event will be triggered (if any).
    /// Null means this is the last <see cref="TimerScheduler.Elapsed"/> event triggered. 
    /// </summary>
    public DateTime? NextSignalTime { get; }

    public TimerElapsedEventArgs(DateTime signalTime, DateTime? nextSignalTime, string? id)
    {
        SignalTime = signalTime;
        NextSignalTime = nextSignalTime;
        SignalId = id ?? "NULL";
    }
}

public class TimerSchedule
{
    /// <summary>
    /// The identifier of the schedule.
    /// </summary>
    public string? Id { get; private set; }

    /// <summary>
    /// The number of times to trigger Elapsed event. The timer will stop after the specified number of triggered Elapsed event.
    /// </summary>
    /// <remarks>
    /// Must be greater or equal to zero. If it is zero, the timer will continue indefinitely.
    /// </remarks>
    public int TryCount { get; private set; }

    /// <summary>
    /// The timer interval of the Elapsed event.
    /// </summary>
    public TimeSpan Interval { get; private set; }

    /// <summary>
    /// Create a <see cref="TimerScheduler"/>'s schedule. 
    /// The <see cref="TimerScheduler"/> will fire Elapsed event accoding to this schedule
    /// </summary>
    /// <param name="interval">Speicify the interval of the timer</param>
    /// <param name="tryCount">The number of times to trigger Elapsed</param>
    /// <param name="id">The schedule identifier</param>
    /// <remarks>
    /// For example, if <paramref name="interval"/> is 1 seconds and <paramref name="tryCount"/> is 2.
    /// If we call <see cref="TimerScheduler.Start"/> at T+0s, the Elapsed event will be trigger at T+1s and T+2s, then the timer will stop.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TimerSchedule(TimeSpan interval, int tryCount, string? id)
    {
        if (tryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(tryCount), "must be greater or equal to zero");

        Interval = interval;
        TryCount = tryCount;
        Id = id ?? "NULL";
    }
}
#endregion