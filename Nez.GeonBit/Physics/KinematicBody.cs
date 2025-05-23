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
// A Kinematic Body object.
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
///     A kinematic collision object that don't respond to forces.
///     This is useful for things like elevators, moving platforms, etc.
///     In other words - objects that move and interact, but ignore outside forces.
/// </summary>
public class KinematicBody : BasicPhysicalBody
{
    /// <summary>
    ///     Create the static collision object from shape.
    /// </summary>
    /// <param name="shape">Collision shape that define this body.</param>
    /// <param name="transformations">Starting transformations.</param>
    public KinematicBody(ICollisionShape shape, Matrix? transformations = null)
    {
        // create the collision object
        _shape = shape;
        BulletCollisionObject = new CollisionObject
        {
            CollisionShape = shape.BulletCollisionShape,
            UserObject = this
        };

        // turn off simulation
        base.EnableSimulation = false;

        // set default group and mask
        CollisionGroup = CollisionGroups.DynamicObjects;
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
            if (value) throw new InvalidOperationException("Cannot change the simulation state of a kinematic body!");
        }
    }

    /// <summary>
    ///     Get / set object world transformations.
    ///     Get the rigid body in bullet format.
    /// </summary>
    public override Matrix WorldTransform
    {
        get => ToMonoGame.Matrix(_BulletEntity.WorldTransform);
        set => _BulletEntity.WorldTransform = ToBullet.Matrix(value);
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