using System;
using System.Collections.Generic;

namespace Nez.Timers;

/// <summary>
///     allows delayed and repeated execution of an Action
/// </summary>
public class TimerManager : GlobalManager
{
	/// <summary>
	///     if true, the timer list will be cleared when a new level loads
	/// </summary>
	public static bool RemoveAllTimersOnLevelLoad = false;

    private readonly List<Timer> _timers = new();
    private Action _stalledMethods;

    public override void Update()
    {
        for (var i = _timers.Count - 1; i >= 0; i--)
            // tick our timer. if it returns true it is done so we remove it
            if (_timers[i].Tick())
            {
                _timers[i].Unload();
                _timers.RemoveAt(i);
            }

        // execute stalled methods
        var stalled = _stalledMethods;
        _stalledMethods = null;
        stalled?.Invoke();
    }

    /// <summary>
    ///     schedules a one-time or repeating timer that will call the passed in Action
    /// </summary>
    /// <param name="timeInSeconds">Time in seconds.</param>
    /// <param name="repeats">If set to <c>true</c> repeats.</param>
    /// <param name="context">Context.</param>
    /// <param name="onTime">On time.</param>
    internal ITimer Schedule(float timeInSeconds, bool repeats, object context, Action<ITimer> onTime)
    {
        var timer = new Timer();
        timer.Initialize(timeInSeconds, repeats, context, onTime);
        _timers.Add(timer);

        return timer;
    }

    /// <summary>
    ///     Schedules a task to be excecuted at the beginning of the next update cycle.
    /// </summary>
    /// <param name="action"></param>
    internal void Stall(Action action)
    {
        _stalledMethods += action;
    }

    public void Clear()
    {
        _timers.Clear();
        _stalledMethods = null;
    }
}