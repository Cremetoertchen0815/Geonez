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
// Global, static graphics manager.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Lights;
using Nez.GeonBit.Materials;
using Nez.GeonBit.Physics;
using Nez.Systems;

namespace Nez.GeonBit;

/// <summary>
///     All built-in blend states.
/// </summary>
public static class BlendStates
{
    /// <summary>
    ///     Additive blending.
    /// </summary>
    public static BlendState Additive = BlendState.Additive;

    /// <summary>
    ///     Alpha blend (alpha channels).
    /// </summary>
    public static BlendState AlphaBlend = BlendState.AlphaBlend;

    /// <summary>
    ///     Non-premultiplied blending.
    /// </summary>
    public static BlendState NonPremultiplied = BlendState.NonPremultiplied;

    /// <summary>
    ///     Opaque (no alpha blending).
    /// </summary>
    public static BlendState Opaque = BlendState.Opaque;
}

/// <summary>
///     A callback to generate the default materials all loaded meshes will recieve.
/// </summary>
/// <param name="mgEffect">MonoGame effect loaded by the mesh loader. You can use it to extract data.</param>
/// <returns>Material instance.</returns>
public delegate MaterialAPI DefaultMaterialGenerator(Effect mgEffect);

/// <summary>
///     A global static class for graphic utilities and management.
/// </summary>
public class GeonDefaultRenderer : Renderer
{
    // sprite batch used by this manager
    private static readonly Batcher _batcher = new(Core.GraphicsDevice);

    private static Scene _scene;

    internal static NezContentManager CurrentContentManager = Core.Content;

    /// <summary>
    ///     Deferred lighting manager.
    /// </summary>
    private Lights.DeferredLighting _DeferredLighting;


    private PhysicsWorld _physics;

    private bool _renderingPrepared;

    public GeonDefaultRenderer(int renderOrder, Scene sourceScene) : base(renderOrder)
    {
        _scene = sourceScene;
        CurrentContentManager = sourceScene.Content;
        RenderingQueues.Initialize();
    }

    /// <summary>
    ///     Manage lights and serve them to materials.
    ///     This object holds the currently active lights manager, given by the scene.
    /// </summary>
    internal static LightsManager ActiveLightsManager { get; set; }

    /// <summary>
    ///     Return if deferred lighting is currently enabled.
    /// </summary>
    public bool IsDeferredLightingEnabled => _DeferredLighting != null;

    /// <summary>
    ///     Currently active camera.
    /// </summary>
    public static Camera3D ActiveCamera { get; set; }

    public bool ForceDrawingToSceneTarget { get; set; } = false;

    /// <summary>
    ///     Get viewport current size (in pixels).
    /// </summary>
    public static Point ViewportSize => new(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height);

    /// <summary>
    ///     Enable deferred lighting.
    /// </summary>
    /// <returns></returns>
    public void EnableDeferredLighting()
    {
        _DeferredLighting = new Lights.DeferredLighting();
    }

    public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        base.OnSceneBackBufferSizeChanged(newWidth, newHeight);
        if (IsDeferredLightingEnabled) _DeferredLighting.OnResize();
    }

    /// <summary>
    ///     Render a renderable entity.
    ///     Will either render immediately, or add to the corresponding rendering queue.
    /// </summary>
    /// <param name="entity">Entity to render.</param>
    /// <param name="world">World matrix for the entity.</param>
    public static void DrawEntity(BaseRenderableEntity entity, Matrix world)
    {
        // if no queue, draw immediately and return
        if (entity.RenderingQueue == RenderingQueue.NoQueue)
        {
            entity.DoEntityDraw(ref world);
            return;
        }

        // add to the rendering queue
        RenderingQueues.AddEntity(entity, world);
    }

    /// <summary>
    ///     Start a drawing frame.
    /// </summary>
    public void PrepareRendering(Scene scene, bool forceVisible, Matrix? viewMatrix = null,
        Matrix? projectionMatrix = null)
    {
        //Don't prepare rendering queues if we are already prepared them in the same frame
        if (_renderingPrepared) return;
        _renderingPrepared = true;

        // update culling nodes camera frustum
        CullingNode.CurrentCameraFrustum = ActiveCamera != null ? ActiveCamera.ViewFrustum : null;


        // update materials view and projection matrix
        if (ActiveCamera != null)
            MaterialAPI.SetViewProjection(viewMatrix ?? ActiveCamera.View, projectionMatrix ?? ActiveCamera.Projection);

        // start frame for deferred lighting manager
        if (IsDeferredLightingEnabled) _DeferredLighting.FrameStart();
        // notify nodes manager that a frame started
        NodesManager.StartFrame();

        //Draw node(fill rendering queues)
        var lst = scene.EntitiesOfType<GeonEntity>();
        foreach (var item in lst) item.Node?.Draw(forceVisible, forceVisible);
        ListPool<GeonEntity>.Free(lst);
    }

    /// <summary>
    ///     Finish a drawing frame and render everything in queues.
    /// </summary>
    public void FinishRendering()
    {
        // draw rendering queues
        RenderingQueues.DrawQueues();

        // notify nodes manager that a frame ended
        NodesManager.EndFrame();

        // start frame for deferred lighting manager
        if (IsDeferredLightingEnabled) _DeferredLighting.FrameEnd();

        // clear the last material applied
        MaterialAPI._lastMaterialApplied = null;

        _renderingPrepared = false;
    }

    /// <summary>
    ///     Draw a tiled texture.
    /// </summary>
    /// <param name="texture">Texture to draw.</param>
    /// <param name="position">Position.</param>
    /// <param name="sourceRect">Source rectangle in texture (also affect drawing size).</param>
    public static void DrawTiledTexture(Texture2D texture, Vector2 position, Rectangle sourceRect)
    {
        _batcher.Begin(null, SamplerState.LinearWrap, null, null);
        _batcher.Draw(texture, position, sourceRect, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        _batcher.End();
    }

    /// <summary>
    ///     Draw a plain texture.
    /// </summary>
    /// <param name="texture">Texture to draw.</param>
    /// <param name="sourceRect">Source rectangle in texture (also affect drawing size).</param>
    /// <param name="destRect">Dest rectangle in viewport.</param>
    public static void DrawTexture(Texture2D texture, Rectangle sourceRect, Rectangle destRect)
    {
        _batcher.Begin();
        _batcher.Draw(texture, destRect, sourceRect, Color.White);
        _batcher.End();
    }

    /// <summary>
    ///     Draw a plain texture.
    /// </summary>
    /// <param name="texture">Texture to draw.</param>
    /// <param name="sourceRect">Source rectangle in texture (also affect drawing size).</param>
    /// <param name="position">Dest position in viewport.</param>
    /// <param name="scale">Will scale the rendered texture.</param>
    /// <param name="origin">Origin for rotation, scaling, etc.</param>
    /// <param name="color">Optional color.</param>
    public static void DrawTexture(Texture2D texture, Rectangle sourceRect, Vector2 position, float scale,
        Vector2 origin, Color? color = null)
    {
        _batcher.Begin();
        _batcher.Draw(texture, position, sourceRect,
            color ?? Color.White,
            0,
            origin,
            Vector2.One * scale,
            SpriteEffects.None,
            0f);
        _batcher.End();
    }

    public override void Render(Scene scene)
    {
        if (RenderTexture is not null)
        {
            Core.GraphicsDevice.SetRenderTarget(RenderTexture);
            Core.GraphicsDevice.Clear(RenderTargetClearColor);
        }
        else if (ForceDrawingToSceneTarget)
        {
            Core.GraphicsDevice.SetRenderTarget(scene.SceneRenderTarget);
            Core.GraphicsDevice.Clear(RenderTargetClearColor);
        }

        PrepareRendering(scene, false);

        FinishRendering();

        if (ShouldDebugRender && Core.DebugRenderEnabled) DebugRender(scene, null);

        // reset stencil state
        Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    public void RenderShadows(GeonScene scene)
    {
        PrepareRendering(scene, true);

        Core.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        Core.GraphicsDevice.BlendState = BlendState.Opaque;

        foreach (var item in scene.Lighting.GetShadowedLights())
        {
            Core.GraphicsDevice.SetRenderTarget(item.ShadowMap);
            Core.GraphicsDevice.Clear(Color.White);

            var matrices = LightsManager.ShadowEffect as IEffectMatrices;
            matrices.View = item.ShadowViewMatrix;
            matrices.Projection = item.ShadowProjectionMatrix;
            RenderingQueues.RenderShadows(item.ShadowSourceID);
            if (item.ShadowStencil is not null)
            {
                Nez.Graphics.Instance.Batcher.Begin();
                Nez.Graphics.Instance.Batcher.Draw(item.ShadowStencil,
                    new Rectangle(0, 0, item.ShadowMap.Width, item.ShadowMap.Height), Color.Black);
                Nez.Graphics.Instance.Batcher.End();
            }
            //item.ShadowMap.SaveAsPng(System.IO.File.OpenWrite("lolz.png"), 2048, 2048);
        }
    }

    public void RenderFromPoint(Vector3 position, Vector3 direction, Vector3 up, Matrix projMatrix)
    {
        PrepareRendering(_scene, true, Matrix.CreateLookAt(position, position + direction, up), projMatrix);

        FinishRendering();

        // reset stencil state
        Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    public TextureCube CaptureEnvironmentMap(RenderTargetCube r, Vector3 position, int size)
    {
        var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 1000f);
        //Core.GraphicsDevice.SetRenderTarget(rr);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.PositiveX);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Left, Vector3.Up, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/left_b.png"), 1000, 1000);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.NegativeX);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Right, Vector3.Up, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/right_b.png"), 1000, 1000);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.PositiveY);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Up, Vector3.Backward, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/top_b.png"), 1000, 1000);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.NegativeY);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Down, Vector3.Forward, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/down_b.png"), 1000, 1000);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.PositiveZ);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Forward, Vector3.Up, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/front_b.png"), 1000, 1000);
        Core.GraphicsDevice.SetRenderTarget(r, CubeMapFace.NegativeZ);
        //Core.GraphicsDevice.Clear(Color.Black);
        RenderFromPoint(position, Vector3.Backward, Vector3.Up, proj);
        //rr.SaveAsPng(System.IO.File.OpenWrite("C:/Users/Creme/Desktop/cubemap/back_b.png"), 1000, 1000);


        return r;
    }

    protected override void DebugRender(Scene scene, Camera cam)
    {
        if (_physics == null) _physics = scene.GetSceneComponent<PhysicsWorld>();

        _physics?.DebugDraw();
    }
}