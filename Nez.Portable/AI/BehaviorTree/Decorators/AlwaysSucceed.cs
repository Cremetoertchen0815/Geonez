using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Decorators;

/// <summary>
///     will always return success except when the child task is running
/// </summary>
public class AlwaysSucceed<T> : Decorator<T>
{
    public override TaskStatus Update(T context)
    {
        Insist.IsNotNull(Child, "child must not be null");

        var status = Child!.Update(context);

        return status == TaskStatus.Running ? TaskStatus.Running : TaskStatus.Success;
    }
}