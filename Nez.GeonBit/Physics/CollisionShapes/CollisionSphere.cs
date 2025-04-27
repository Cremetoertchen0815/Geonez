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
// Collision shape for a sphere.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using BulletSharp;

namespace Nez.GeonBit.Physics.CollisionShapes;

/// <summary>
///     Sphere shape.
/// </summary>
public class CollisionSphere : ICollisionShape
{
    /// <summary>
    ///     Create the collision sphere.
    /// </summary>
    /// <param name="radius">Sphere radius.</param>
    public CollisionSphere(double radius = 1f)
    {
        _shape = new SphereShape(radius);
    }

    /// <summary>
    ///     Clone the physical shape.
    /// </summary>
    /// <returns>Cloned shape.</returns>
    protected override ICollisionShape CloneImp()
    {
        var shape = _shape as SphereShape;
        return new CollisionSphere(shape.Radius);
    }
}