using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Decorators;

/// <summary>
///     will keep executing its child task until the child task returns failure
/// </summary>
public class UntilFail<T> : Decorator<T>
{
    public override TaskStatus Update(T context)
    {
        Insist.IsNotNull(Child, "child must not be null");

        var status = Child!.Update(context);

        return status != TaskStatus.Failure ? TaskStatus.Running : TaskStatus.Success;
    }
}