namespace Nez.AI.BehaviorTree.Conditionals;

/// <summary>
///     returns success when the random probability is above the successProbability probability. It will otherwise return
///     failure.
///     successProbability should be between 0 and 1.
/// </summary>
public class RandomProbability<T>(int successProbability) : Behavior<T>, IConditional<T>
{
    public override TaskStatus Update(T context)
    {
        return Random.NextFloat() > successProbability ? TaskStatus.Success : TaskStatus.Failure;
    }
}