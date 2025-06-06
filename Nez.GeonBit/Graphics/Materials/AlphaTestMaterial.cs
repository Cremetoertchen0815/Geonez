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
// A material with basic alpha test (invisble pixels are ommitted).
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Materials;

/// <summary>
///     Material with alpha test.
/// </summary>
public class AlphaTestMaterial : MaterialAPI
{
    // empty effect instance to clone when creating new material
    private static readonly AlphaTestEffect _emptyEffect = new(Core.GraphicsDevice);

    // the effect instance of this material.
    private readonly AlphaTestEffect _effect;

    private CompareFunction _alphaFunction = CompareFunction.GreaterEqual;

    private int _referenceAlpha = 128;

    /// <summary>
    ///     Create the default material from empty effect.
    /// </summary>
    public AlphaTestMaterial() : this(_emptyEffect)
    {
    }

    /// <summary>
    ///     Create the material.
    /// </summary>
    /// <param name="fromEffect">Effect to create material from.</param>
    /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
    public AlphaTestMaterial(AlphaTestEffect fromEffect, bool copyEffectProperties = true)
    {
        // store effect and set default properties
        _effect = fromEffect.Clone() as AlphaTestEffect;
        SetDefaults();

        // copy properties from effect itself
        if (copyEffectProperties)
        {
            // set effect defaults
            Texture = fromEffect.Texture;
            TextureEnabled = fromEffect.Texture != null;
            Alpha = fromEffect.Alpha;
        }
    }

    /// <summary>
    ///     Create the material from another material instance.
    /// </summary>
    /// <param name="other">Other material to clone.</param>
    public AlphaTestMaterial(AlphaTestMaterial other)
    {
        _effect = other._effect.Clone() as AlphaTestEffect;
        MaterialAPI asBase = this;
        other.CloneBasics(ref asBase);
        AlphaFunction = other.AlphaFunction;
        ReferenceAlpha = other.ReferenceAlpha;
    }

    /// <summary>
    ///     Get the effect instance.
    /// </summary>
    public override Effect Effect => _effect;

    /// <summary>
    ///     The function used to decide which pixels are visible and which are not.
    /// </summary>
    public CompareFunction AlphaFunction
    {
        get => _alphaFunction;
        set
        {
            _alphaFunction = value;
            SetAsDirty(MaterialDirtyFlags.AlphaTest);
        }
    }

    /// <summary>
    ///     Alpha value to compare with the AlphaFunction, to decide which pixels are visible and which are not.
    /// </summary>
    public int ReferenceAlpha
    {
        get => _referenceAlpha;
        set
        {
            _referenceAlpha = value;
            SetAsDirty(MaterialDirtyFlags.AlphaTest);
        }
    }

    /// <summary>
    ///     Apply this material.
    /// </summary>
    protected override void MaterialSpecificApply(bool wasLastMaterial)
    {
        // set world matrix
        if (IsDirty(MaterialDirtyFlags.World)) _effect.World = World;

        // if it was last material used, stop here - no need for the following settings
        if (wasLastMaterial) return;

        // set all effect params
        if (IsDirty(MaterialDirtyFlags.TextureParams)) _effect.Texture = Texture;
        if (IsDirty(MaterialDirtyFlags.Alpha)) _effect.Alpha = Alpha;
        if (IsDirty(MaterialDirtyFlags.MaterialColors)) _effect.DiffuseColor = DiffuseColor.ToVector3();
        if (IsDirty(MaterialDirtyFlags.AlphaTest))
        {
            _effect.AlphaFunction = AlphaFunction;
            _effect.ReferenceAlpha = ReferenceAlpha;
        }
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
        return new AlphaTestMaterial(this);
    }
}