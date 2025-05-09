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
// A static Body object.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Runtime.CompilerServices;
using BulletSharp;
using Microsoft.Xna.Framework;
using Nez.GeonBit.Physics.CollisionShapes;

[assembly: InternalsVisibleTo("World")]

namespace Nez.GeonBit.Physics;

/// <summary>
///     A static collision object that does not respond to events and can't be moved.
///     This is useful for really static things that can only block and collide, but don't really do anything else (trees,
///     rocks, etc..).
/// </summary>
public class StaticBody : BasicPhysicalBody
{
    /// <summary>
    ///     Create the static collision object from shape.
    /// </summary>
    /// <param name="shape">Collision shape that define this body.</param>
    /// <param name="transformations">Starting transformations.</param>
    public StaticBody(ICollisionShape shape, Matrix? transformations = null)
    {
        // create the collision object
        _shape = shape;
        BulletCollisionObject = new CollisionObject
        {
            CollisionShape = shape.BulletCollisionShape,
            UserObject = this
        };

        // turn off simulation and collision events
        base.EnableSimulation = false;
        InvokeCollisionEvents = false;

        // set default group and mask
        CollisionGroup = CollisionGroups.StaticObjects;
        CollisionMask = CollisionMasks.Targets;

        // if provided, set transformations
        if (transformations != null) BulletCollisionObject.WorldTransform = ToBullet.Matrix(transformations.Value);
    }

    /// <summary>
    ///     Return the bullet 3d entity.
    /// </summary>
    internal override CollisionObject _BulletEntity => BulletCollisionObject;

    /// <summary>
    ///     Get the rigid body in bullet format.
    /// </summary>
    internal CollisionObject BulletCollisionObject { get; }

    /// <summary>
    ///     Return if this is a static object.
    /// </summary>
    public override bool IsStatic => true;

    /// <summary>
    ///     Return if this is a kinematic object.
    /// </summary>
    public override bool IsKinematic => true;

    /// <summary>
    ///     If false, will not simulate forces on this body and will make it behave like a kinematic body.
    /// </summary>
    public override bool EnableSimulation
    {
        get => false;
        set
        {
            if (value) throw new InvalidOperationException("Cannot change the simulation state of a static body!");
        }
    }

    /// <summary>
    ///     Get / set object world transformations.
    ///     Get the rigid body in bullet format.
    /// </summary>
    public override Matrix WorldTransform
    {
        get => ToMonoGame.Matrix(_BulletEntity.WorldTransform);
        set
        {
            _BulletEntity.WorldTransform = ToBullet.Matrix(value);
            UpdateAABB();
        }
    }

    /// <summary>
    ///     Attach self to a bullet3d physics world.
    /// </summary>
    /// <param name="world"></param>
    internal override void AddSelfToBulletWorld(DynamicsWorld world)
    {
        UpdateCollisionFlags();
        world.AddCollisionObject(BulletCollisionObject, CollisionGroup, CollisionMask);
    }

    /// <summary>
    ///     Remove self from a bullet3d physics world.
    /// </summary>
    /// <param name="world">World to remove from.</param>
    internal override void RemoveSelfFromBulletWorld(DynamicsWorld world)
    {
        world.RemoveCollisionObject(BulletCollisionObject);
    }
}