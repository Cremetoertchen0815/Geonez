using Nez.AI.BehaviorTree.Conditionals;
using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Decorators;

/// <summary>
///     decorator that will only run its child if a condition is met. By default, the condition will be reevaluated every
///     tick.
/// </summary>
public class ConditionalDecorator<T>(IConditional<T> conditional, bool shouldReevalute) : Decorator<T>, IConditional<T>
{
    private TaskStatus _conditionalStatus;


    public override TaskStatus Update(T context)
    {
        Insist.IsNotNull(Child, "child must not be null");

        // evalute the condition if we need to
        _conditionalStatus = ExecuteConditional(context);

        return _conditionalStatus == TaskStatus.Success ? Child!.Tick(context) : TaskStatus.Failure;
    }


    public override void Invalidate()
    {
        base.Invalidate();
        _conditionalStatus = TaskStatus.Invalid;
    }


    public override void OnStart()
    {
        _conditionalStatus = TaskStatus.Invalid;
    }


    /// <summary>
    ///     executes the conditional either following the shouldReevaluate flag or with an option to force an update. Aborts
    ///     will force the
    ///     update to make sure they get the proper data if a Conditional changes.
    /// </summary>
    /// <returns>The conditional.</returns>
    /// <param name="context">Context.</param>
    /// <param name="forceUpdate">If set to <c>true</c> force update.</param>
    internal TaskStatus ExecuteConditional(T context, bool forceUpdate = false)
    {
        if (forceUpdate || shouldReevalute || _conditionalStatus == TaskStatus.Invalid)
            _conditionalStatus = conditional.Update(context);
        return _conditionalStatus;
    }
}