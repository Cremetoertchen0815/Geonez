using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Decorators;

/// <summary>
///     will keep executing its child task until the child task returns success
/// </summary>
public class UntilSuccess<T> : Decorator<T>
{
    public override TaskStatus Update(T context)
    {
        Insist.IsNotNull(Child, "child must not be null");

        var status = Child!.Tick(context);

        return status != TaskStatus.Success ? TaskStatus.Running : TaskStatus.Success;
    }
}