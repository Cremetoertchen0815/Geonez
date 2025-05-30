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
// Scene node optimized for particle systems.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

namespace Nez.GeonBit;

/// <summary>
///     A scene node optimized for particles.
/// </summary>
public class ParticleNode : BoundingSphereCullingNode
{
    /// <summary>
    ///     Clone this scene node.
    /// </summary>
    /// <returns>GeonNode copy.</returns>
    public override Node Clone()
    {
        var ret = new ParticleNode
        {
            Transformations = Transformations.Clone(),
            LastBoundingBox = LastBoundingBox,
            Visible = Visible
        };
        return ret;
    }

    /// <summary>
    ///     Update culling test / cached data.
    ///     This is called whenever trying to draw this node after transformations update
    /// </summary>
    protected override void UpdateCullingData()
    {
    }
}