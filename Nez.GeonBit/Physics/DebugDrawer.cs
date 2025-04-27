﻿#region File Description

//-----------------------------------------------------------------------------
// Debug drawer for BulletSharp.
// Draws physical world etc when in debug mode.
//
// This code is almost entirely take from Mark Neale BulletSharp examples, see original comment below.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

/*
 * C# / XNA  port of Bullet (c) 2011 Mark Neale <xexuxjy@hotmail.com>
 *
 * Bullet Continuous Collision Detection and Physics Library
 * Copyright (c) 2003-2008 Erwin Coumans  http://www.bulletphysics.com/
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = BulletSharp.Math.Vector3;

namespace Nez.GeonBit.Physics;

/// <summary>
///     Draw Bullet3d physical world using MonoGame graphic device, for debug purposes.
/// </summary>
public class PhysicsDebugDraw : DebugDraw
{
    // graphic device instance
    private readonly GraphicsDevice _device;

    // effect to use for debug drawings
    private BasicEffect _effect;

    /// <summary>
    ///     Create the debug drawer.
    /// </summary>
    /// <param name="device">MonoGame graphic device.</param>
    public PhysicsDebugDraw(GraphicsDevice device)
    {
        _device = device;
    }

    /// <summary>
    ///     Get if in debug mode or not.
    /// </summary>
    public override DebugDrawModes DebugMode
    {
        get => DebugDrawModes.DrawWireframe | DebugDrawModes.DrawContactPoints | DebugDrawModes.NoHelpText;
        set { }
    }

    /// <summary>
    ///     Draw 3d text.
    /// </summary>
    /// <param name="location">Psition to draw.</param>
    /// <param name="textString">String to render.</param>
    public override void Draw3DText(ref Vector3 location, string textString)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Draw contact point.
    /// </summary>
    /// <param name="pointOnB"></param>
    /// <param name="normalOnB"></param>
    /// <param name="distance"></param>
    /// <param name="lifeTime"></param>
    /// <param name="color"></param>
    public void DrawContactPoint(ref Microsoft.Xna.Framework.Vector3 pointOnB,
        ref Microsoft.Xna.Framework.Vector3 normalOnB, float distance, int lifeTime, Color color)
    {
        var vertices = new[]
        {
            new VertexPositionColor(pointOnB, color),
            new VertexPositionColor(pointOnB + normalOnB, color)
        };
        _device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
    }

    /// <summary>
    ///     Draw a line with a single color.
    /// </summary>
    /// <param name="from">Starting pos.</param>
    /// <param name="to">Ending pos.</param>
    /// <param name="color">Color.</param>
    public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
    {
        var col = new Color((float)color.X, (float)color.Y, (float)color.Z);
        var vertices = new[]
        {
            new VertexPositionColor(ToMonoGame.Vector(from), col),
            new VertexPositionColor(ToMonoGame.Vector(to), col)
        };
        _device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
    }

    /// <summary>
    ///     Draw a line with graduate colors.
    /// </summary>
    /// <param name="from">Starting pos.</param>
    /// <param name="to">Ending pos.</param>
    /// <param name="color">Starting color.</param>
    /// <param name="color2">Ending color.</param>
    public void DrawLine(ref Microsoft.Xna.Framework.Vector3 from, ref Microsoft.Xna.Framework.Vector3 to,
        ref Microsoft.Xna.Framework.Vector3 color, ref Microsoft.Xna.Framework.Vector3 color2)
    {
        var col = new Color(color.X, color.Y, color.Z);
        var col2 = new Color(color2.X, color2.Y, color2.Z);
        var vertices = new[]
        {
            new VertexPositionColor(from, col),
            new VertexPositionColor(to, col2)
        };
        _device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
    }

    /// <summary>
    ///     Draw the physical world.
    /// </summary>
    /// <param name="world">World to draw.</param>
    public void DrawDebugWorld(DynamicsWorld world)
    {
        // no camera? skip
        if (GeonDefaultRenderer.ActiveCamera == null) return;

        // create effect if needed
        if (_effect == null)
            _effect = new BasicEffect(_device)
            {
                VertexColorEnabled = true
            };

        // set effect properties
        _effect.View = GeonDefaultRenderer.ActiveCamera.View;
        _effect.Projection = GeonDefaultRenderer.ActiveCamera.Projection;

        // set self as the debug drawer
        world.DebugDrawer = this;

        // reset depth stencil and rasterizer states
        var RasterizerState = new RasterizerState();
        var DepthStencilState = new DepthStencilState();
        RasterizerState.CullMode = CullMode.None;
        RasterizerState.DepthClipEnable = true;
        RasterizerState.FillMode = FillMode.Solid;
        DepthStencilState.DepthBufferEnable = true;
        DepthStencilState.DepthBufferWriteEnable = true;
        Core.GraphicsDevice.RasterizerState = RasterizerState;
        Core.GraphicsDevice.DepthStencilState = DepthStencilState;

        // apply effect
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            // draw current pass
            pass.Apply();

            // draw world
            world.DebugDrawWorld();
        }
    }

    /// <summary>
    ///     Report error warning from Bullet3d.
    /// </summary>
    /// <param name="warningString">Warning to report.</param>
    public override void ReportErrorWarning(string warningString)
    {
        throw new NotImplementedException();
    }
}