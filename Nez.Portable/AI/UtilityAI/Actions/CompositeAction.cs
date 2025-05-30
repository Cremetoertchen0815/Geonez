﻿using System.Collections.Generic;

namespace Nez.AI.UtilityAI;

/// <summary>
///     Action that contains a List of Actions that it will execute sequentially
/// </summary>
public class CompositeAction<T> : IAction<T>
{
    private readonly List<IAction<T>> _actions = new();


    void IAction<T>.Execute(T context)
    {
        for (var i = 0; i < _actions.Count; i++)
            _actions[i].Execute(context);
    }


    public CompositeAction<T> AddAction(IAction<T> action)
    {
        _actions.Add(action);
        return this;
    }
}