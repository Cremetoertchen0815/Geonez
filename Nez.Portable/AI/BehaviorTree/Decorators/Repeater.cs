using JetBrains.Annotations;
using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Decorators;

/// <summary>
///     will repeat execution of its child task until the child task has been run a specified number of times. It has the
///     option of
///     continuing to execute the child task even if the child task returns a failure.
/// </summary>
[PublicAPI]
public class Repeater<T>(int count, bool endOnFailure = false) : Decorator<T>
{
    private int _iterationCount;

    /// <summary>
    ///     The number of times to repeat the execution of its child task
    /// </summary>
    public int Count = count;

    /// <summary>
    ///     Should the task return if the child task returns a failure
    /// </summary>
    public bool EndOnFailure = endOnFailure;

    /// <summary>
    ///     Allows the repeater to repeat forever
    /// </summary>
    public bool RepeatForever;


    public Repeater(bool repeatForever, bool endOnFailure = false) : this(0, endOnFailure)
    {
        RepeatForever = repeatForever;
    }


    public override void OnStart()
    {
        _iterationCount = 0;
    }


    public override TaskStatus Update(T context)
    {
        Insist.IsNotNull(Child, "child must not be null");

        // early out if we are done. we check here and after running just in case the count is 0
        if (!RepeatForever && _iterationCount == Count)
            return TaskStatus.Success;

        var status = Child!.Tick(context);
        _iterationCount++;

        if (EndOnFailure && status == TaskStatus.Failure || !RepeatForever && _iterationCount == Count)
            return TaskStatus.Success;

        return TaskStatus.Running;
    }
}