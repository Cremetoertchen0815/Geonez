namespace Nez.AI.BehaviorTree.Actions;

/// <summary>
///     Wait a specified amount of time. The task will return running until the task is done waiting. It will return
///     success after the wait
///     time has elapsed.
/// </summary>
public class WaitAction<T>(float waitTime) : Behavior<T>
{
    private float _startTime;

    /// <summary>
    /// The amount of time to wait
    /// </summary>
    public float WaitTime = waitTime;


    public override void OnStart()
    {
        _startTime = 0;
    }


    public override TaskStatus Update(T context)
    {
        // We cant use Time.deltaTime due to the tree ticking at its own rate, so we store the start time instead.
        if (_startTime == 0)
            _startTime = Time.TotalTime;

        return Time.TotalTime - _startTime >= WaitTime ? TaskStatus.Success : TaskStatus.Running;
    }
}