﻿#region LICENSE

//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------

#endregion

#region File Description

//-----------------------------------------------------------------------------
// Base class for nodes capable of culling based on different algorithms.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;

namespace Nez.GeonBit;

/// <summary>
///     A base culling node class for nodes capable of culling optimization.
/// </summary>
public abstract class CullingNode : Node
{
    /// <summary>
    ///     The camera frustum to cull by. You need to update this every time the camera frustum changes in order
    ///     to make the culling work currectly.
    /// </summary>
    public static BoundingFrustum CurrentCameraFrustum = null;

    /// <summary>
    ///     Do we need to update whatever culling method we use?
    ///     This is set to true when node transformations or one of its children updates.
    /// </summary>
    protected bool _isCullingDirty = true;

    /// <summary>
    ///     Get current camera frustum.
    /// </summary>
    protected BoundingFrustum CameraFrustum => CurrentCameraFrustum;

    /// <summary>
    ///     Return if we should cull this node in current frame.
    /// </summary>
    public virtual bool ShouldCull => !IsInScreen;

    /// <summary>
    ///     Get if this node is currently visible in camera.
    /// </summary>
    public abstract bool IsInScreen { get; }

    /// <summary>
    ///     Get if this node is partly inside screen (eg intersects with camera frustum).
    /// </summary>
    public abstract bool IsPartlyInScreen { get; }

    /// <summary>
    ///     Draw the node and its children.
    /// </summary>
    /// <param name="forceEvenIfAlreadyDrawn">If true, will draw this node even if it was already drawn in current frame.</param>
    protected override void DrawSpecific(bool forceEvenIfAlreadyDrawn = false)
    {
        // if camera frustum is not defined, draw this node without culling
        if (DisableCulling || CameraFrustum == null)
        {
            base.DrawSpecific();
            return;
        }

        // update transformations (only if needed, testing logic is inside)
        DoTransformationsUpdateIfNeeded();

        // if need to update culling data (node or one of its children transformed), call the update culling method.
        if (_isCullingDirty)
        {
            // update culling and mark culling no longer dirty
            UpdateCullingData();
            _isCullingDirty = false;
        }

        // draw all child nodes
        foreach (var node in _childNodes) node.Draw();

        // call draw callback
        __OnNodeDraw?.Invoke(this);

        // draw all child entities
        foreach (var entity in _childEntities) entity.Draw(this, ref _localTransform, ref _worldTransform);
    }

    /// <summary>
    ///     Update culling test / cached data.
    ///     This is called whenever trying to draw this node after transformations update
    /// </summary>
    protected abstract void UpdateCullingData();

    /// <summary>
    ///     Called every time one of the child nodes recalculate world transformations.
    /// </summary>
    /// <param name="node">The child node that updated.</param>
    public override void OnChildWorldMatrixChange(Node node)
    {
        base.OnChildWorldMatrixChange(node);
        _isCullingDirty = true;
    }

    /// <summary>
    ///     Called when the world matrix of this node is actually recalculated (invoked after the calculation).
    /// </summary>
    protected override void OnWorldMatrixChange()
    {
        // call base function
        base.OnWorldMatrixChange();

        // set culling to dirty
        _isCullingDirty = true;
    }

    /// <summary>
    ///     Called every time an entity was added / removed from this node.
    /// </summary>
    /// <param name="entity">Entity that was added / removed.</param>
    /// <param name="wasAdded">If true its an entity that was added, if false, an entity that was removed.</param>
    protected override void OnEntitiesListChange(IEntity entity, bool wasAdded)
    {
        _isCullingDirty = true;
    }

    /// <summary>
    ///     Called whenever an entity was added / removed from this node.
    /// </summary>
    /// <param name="node">GeonNode that was added / removed.</param>
    /// <param name="wasAdded">If true its a node that was added, if false, a node that was removed.</param>
    protected override void OnChildNodesListChange(Node node, bool wasAdded)
    {
        _isCullingDirty = true;
    }
}