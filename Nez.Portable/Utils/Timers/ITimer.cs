namespace Nez;

public interface ITimer : ICancellableTimer
{
    object Context { get; }

    void ICancellableTimer.Cancel(bool completeFinalAction)
    {
        if (completeFinalAction) FinishNow();
        else Abort();
    }


    /// <summary>
    ///     call stop to stop this timer from being run again. This has no effect on a non-repeating timer.
    /// </summary>
    void Abort();

    /// <summary>
    ///     call stop to stop this timer from being run again. This has no effect on a non-repeating timer.
    /// </summary>
    void FinishNow();

    /// <summary>
    ///     resets the elapsed time of the timer to 0
    /// </summary>
    void Reset();

    /// <summary>
    ///     returns the context casted to T as a convenience
    /// </summary>
    /// <returns>The context.</returns>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    T GetContext<T>();
}