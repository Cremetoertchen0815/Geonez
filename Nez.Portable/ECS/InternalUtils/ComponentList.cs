﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nez;

public class ComponentList
{
    // global updateOrder sort for the IUpdatable list
    private static readonly IUpdatableComparer compareUpdatableOrder = new();

    /// <summary>
    ///     list of components added to the entity
    /// </summary>
    private readonly FastList<Component> _components = new();

    /// <summary>
    ///     The list of components that were marked for removal this frame. Used to group the components so we can process them
    ///     simultaneously
    /// </summary>
    private readonly List<Component> _componentsToRemove = new();

    private readonly Entity _entity;
    private readonly List<Component> _tempBufferList = new();

    /// <summary>
    ///     list of all Components that want update called
    /// </summary>
    private readonly FastList<IUpdatable> _updatableComponents = new();

    /// <summary>
    ///     The list of components that were added this frame. Used to group the components so we can process them
    ///     simultaneously
    /// </summary>
    internal List<Component> _componentsToAdd = new();

    /// <summary>
    ///     flag used to determine if we need to sort our Components this frame
    /// </summary>
    private bool _isComponentListUnsorted;

    public ComponentList(Entity entity)
    {
        _entity = entity;
    }


    public void MarkEntityListUnsorted()
    {
        _isComponentListUnsorted = true;
    }

    public FastList<Component> GetBase()
    {
        return _components;
    }

    public void Add(Component component)
    {
        _componentsToAdd.Add(component);
    }

    public void Remove(Component component)
    {
        Debug.WarnIf(_componentsToRemove.Contains(component),
            "You are trying to remove a Component ({0}) that you already removed", component);

        // this may not be a live Component so we have to watch out for if it hasnt been processed yet but it is being removed in the same frame
        if (_componentsToAdd.Contains(component))
        {
            _componentsToAdd.Remove(component);
            return;
        }

        component.isGettingRemoved = true;
        _componentsToRemove.Add(component);
    }

    /// <summary>
    ///     removes all components from the component list immediately
    /// </summary>
    public void RemoveAllComponents()
    {
        for (var i = 0; i < _components.Length; i++)
            HandleRemove(_components.Buffer[i]);

        _components.Clear();
        _updatableComponents.Clear();
        _componentsToAdd.Clear();
        _componentsToRemove.Clear();
    }

    internal void DeregisterAllComponents()
    {
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];

            // deal with renderLayer list if necessary
            if (component is RenderableComponent)
                _entity.Scene.RenderableComponents.Remove(component as RenderableComponent);

            // deal with IUpdatable
            if (component is IUpdatable)
                _updatableComponents.Remove(component as IUpdatable);
        }
    }

    internal void RegisterAllComponents()
    {
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];
            if (component is RenderableComponent)
                _entity.Scene.RenderableComponents.Add(component as RenderableComponent);

            if (component is IUpdatable)
                _updatableComponents.Add(component as IUpdatable);
        }
    }


    /// <summary>
    ///     handles any Components that need to be removed or added
    /// </summary>
    private void UpdateLists()
    {
        // handle removals
        if (_componentsToRemove.Count > 0)
        {
            for (var i = 0; i < _componentsToRemove.Count; i++)
            {
                HandleRemove(_componentsToRemove[i]);
                _components.Remove(_componentsToRemove[i]);
            }

            _componentsToRemove.Clear();
        }

        // handle additions
        if (_componentsToAdd.Count > 0)
        {
            for (int i = 0, count = _componentsToAdd.Count; i < count; i++)
            {
                var component = _componentsToAdd[i];
                if (component is RenderableComponent)
                    _entity.Scene.RenderableComponents.Add(component as RenderableComponent);

                if (component is IUpdatable)
                    _updatableComponents.Add(component as IUpdatable);

                component.Entity =
                    _entity; //Prevents Entity from being null, when you immediately remove and readd a component
                _components.Add(component);
                _tempBufferList.Add(component);
            }

            // clear before calling onAddedToEntity in case more Components are added then
            _componentsToAdd.Clear();
            _isComponentListUnsorted = true;

            // now that all components are added to the scene, we loop through again and call onAddedToEntity/onEnabled
            for (var i = 0; i < _tempBufferList.Count; i++)
            {
                var component = _tempBufferList[i];
                component.OnAddedToEntity();

                // component.enabled checks both the Entity and the Component
                if (component.Enabled)
                    component.OnEnabled();
            }

            _tempBufferList.Clear();
        }

        if (_isComponentListUnsorted)
        {
            _updatableComponents.Sort(compareUpdatableOrder);
            _isComponentListUnsorted = false;
        }
    }

    private void HandleRemove(Component component)
    {
        // deal with renderLayer list if necessary
        if (component is RenderableComponent)
            _entity.Scene.RenderableComponents.Remove(component as RenderableComponent);

        // deal with IUpdatable
        if (component is IUpdatable)
            _updatableComponents.Remove(component as IUpdatable);

        component.OnRemovedFromEntity();
        if (component.Entity == _entity) component.Entity = null;
        component.isGettingRemoved = false;
    }

    /// <summary>
    ///     Gets the first component of type T and returns it. Optionally skips checking un-initialized Components (Components
    ///     who have not yet had their
    ///     onAddedToEntity method called). If no components are found returns null.
    /// </summary>
    /// <returns>The component.</returns>
    /// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetComponent<T>(bool onlyReturnInitializedComponents) where T : Component
    {
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];
            if (component is T)
                return component as T;
        }

        // we optionally check the pending components just in case addComponent and getComponent are called in the same frame
        if (!onlyReturnInitializedComponents)
            for (var i = 0; i < _componentsToAdd.Count; i++)
            {
                var component = _componentsToAdd[i];
                if (component is T)
                    return component as T;
            }

        return null;
    }

    public T GetComponent<T>(string name, bool onlyReturnInitializedComponents) where T : Component
    {
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];
            if (component is T && component.Name == name)
                return component as T;
        }

        // we optionally check the pending components just in case addComponent and getComponent are called in the same frame
        if (!onlyReturnInitializedComponents)
            for (var i = 0; i < _componentsToAdd.Count; i++)
            {
                var component = _componentsToAdd[i];
                if (component is T)
                    return component as T;
            }

        return null;
    }

    /// <summary>
    ///     Gets all the components of type T without a List allocation
    /// </summary>
    /// <param name="components">Components.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetComponents<T>(List<T> components) where T : class
    {
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];
            if (component is T)
                components.Add(component as T);
        }

        // we also check the pending components just in case addComponent and getComponent are called in the same frame
        for (var i = 0; i < _componentsToAdd.Count; i++)
        {
            var component = _componentsToAdd[i];
            if (component is T)
                components.Add(component as T);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> GetComponents<T>(string name) where T : Component
    {
        var components = new List<T>();
        for (var i = 0; i < _components.Length; i++)
        {
            var component = _components.Buffer[i];
            if (component is T && component.Name == name)
                components.Add(component as T);
        }

        // we also check the pending components just in case addComponent and getComponent are called in the same frame
        for (var i = 0; i < _componentsToAdd.Count; i++)
        {
            var component = _componentsToAdd[i];
            if (component is T && component.Name == name)
                components.Add(component as T);
        }

        return components;
    }

    /// <summary>
    ///     Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
    /// </summary>
    /// <returns>The components.</returns>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<T> GetComponents<T>() where T : class
    {
        var components = ListPool<T>.Obtain();
        GetComponents(components);

        return components;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Update()
    {
        UpdateLists();
        for (var i = 0; i < _updatableComponents.Length; i++)
        {
#if TRACE
            var timer = DeltaAnalyzer.MeasureSegment(_entity.Name, _updatableComponents.Buffer[i].GetType().Name,
                DeltaAnalyzer.DeltaSegmentType.Update);
            if (_updatableComponents.Buffer[i].Enabled && (_updatableComponents.Buffer[i] as Component).Enabled)
                _updatableComponents.Buffer[i].Update();
            timer.Stop();
#else
				if (_updatableComponents.Buffer[i].Enabled && (_updatableComponents.Buffer[i] as Component).Enabled)
					_updatableComponents.Buffer[i].Update();
#endif
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OnEntityTransformChanged(Transform.Component comp)
    {
        for (var i = 0; i < _components.Length; i++)
            if (_components.Buffer[i].Enabled)
                _components.Buffer[i].OnEntityTransformChanged(comp);

        for (var i = 0; i < _componentsToAdd.Count; i++)
            if (_componentsToAdd[i].Enabled)
                _componentsToAdd[i].OnEntityTransformChanged(comp);
    }

    internal void OnEntityEnabled()
    {
        for (var i = 0; i < _components.Length; i++)
            _components.Buffer[i].OnEnabled();
    }

    internal void OnEntityDisabled()
    {
        for (var i = 0; i < _components.Length; i++)
            _components.Buffer[i].OnDisabled();
    }

    internal void DebugRender(Batcher batcher)
    {
        for (var i = 0; i < _components.Length; i++)
            if (_components.Buffer[i].Enabled)
                _components.Buffer[i].DebugRender(batcher);
    }


    #region array access

    public int Count => _components.Length;

    public Component this[int index] => _components.Buffer[index];

    public Component[] GetArray()
    {
        return _components.Buffer;
    }

    #endregion
}