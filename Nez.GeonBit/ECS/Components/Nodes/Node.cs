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
// A node is the basic container class in the scene graph. 
// Every node consists of 3d transformation, child nodes, and child entities to draw.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Nez.GeonBit;

/// <summary>
///     A callback function you can register on different node-related events.
/// </summary>
/// <param name="node">The node instance the event came from.</param>
public delegate void NodeEventCallback(Node node);

/// <summary>
///     A basic scene node with transformations.
///     You can attach renderable entities to it, or child nodes that will inherit its transformations.
/// </summary>
public class Node
{
    /// <summary>
    ///     Callback that triggers every time a node updates its matrix.
    /// </summary>
    internal static NodeEventCallback __OnNodeTransformationsUpdate;

    /// <summary>
    ///     Callback that triggers every time a node is rendered.
    ///     Note: nodes that are culled out should not trigger this.
    /// </summary>
    internal static NodeEventCallback __OnNodeDraw;

    /// <summary>
    ///     Const return value for empty bounding box.
    /// </summary>
    private static readonly BoundingBox EmptyBoundingBox = new();

    /// <summary>
    ///     Const return value for empty bounding sphere.
    /// </summary>
    private static readonly BoundingSphere EmptyBoundingSphere = new();

    /// <summary>
    ///     For how many rendering frames bounding box / bounding sphere hold in cache.
    /// </summary>
    public static byte BoundingShapesTtl = 5;

    // do we need to update bounding box?
    private bool _boundingBoxDirty = true;

    // do we need to update bounding sphere?
    private bool _boundingSphereDirty = true;

    /// <summary>
    ///     Child entities under this node.
    /// </summary>
    protected List<IEntity> _childEntities = new();

    /// <summary>
    ///     Child nodes under this node.
    /// </summary>
    protected List<Node> _childNodes = new();

    /// <summary>
    ///     Turns true when the transformations of this node changes.
    /// </summary>
    protected bool _isDirty = true;


    protected uint _lastNodeTransformVersion;

    /// <summary>
    ///     Special internal list used when we need to link this node to other nodes, like the case with octree.
    /// </summary>
    private List<Node> _linkedNodes;

    /// <summary>
    ///     Local transformations matrix, eg the result of the current local transformations.
    /// </summary>
    protected Matrix _localTransform = Matrix.Identity;

    /// <summary>
    ///     The last transformations version we got from our parent.
    /// </summary>
    protected uint _parentLastTransformVersion;

    /// <summary>
    ///     This number increment every time we update transformations.
    ///     We use it to check if our parent's transformations had been changed since last
    ///     time this node was rendered, and if so, we re-apply parent updated transformations.
    /// </summary>
    protected uint _transformVersion;

    /// <summary>
    ///     World transformations matrix, eg the result of the local transformations multiplied with parent transformations.
    /// </summary>
    protected Matrix _worldTransform = Matrix.Identity;

    /// <summary>
    ///     Is culling enabled for this scene node.
    ///     Note: while this is not used for based node types, its a generic property for inheriting nodes.
    /// </summary>
    public bool DisableCulling = false;

    /// <summary>
    ///     Optional identifier we can give to nodes.
    /// </summary>
    public string Identifier;

    /// <summary>
    ///     Bounding box caching.
    /// </summary>
    internal BoundingBox LastBoundingBox;

    /// <summary>
    ///     Bounding sphere caching.
    /// </summary>
    internal BoundingSphere LastBoundingSphere;

    /// <summary>
    ///     Optional user data we can attach to nodes.
    /// </summary>
    public object UserData;

    /// <summary>
    ///     Create the node.
    /// </summary>
    public Node()
    {
        // count the object creation
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.AddedOrCreated);
    }

    /// <summary>
    ///     Parent node.
    /// </summary>
    public Node Parent { get; internal set; }

    public Node Root => Entity?.Node ?? Parent;

    public GeonEntity Entity { get; internal set; }

    /// <summary>
    ///     Last frame this node was drawn.
    /// </summary>
    public uint LastDrawFrame { get; private set; }

    /// <summary>
    ///     Get if this node was drawn in current frame.
    /// </summary>
    public bool WasDrawnThisFrame => LastDrawFrame == NodesManager.CurrFrame;

    /// <summary>
    ///     Get the transformations instance directly.
    /// </summary>
    protected Transformations Transformations { get; set; } = new();

    /// <summary>
    ///     Is this node currently visible?
    /// </summary>
    public virtual bool Visible { get; set; } = true;

    /// <summary>
    ///     If true, it means transformations are coming from external source (like a physical body).
    ///     If you set it true, this node will no longer calculate transformations on its own, and you'll need to use
    ///     'SetWorldTransformsMatrix()' to update the world matrix.
    /// </summary>
    public bool UseExternalTransformations { get; set; }

    /// <summary>
    ///     Transformation version is a special identifier that changes whenever the world transformations
    ///     of this node changes. Its not necessarily a sequence, but if you check this number for changes every
    ///     frame its a good indication of transformation change.
    /// </summary>
    public uint TransformVersion => _transformVersion;

    /// <summary>
    ///     Add a link to another node.
    /// </summary>
    /// <param name="node"></param>
    internal void LinkToNode(Node node)
    {
        LinkedNodes.Add(node);
    }


    /// <summary>
    ///     Add an entity to this node.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    public void AddEntity(IEntity entity)
    {
        // make sure this node can hold entities
        if (!CanHoldEntities) throw new InvalidOperationException("Cannot add entities to this node type.");

        // add new entity to node
        _childEntities.Add(entity);
        OnEntitiesListChange(entity, true);
    }

    /// <summary>
    ///     Remove an entity from this node.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    public void RemoveEntity(IEntity entity)
    {
        _childEntities.Remove(entity);
        OnEntitiesListChange(entity, false);
    }

    /// <summary>
    ///     Called whenever a child node was added / removed from this node.
    /// </summary>
    /// <param name="entity">Entity that was added / removed.</param>
    /// <param name="wasAdded">If true its an entity that was added, if false, an entity that was removed.</param>
    protected virtual void OnEntitiesListChange(IEntity entity, bool wasAdded)
    {
    }

    /// <summary>
    ///     Called whenever an entity was added / removed from this node.
    /// </summary>
    /// <param name="node">GeonNode that was added / removed.</param>
    /// <param name="wasAdded">If true its a node that was added, if false, a node that was removed.</param>
    protected virtual void OnChildNodesListChange(Node node, bool wasAdded)
    {
    }

    /// <summary>
    ///     Add a child node to this node.
    /// </summary>
    /// <param name="node">GeonNode to add.</param>
    public Node AddChildNode(Node node)
    {
        // node already got a parent?
        if (node.Parent != null) throw new InvalidOperationException("Can't add a node that already have a parent.");

        // add node to children list
        _childNodes.Add(node);

        // set self as node's parent
        node.SetParent(this);
        OnChildNodesListChange(node, true);

        return node;
    }

    /// <summary>
    ///     Remove a child node from this node.
    /// </summary>
    /// <param name="node">GeonNode to add.</param>
    public void RemoveChildNode(Node node)
    {
        // if node is null skip
        if (node == null) return;

        // make sure the node is a child of this node
        if (node.Parent != this)
            throw new InvalidOperationException("Can't remove a node that don't belong to this parent.");

        // remove node from children list
        _childNodes.Remove(node);

        // clear node parent
        node.SetParent(null);
        OnChildNodesListChange(node, false);
    }

    /// <summary>
    ///     Find and return first child node by identifier.
    /// </summary>
    /// <param name="identifier">GeonNode identifier to search for.</param>
    /// <param name="searchInChildren">If true, will also search recurisvely in children.</param>
    /// <returns>GeonNode with given identifier or null if not found.</returns>
    public Node FindChildNode(string identifier, bool searchInChildren = true)
    {
        foreach (var node in _childNodes)
        {
            // search in direct children
            if (node.Identifier == identifier) return node;

            // recursive search
            if (searchInChildren)
            {
                var foundInChild = node.FindChildNode(identifier, searchInChildren);
                if (foundInChild != null) return foundInChild;
            }
        }

        // if got here it means we didn't find any child node with given identifier
        return null;
    }

    /// <summary>
    ///     Remove this node from its parent.
    /// </summary>
    public void RemoveFromParent()
    {
        // don't have a parent?
        if (Parent == null) throw new InvalidOperationException("Can't remove an orphan node from parent.");

        // remove from parent
        Parent.RemoveChildNode(this);
    }

    /// <summary>
    ///     Called when the world matrix of this node is actually recalculated (invoked after the calculation).
    /// </summary>
    protected virtual void OnWorldMatrixChange()
    {
        // count events
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.HeavyUpdate);

        // update transformations version
        _transformVersion++;

        // trigger update event
        __OnNodeTransformationsUpdate?.Invoke(this);

        // mark bounding-box and bounding-sphere as dirty
        _boundingBoxDirty = true;
        _boundingSphereDirty = true;

        // notify parent
        if (Parent != null) Parent.OnChildWorldMatrixChange(this);
    }

    /// <summary>
    ///     Called when local transformations are set, eg when Position, Rotation, Scale etc. is changed.
    ///     We use this to set this node as "dirty", eg that we need to update local transformations.
    /// </summary>
    protected virtual void OnTransformationsSet()
    {
        if (UseExternalTransformations) return;
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.ValueChanged);
        NodesManager.AddNodeToUpdateQueue(this);
        _isDirty = true;
    }

    /// <summary>
    ///     Set the parent of this node.
    /// </summary>
    /// <param name="newParent">New parent node to set, or null for no parent.</param>
    protected virtual void SetParent(Node newParent)
    {
        // count the event
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.ValueChanged);

        // set parent
        Parent = newParent;

        // set our parents last transformations version to make sure we'll update world transformations next frame.
        _parentLastTransformVersion = newParent != null ? newParent._transformVersion - 1 : 1;
    }


    /// <summary>
    ///     Force this node to update transformation and recalculate bounding box and sphere.
    /// </summary>
    /// <param name="updateNow">If true, will update right now. If false, will do the actual update next drawing frame.</param>
    public void ForceFullUpdate(bool updateNow = true)
    {
        // count the event
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.ForceUpdate);

        // mark everything as dirty
        _isDirty = true;
        _boundingBoxDirty = true;
        _boundingSphereDirty = true;

        // if update now, do transformation updates
        if (updateNow)
        {
            DoTransformationsUpdateIfNeeded();
            UpdateBoundingBox();
            UpdateBoundingSphere();
        }
    }

    /// <summary>
    ///     Calc final transformations for current frame.
    ///     This uses an indicator to know if an update is needed, so no harm is done if you call it multiple times.
    /// </summary>
    protected virtual void DoTransformationsUpdateIfNeeded()
    {
        // if use external transformations, skip
        if (UseExternalTransformations)
        {
            _isDirty = false;
            return;
        }

        // if local transformations are dirty, or parent transformations are out-of-date, update world transformations
        if (_isDirty || WasParentUpdate)
        {
            // if local transformations are dirty, we need to update local transforms
            if (_isDirty) _localTransform = Transformations.BuildMatrix();

            // if we got parent, apply its transformations
            if (Parent != null)
            {
                _worldTransform = _localTransform * Parent._worldTransform;
                _parentLastTransformVersion = Parent._transformVersion;
            }
            // if not, world transformations are the same as local, and reset parent last transformations version
            else
            {
                _worldTransform = _localTransform;
                _parentLastTransformVersion = 0;
            }

            // no longer dirty
            _isDirty = false;

            // called the function that mark world matrix change (increase transformation version etc)
            OnWorldMatrixChange();
        }
    }

    /// <summary>
    ///     Transform a given transformations and return the result matrix.
    /// </summary>
    /// <param name="trans">Transformations to transform.</param>
    /// <returns>Matrix with combined transformations.</returns>
    public Matrix TransformMatrix(Transformations trans)
    {
        // build matrix for given transformations
        var transMatrix = trans.BuildMatrix();

        // get our world transformations
        var worldMatrix = WorldTransformations;

        // count the event
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.ForceUpdate);

        // combine and return
        return transMatrix * worldMatrix;
    }


    /// <summary>
    ///     Set world transformations from external source.
    ///     To prevent the node from overriding these changes, set 'UseExternalTransformations' to true.
    /// </summary>
    /// <param name="world"></param>
    public void SetWorldTransforms(ref Matrix world)
    {
        if (_worldTransform == world) return;
        _worldTransform = world;
        OnWorldMatrixChange();
    }

    /// <summary>
    ///     Get a debug string representation of this scene node and its children.
    /// </summary>
    /// <returns>String representing the scene tree starting from this node.</returns>
    public string GetDebugString()
    {
        var stringBuilder = new StringBuilder();
        GetDebugString(ref stringBuilder);
        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Internal implementation of get debug string function.
    ///     This shows a textual representation of this node and its children.
    /// </summary>
    /// <param name="stringBuilder">String builder used to create the string.</param>
    /// <param name="depth">Current depth in scene graph.</param>
    /// <param name="prefix">Current prefix (should match depth level).</param>
    /// <returns>Debug string with data about this node.</returns>
    protected void GetDebugString(ref StringBuilder stringBuilder, int depth = 0, string prefix = "")
    {
        // add self
        stringBuilder.Append(prefix);
        GetDebugName(ref stringBuilder);
        stringBuilder.Append('\n');

        // add children
        var childPrefix = prefix + new string(' ', 3);
        foreach (var child in _childNodes) child.GetDebugString(ref stringBuilder, depth + 1, childPrefix);
    }

    /// <summary>
    ///     Get debug name representation of this node (used by GetDebugString).
    /// </summary>
    /// <param name="stringBuilder">String builder to add debug name to.</param>
    /// <returns>Debug name of this node.</returns>
    protected void GetDebugName(ref StringBuilder stringBuilder)
    {
        if (!Visible) stringBuilder.Append("X");
        if (_isDirty) stringBuilder.Append("d");
        stringBuilder.Append(" ");
        stringBuilder.Append(GetType().Name);
        stringBuilder.Append(" [").Append(Identifier).Append("]");
        stringBuilder.Append(" {").Append(_childEntities.Count).Append("}");
    }

    /// <summary>
    ///     Clone this scene node.
    /// </summary>
    /// <returns>GeonNode copy.</returns>
    public virtual Node Clone()
    {
        var ret = new Node
        {
            Transformations = Transformations.Clone(),
            Visible = Visible,
            UseExternalTransformations = UseExternalTransformations
        };
        return ret;
    }

    /// <summary>
    ///     Build matrix from node transformations.
    /// </summary>
    public Matrix BuildTransformationsMatrix()
    {
        return Transformations.BuildMatrix();
    }

    /// <summary>
    ///     Return if should draw this node at this frame (test basic stuff like was already drawn, is visible, etc.
    /// </summary>
    /// <param name="forceEvenIfAlreadyDrawn">If true, will draw this node even if it was already drawn in current frame.</param>
    /// <returns>If should draw at this frame.</returns>
    protected bool ShouldDraw(bool forceEvenIfAlreadyDrawn = false)
    {
        return Visible && (forceEvenIfAlreadyDrawn || !WasDrawnThisFrame);
    }

    /// <summary>
    ///     Draw the node and its children.
    /// </summary>
    /// <param name="forceEvenIfAlreadyDrawn">If true, will draw this node even if it was already drawn in current frame.</param>
    public void Draw(bool forceEvenIfAlreadyDrawn = false, bool forceDraw = false)
    {
        // check if we shouldn't draw at this frame
        if (!ShouldDraw(forceEvenIfAlreadyDrawn) && !forceDraw) return;

        // draw node
        DrawSpecific(forceEvenIfAlreadyDrawn);

        // set last drawn frame
        LastDrawFrame = NodesManager.CurrFrame;
    }

    /// <summary>
    ///     Draw the node and its children.
    /// </summary>
    /// <param name="forceEvenIfAlreadyDrawn">If true, will draw this node even if it was already drawn in current frame.</param>
    protected virtual void DrawSpecific(bool forceEvenIfAlreadyDrawn = false)
    {
        // update transformations (only if needed, testing logic is inside)
        DoTransformationsUpdateIfNeeded();

        // draw all child nodes
        for (var i = 0; i < _childNodes.Count; i++)
        {
            var node = _childNodes[i];
            node.Draw(forceEvenIfAlreadyDrawn);
        }

        // trigger draw event
        __OnNodeDraw?.Invoke(this);

        // draw all child entities
        for (var i = 0; i < _childEntities.Count; i++)
        {
            var entity = _childEntities[i];
            entity.Draw(this, ref _localTransform, ref _worldTransform);
        }
    }

    /// <summary>
    ///     Update transformations for this node and its children, if needed.
    /// </summary>
    /// <param name="recursive">If true, will also iterate and force-update children.</param>
    public void UpdateTransformations(bool recursive)
    {
        // not visible? skip
        if (!Visible) return;

        // update transformations (only if needed, testing logic is inside)
        DoTransformationsUpdateIfNeeded();

        // force-update all child nodes
        if (recursive)
            foreach (var node in _childNodes)
                node.UpdateTransformations(recursive);
    }

    /// <summary>
    ///     Reset all local transformations.
    /// </summary>
    public void ResetTransformations()
    {
        Transformations = new Transformations();
        OnTransformationsSet();
    }

    /// <summary>
    ///     Move position by vector.
    /// </summary>
    /// <param name="moveBy">Vector to translate by.</param>
    public void Translate(Vector3 moveBy)
    {
        Transformations.Position += moveBy;
        OnTransformationsSet();
    }

    /// <summary>
    ///     Called every time one of the child nodes recalculate world transformations.
    /// </summary>
    /// <param name="node">The child node that updated.</param>
    public virtual void OnChildWorldMatrixChange(Node node)
    {
        // mark bounding-box and bounding-sphere as dirty
        _boundingBoxDirty = true;
        _boundingSphereDirty = true;

        // update parent as well
        if (Parent != null) Parent.OnChildWorldMatrixChange(node);
    }

    /// <summary>
    ///     Get up-to-date bounding box of this node and all its child nodes, and recalculate it if needed.
    /// </summary>
    /// <returns>Bounding box of the node and its children.</returns>
    public virtual BoundingBox GetBoundingBox()
    {
        // if need to update bounding box, update and return
        if (_boundingBoxDirty || WasParentUpdate) return UpdateBoundingBox();

        // if no need to update just return last box calculated
        return LastBoundingBox;
    }

    /// <summary>
    ///     Recalculate bounding box of this node and all its child nodes.
    /// </summary>
    /// <returns>Bounding box of the node and its children.</returns>
    public virtual BoundingBox UpdateBoundingBox()
    {
        // count event
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.HeavyUpdate);

        // if empty skip
        if (Empty)
        {
            LastBoundingBox = EmptyBoundingBox;
            _boundingBoxDirty = false;
            return EmptyBoundingBox;
        }

        // make sure transformations are up-to-date
        DoTransformationsUpdateIfNeeded();

        // list of points to build bounding box from
        var corners = new List<Vector3>();

        // apply all child nodes bounding boxes
        foreach (var child in _childNodes)
        {
            // skip invisible nodes
            if (!child.Visible) continue;

            // get bounding box
            var currBox = child.GetBoundingBox();
            if (currBox.Min != currBox.Max)
            {
                corners.Add(currBox.Min);
                corners.Add(currBox.Max);
            }
        }

        // apply all entities directly under this node
        foreach (var entity in _childEntities)
        {
            // skip invisible entities
            if (!entity.Visible) continue;

            // get entity bounding box
            var currBox = entity.GetBoundingBox(this, ref _localTransform, ref _worldTransform);
            if (currBox.Min != currBox.Max)
            {
                corners.Add(currBox.Min);
                corners.Add(currBox.Max);
            }
        }

        // nothing in this node?
        if (corners.Count == 0) return EmptyBoundingBox;

        // add to cache and return
        LastBoundingBox = BoundingBox.CreateFromPoints(corners);
        _boundingBoxDirty = false;
        return LastBoundingBox;
    }

    /// <summary>
    ///     Get up-to-date bounding sphere of this node and all its child nodes, and recalculate it if needed.
    /// </summary>
    /// <returns>Bounding sphere of the node and its children.</returns>
    public virtual BoundingSphere GetBoundingSphere()
    {
        // if need to update bounding sphere, update and return
        if (_boundingSphereDirty || WasParentUpdate) return UpdateBoundingSphere();

        // if no need to update just return last sphere calculated
        return LastBoundingSphere;
    }


    /// <summary>
    ///     Calculate bounding sphere and return results.
    ///     This also set internal caching.
    /// </summary>
    /// <returns>Bounding sphere of the node and its children.</returns>
    public virtual BoundingSphere UpdateBoundingSphere()
    {
        // count event
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.HeavyUpdate);

        // if empty skip
        if (Empty)
        {
            _boundingSphereDirty = false;
            LastBoundingSphere = EmptyBoundingSphere;
            return EmptyBoundingSphere;
        }

        // make sure transformations are up-to-date
        DoTransformationsUpdateIfNeeded();

        // bounding sphere to return
        var ret = new BoundingSphere();

        // calculate all child nodes bounding spheres
        foreach (var child in _childNodes)
        {
            // skip invisible nodes
            if (!child.Visible) continue;

            // get bounding sphere
            var currSphere = child.GetBoundingSphere();
            ret = BoundingSphere.CreateMerged(ret, currSphere);
        }

        // apply all entities directly under this node
        foreach (var entity in _childEntities)
        {
            // skip invisible entities
            if (!entity.Visible) continue;

            // get entity bounding sphere
            var currSphere = entity.GetBoundingSphere(this, ref _localTransform, ref _worldTransform);
            ret = BoundingSphere.CreateMerged(ret, currSphere);
        }

        // put into cache
        _boundingSphereDirty = false;
        LastBoundingSphere = ret;

        // return final bounding sphere
        return ret;
    }

    #region Properties

    /// <summary>
    ///     Get if this node can hold renderable entities (if not, it means this node is just for child nodes, and not for
    ///     entities.
    /// </summary>
    public virtual bool CanHoldEntities => true;


    /// <summary>
    ///     Get if parent transformations were update and we need to update too.
    /// </summary>
    protected bool WasParentUpdate => (Parent != null && _parentLastTransformVersion != Parent._transformVersion) ||
                                      (Parent == null && _parentLastTransformVersion != 0);

    /// <summary>
    ///     Return local transformations matrix (note: will recalculate if needed).
    /// </summary>
    public Matrix LocalTransformations
    {
        get
        {
            DoTransformationsUpdateIfNeeded();
            return _localTransform;
        }
    }

    /// <summary>
    ///     Return world transformations matrix (note: will recalculate if needed).
    /// </summary>
    public Matrix WorldTransformations
    {
        get
        {
            DoTransformationsUpdateIfNeeded();
            return _worldTransform;
        }
    }

    /// <summary>
    ///     Get position in world space.
    /// </summary>
    /// <remarks>
    ///     Naive implementation using world matrix decompose. For better performance, override this with your own cached
    ///     version.
    /// </remarks>
    public virtual Vector3 WorldPosition => WorldTransformations.Translation;

    /// <summary>
    ///     Get Rotastion in world space.
    /// </summary>
    /// <remarks>
    ///     Naive implementation using world matrix decompose. For better performance, override this with your own cached
    ///     version.
    /// </remarks>
    public virtual Quaternion WorldRotation
    {
        get
        {
            var world = WorldTransformations;
            return Math3D.GetRotation(ref world);
        }
    }

    /// <summary>
    ///     Get Scale in world space.
    /// </summary>
    /// <remarks>
    ///     Naive implementation using world matrix decompose. For better performance, override this with your own cached
    ///     version.
    /// </remarks>
    public virtual Vector3 WorldScale
    {
        get
        {
            var world = WorldTransformations;
            return Math3D.GetScale(ref world);
        }
    }


    /// <summary>
    ///     Get / Set the order in which we apply local transformations in this node.
    /// </summary>
    public TransformOrder TransformationsOrder
    {
        get => Transformations.TransformOrder;
        set
        {
            Transformations.TransformOrder = value;
            OnTransformationsSet();
        }
    }

    /// <summary>
    ///     Get / Set the rotation type (euler / quaternion).
    /// </summary>
    public RotationType RotationType
    {
        get => Transformations.RotationType;
        set
        {
            Transformations.RotationType = value;
            OnTransformationsSet();
        }
    }

    /// <summary>
    ///     Get / Set the order in which we apply local rotation in this node.
    /// </summary>
    public RotationOrder RotationOrder
    {
        get => Transformations.RotationOrder;
        set
        {
            Transformations.RotationOrder = value;
            OnTransformationsSet();
        }
    }

    /// <summary>
    ///     Get / Set node local position.
    /// </summary>
    public Vector3 Position
    {
        get => Transformations.Position;
        set
        {
            if (Transformations.Position != value) OnTransformationsSet();
            Transformations.Position = value;
        }
    }

    /// <summary>
    ///     Get / Set node local scale.
    /// </summary>
    public Vector3 Scale
    {
        get => Transformations.Scale;
        set
        {
            if (Transformations.Scale != value) OnTransformationsSet();
            Transformations.Scale = value;
        }
    }

    /// <summary>
    ///     Get / Set node local rotation.
    /// </summary>
    public Vector3 Rotation
    {
        get => Transformations.Rotation;
        set
        {
            if (Transformations.Rotation != value) OnTransformationsSet();
            Transformations.Rotation = value;
        }
    }

    /// <summary>
    ///     Alias to access rotation X directly.
    /// </summary>
    [NotInspectable]
    public float RotationX
    {
        get => Transformations.Rotation.X;
        set
        {
            if (Transformations.Rotation.X != value) OnTransformationsSet();
            Transformations.Rotation.X = value;
        }
    }

    /// <summary>
    ///     Alias to access rotation Y directly.
    /// </summary>
    [NotInspectable]
    public float RotationY
    {
        get => Transformations.Rotation.Y;
        set
        {
            if (Transformations.Rotation.Y != value) OnTransformationsSet();
            Transformations.Rotation.Y = value;
        }
    }

    /// <summary>
    ///     Alias to access rotation Z directly.
    /// </summary>
    [NotInspectable]
    public float RotationZ
    {
        get => Transformations.Rotation.Z;
        set
        {
            if (Transformations.Rotation.Z != value) OnTransformationsSet();
            Transformations.Rotation.Z = value;
        }
    }

    /// <summary>
    ///     Alias to access scale X directly.
    /// </summary>
    [NotInspectable]
    public float ScaleX
    {
        get => Transformations.Scale.X;
        set
        {
            if (Transformations.Scale.X != value) OnTransformationsSet();
            Transformations.Scale.X = value;
        }
    }

    /// <summary>
    ///     Alias to access scale Y directly.
    /// </summary>
    [NotInspectable]
    public float ScaleY
    {
        get => Transformations.Scale.Y;
        set
        {
            if (Transformations.Scale.Y != value) OnTransformationsSet();
            Transformations.Scale.Y = value;
        }
    }

    /// <summary>
    ///     Alias to access scale Z directly.
    /// </summary>
    [NotInspectable]
    public float ScaleZ
    {
        get => Transformations.Scale.Z;
        set
        {
            if (Transformations.Scale.Z != value) OnTransformationsSet();
            Transformations.Scale.Z = value;
        }
    }


    /// <summary>
    ///     Alias to access position X directly.
    /// </summary>
    [NotInspectable]
    public float PositionX
    {
        get => Transformations.Position.X;
        set
        {
            if (Transformations.Position.X != value) OnTransformationsSet();
            Transformations.Position.X = value;
        }
    }

    /// <summary>
    ///     Alias to access position Y directly.
    /// </summary>
    [NotInspectable]
    public float PositionY
    {
        get => Transformations.Position.Y;
        set
        {
            if (Transformations.Position.Y != value) OnTransformationsSet();
            Transformations.Position.Y = value;
        }
    }

    /// <summary>
    ///     Alias to access position Z directly.
    /// </summary>
    [NotInspectable]
    public float PositionZ
    {
        get => Transformations.Position.Z;
        set
        {
            if (Transformations.Position.Z != value) OnTransformationsSet();
            Transformations.Position.Z = value;
        }
    }

    /// <summary>
    ///     Return true if this node is empty.
    /// </summary>
    public bool Empty => _childEntities.Count == 0 && _childNodes.Count == 0;

    /// <summary>
    ///     Get if this node have any entities in it.
    /// </summary>
    public bool HaveEntities => _childEntities.Count != 0;

    /// <summary>
    ///     Other nodes this node is linked to.
    ///     This mechanism is used to connect nodes internally.
    /// </summary>
    internal List<Node> LinkedNodes
    {
        get
        {
            if (_linkedNodes == null) _linkedNodes = new List<Node>();
            return _linkedNodes;
        }
    }

    #endregion
}