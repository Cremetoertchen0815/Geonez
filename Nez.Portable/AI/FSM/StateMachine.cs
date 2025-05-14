using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nez.Debugging;

namespace Nez.AI.FSM;

[PublicAPI]
public class StateMachine<T>
{
    private readonly Dictionary<Type, State<T>> _states = new();
    protected T Context;

    public State<T>? CurrentState { get; protected set; }
    public float ElapsedTimeInState;

    public State<T>? PreviousState;


    public StateMachine(T context, State<T> initialState)
    {
        Context = context;

        // setup our initial state
        AddState(initialState);
        CurrentState = initialState;
        CurrentState.Begin();
    }

    public event Action? OnStateChanged;


    /// <summary>
    ///     adds the state to the machine
    /// </summary>
    public void AddState(State<T> state)
    {
        state.SetMachineAndContext(this, Context);
        _states[state.GetType()] = state;
    }


    /// <summary>
    ///     ticks the state machine with the provided delta time
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        ElapsedTimeInState += deltaTime;
        CurrentState?.Reason();
        CurrentState?.Update(deltaTime);
    }

    /// <summary>
    ///     Gets a specific state from the machine without having to
    ///     change to it.
    /// </summary>
    public virtual TR GetState<TR>() where TR : State<T>
    {
        var type = typeof(TR);
        Insist.IsTrue(_states.ContainsKey(type),
            "{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), type);

        return (TR)_states[type];
    }


    /// <summary>
    ///     changes the current state
    /// </summary>
    public TR? ChangeState<TR>() where TR : State<T>
    {
        // avoid changing to the same state
        var newType = typeof(TR);
        if (CurrentState is TR rstate)
            return rstate;

        // only call end if we have a currentState
        CurrentState?.End();

        Insist.IsTrue(_states.ContainsKey(newType),
            "{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), newType);

        // swap states and call begin
        ElapsedTimeInState = 0f;
        PreviousState = CurrentState;
        CurrentState = _states[newType];
        CurrentState.Begin();

        // fire the changed event if we have a listener
        OnStateChanged?.Invoke();

        return CurrentState as TR;
    }
}