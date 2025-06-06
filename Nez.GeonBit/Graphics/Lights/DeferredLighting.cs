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
// Manage deferred lighting components.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Lights;

/// <summary>
///     A class to manage the deferred lighting parts of the engine.
/// </summary>
public class DeferredLighting
{
    // hold diffuse color + specular intensity (on a)
    private RenderTarget2D _rtColorSpecularIntensity;

    // hold depth of the pixels
    private RenderTarget2D _rtDepth;

    // hold normals + specular power (on a)
    private RenderTarget2D _rtNormalsSpecularPower;

    /// <summary>
    ///     Initialize deferred lighting manager.
    /// </summary>
    public DeferredLighting()
    {
    }

    /// <summary>
    ///     Clear deferred lighting.
    /// </summary>
    ~DeferredLighting()
    {
        Dispose();
    }

    /// <summary>
    ///     Dispose buffers and render targets.
    /// </summary>
    public void Dispose()
    {
        if (_rtColorSpecularIntensity != null) _rtColorSpecularIntensity.Dispose();

        if (_rtNormalsSpecularPower != null) _rtNormalsSpecularPower.Dispose();

        if (_rtDepth != null) _rtDepth.Dispose();

        _rtColorSpecularIntensity = _rtNormalsSpecularPower = _rtDepth = null;
    }

    /// <summary>
    ///     Handle resize event.
    /// </summary>
    public void OnResize()
    {
        // dispose previous render targets, if exists
        Dispose();

        // get device and screen width and height
        var device = Core.GraphicsDevice;
        var backBufferWidth = device.PresentationParameters.BackBufferWidth;
        var backBufferHeight = device.PresentationParameters.BackBufferHeight;

        // create render targets
        _rtColorSpecularIntensity = new RenderTarget2D(device, backBufferWidth, backBufferHeight, false,
            SurfaceFormat.Color, DepthFormat.None);
        _rtNormalsSpecularPower = new RenderTarget2D(device, backBufferWidth, backBufferHeight, false,
            SurfaceFormat.Color, DepthFormat.None);
        _rtDepth = new RenderTarget2D(device, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single,
            DepthFormat.None);
    }

    /// <summary>
    ///     Call when a frame starts to apply the g-buffer.
    /// </summary>
    public void FrameStart()
    {
        SetGBuffer();
        ClearGBuffer();
    }

    /// <summary>
    ///     Call when a frame ends to resolve the g-buffer.
    /// </summary>
    public void FrameEnd()
    {
        var device = Core.GraphicsDevice;
        device.SetRenderTarget(null);
    }

    /// <summary>
    ///     Set the g-buffer.
    /// </summary>
    private void SetGBuffer()
    {
        var device = Core.GraphicsDevice;
        device.SetRenderTargets(_rtColorSpecularIntensity, _rtNormalsSpecularPower, _rtDepth);
    }

    /// <summary>
    ///     Clear the g-buffer components.
    /// </summary>
    private void ClearGBuffer()
    {
        var device = Core.GraphicsDevice;
        device.Clear(Color.Gray);
    }
}