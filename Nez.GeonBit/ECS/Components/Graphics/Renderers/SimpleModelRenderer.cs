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
// A component that renders a simple 3D model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit;

/// <summary>
///     This component renders a simple 3D model.
///     Unlike the ModelRenderer, this component is less customizeable, but renders slightly faster.
/// </summary>
public class SimpleModelRenderer : BaseRendererComponent
{
    /// <summary>
    ///     The entity from the core layer used to draw the model.
    /// </summary>
    protected SimpleModelEntity _entity;

    /// <summary>
    ///     Protected constructor without params to use without creating entity, for inheriting classes.
    /// </summary>
    protected SimpleModelRenderer()
    {
    }

    /// <summary>
    ///     Create the model renderer component.
    /// </summary>
    /// <param name="model">Model to draw.</param>
    public SimpleModelRenderer(Model model)
    {
        _entity = new SimpleModelEntity(model);
    }

    /// <summary>
    ///     Get the main entity instance of this renderer.
    /// </summary>
    protected override BaseRenderableEntity RenderableEntity => _entity;

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component CopyBasics(Component copyTo)
    {
        var ret = new SimpleModelRenderer(_entity.Model);
        base.CopyBasics(ret);
        return ret;
    }
}