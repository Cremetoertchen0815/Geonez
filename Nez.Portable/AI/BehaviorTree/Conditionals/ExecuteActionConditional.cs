using System;
using Nez.AI.BehaviorTree.Actions;

namespace Nez.AI.BehaviorTree.Conditionals;

/// <summary>
///     wraps an ExecuteAction so that it can be used as a Conditional
/// </summary>
public class ExecuteActionConditional<T>(Func<T, TaskStatus> action) : ExecuteAction<T>(action), IConditional<T>;