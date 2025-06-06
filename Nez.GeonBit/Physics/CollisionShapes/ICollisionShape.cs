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
// Interface for a physical collision shape.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System.Runtime.CompilerServices;
using BulletSharp;
using BulletSharp.Math;

namespace Nez.GeonBit.Physics.CollisionShapes;

/// <summary>
///     GeonBit.Physics.CollisionShapes contain all the physical shapes we can use for rigid bodies.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     The interface of a physical collision shape.
/// </summary>
public abstract class ICollisionShape
{
    /// <summary>
    ///     Bullet shape instance (must be set by the inheriting class).
    /// </summary>
    protected CollisionShape _shape;

    /// <summary>
    ///     Get the bullet collision shape.
    /// </summary>
    internal CollisionShape BulletCollisionShape => _shape;

    /// <summary>
    ///     Clone the physical shape.
    /// </summary>
    /// <returns>Cloned shape.</returns>
    public ICollisionShape Clone()
    {
        // store old scale and reset scaling
        var oldScale = _shape.LocalScaling;
        _shape.LocalScaling = Vector3.One;

        // call the per-shape cloning logic
        var ret = CloneImp();

        // turn scale back to normal and return cloned shape
        _shape.LocalScaling = oldScale;
        return ret;
    }

    /// <summary>
    ///     Implement per-shape cloning logic.
    /// </summary>
    /// <returns>Cloned shape.</returns>
    protected abstract ICollisionShape CloneImp();
}