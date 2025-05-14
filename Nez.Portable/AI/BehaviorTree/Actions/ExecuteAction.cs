using System;
using Nez.Debugging;

namespace Nez.AI.BehaviorTree.Actions;

/// <summary>
/// Wraps a Func so that you can avoid having to subclass to create new actions
/// </summary>
public class ExecuteAction<T>(Func<T, TaskStatus> action) : Behavior<T>
{
    public override TaskStatus Update(T context)
    {
        return action(context);
    }
}