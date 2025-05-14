using System.Collections.Generic;
using JetBrains.Annotations;
using Nez.Debug;

namespace Nez.AI.GOAP;

/// <summary>
///     Agent provides a simple and concise way to use the planner. It is not necessary to use at all since it is just a
///     convenince wrapper
///     around the ActionPlanner making it easier to get plans and store the results.
/// </summary>
[PublicAPI]
public abstract class Agent
{
    protected readonly ActionPlanner Planner = new();
    public Stack<Action>? Actions;


    public bool Plan(bool debugPlan = false)
    {
        List<AStarNode>? nodes = null;
        if (debugPlan)
            nodes = [];

        Actions = Planner.Plan(GetWorldState(), GetGoalState(), nodes);

        if (nodes is not { Count: > 0 }) return HasActionPlan();
        
        DebugHelpers.Log("---- ActionPlanner plan ----");
        DebugHelpers.Log("plan cost = {0}\n", nodes[^1].CostSoFar);
        DebugHelpers.Log("{0}\t{1}", "start".PadRight(15), GetWorldState().Describe(Planner));
        for (var i = 0; i < nodes.Count; i++)
        {
            DebugHelpers.Log("{0}: {1}\t{2}", i, nodes[i].Action.GetType().Name.PadRight(15),
                nodes[i].WorldState.Describe(Planner));
            Pool<AStarNode>.Free(nodes[i]);
        }

        return HasActionPlan();
    }


    public bool HasActionPlan()
    {
        return Actions is { Count: > 0 };
    }


    /// <summary>
    ///     current WorldState
    /// </summary>
    /// <returns>The world state.</returns>
    public abstract WorldState GetWorldState();


    /// <summary>
    ///     the goal state that the agent wants to achieve
    /// </summary>
    /// <returns>The goal state.</returns>
    public abstract WorldState GetGoalState();
}