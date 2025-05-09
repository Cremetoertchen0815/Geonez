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
// A material to use on skybox mesh.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Materials;

/// <summary>
///     A material for skybox mesh.
/// </summary>
public class SkyboxMaterial : MaterialAPI
{
    // the effect instance of this material.
    private readonly BasicEffect _effect;

    /// <summary>
    ///     If true, will rotate by 90 degrees on X axis to flip between Y and Z axis.
    ///     Useful for blender exported models.
    /// </summary>
    public bool FlipYZ;

    /// <summary>
    ///     Create the default material.
    /// </summary>
    /// <param name="texture">Skybox texture.</param>
    /// <param name="flipYZ">If true, will flip between Y and Z axis.</param>
    public SkyboxMaterial(string texture, bool flipYZ = false)
    {
        // store effect and set default properties
        FlipYZ = flipYZ;
        _effect = new BasicEffect(Core.GraphicsDevice)
        {
            TextureEnabled = true,
            Texture = GeonDefaultRenderer.CurrentContentManager.LoadTexture(texture),
            LightingEnabled = false
        };
    }

    /// <summary>
    ///     Create the default material.
    /// </summary>
    /// <param name="texture">Skybox texture.</param>
    /// <param name="flipYZ">If true, will flip between Y and Z axis.</param>
    public SkyboxMaterial(Texture2D texture, bool flipYZ = false)
    {
        // store effect and set default properties
        FlipYZ = flipYZ;
        _effect = new BasicEffect(Core.GraphicsDevice)
        {
            TextureEnabled = true,
            Texture = texture,
            LightingEnabled = false
        };
    }

    /// <summary>
    ///     Create the material from another material instance.
    /// </summary>
    /// <param name="other">Other material to clone.</param>
    public SkyboxMaterial(SkyboxMaterial other)
    {
        _effect = other._effect.Clone() as BasicEffect;
        MaterialAPI asBase = this;
        other.CloneBasics(ref asBase);
        FlipYZ = other.FlipYZ;
    }

    /// <summary>
    ///     Get the effect instance.
    /// </summary>
    public override Effect Effect => _effect;

    /// <summary>
    ///     Apply this material.
    /// </summary>
    protected override void MaterialSpecificApply(bool wasLastMaterial)
    {
        // create world matrix which is camera position + scale large enough to cover far plane
        _effect.World =
            (FlipYZ ? Matrix.CreateRotationX((float)Math.PI * -0.5f) : Matrix.Identity) *
            Matrix.CreateScale(GeonDefaultRenderer.ActiveCamera.FarClipPlane * Vector3.One * 1.5f) *
            Matrix.CreateTranslation(GeonDefaultRenderer.ActiveCamera.OverridePosition ??
                                     GeonDefaultRenderer.ActiveCamera.Position);
    }

    /// <summary>
    ///     Update material view matrix.
    /// </summary>
    /// <param name="view">New view to set.</param>
    protected override void UpdateView(ref Matrix view)
    {
        _effect.View = View;
    }

    /// <summary>
    ///     Update material projection matrix.
    /// </summary>
    /// <param name="projection">New projection to set.</param>
    protected override void UpdateProjection(ref Matrix projection)
    {
        _effect.Projection = Projection;
    }

    /// <summary>
    ///     Clone this material.
    /// </summary>
    /// <returns>Copy of this material.</returns>
    public override MaterialAPI Clone()
    {
        return new SkyboxMaterial(this);
    }
}