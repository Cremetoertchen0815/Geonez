namespace Nez.AI.BehaviorTree.Actions;

/// <summary>
/// Runs an entire BehaviorTree as a child and returns success
/// </summary>
public class BehaviorTreeReference<T>(BehaviorTree<T> tree) : Behavior<T>
{
    public override TaskStatus Update(T context)
    {
        tree.Tick();
        return TaskStatus.Success;
    }
}