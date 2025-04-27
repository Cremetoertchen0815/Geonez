﻿using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;

namespace Nez;

/// <summary>
///     builds up a cover of squares then removes them
/// </summary>
public class SquaresTransition : SceneTransition
{
    private readonly Rectangle _destinationRect;
    private readonly Effect _squaresEffect;
    private Texture2D _overlayTexture;

    /// <summary>
    ///     delay before removing squares
    /// </summary>
    public float DelayBeforeSquaresInDuration = 0;

    /// <summary>
    ///     ease equation to use for the animation
    /// </summary>
    public EaseType EaseType = EaseType.QuartOut;

    /// <summary>
    ///     duration for squares to populate the screen
    /// </summary>
    public float SquaresInDuration = 0.6f;

    /// <summary>
    ///     duration for squares to unpopulate screen
    /// </summary>
    public float SquaresOutDuration = 0.6f;


    public SquaresTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
    {
        _destinationRect = PreviousSceneRender.Bounds;

        // load Effect and set defaults
        _squaresEffect = Core.Content.LoadEffect("nez/effects/transitions/Squares.mgfxo");
        SquareColor = Color.Black;
        Smoothness = 0.5f;

        var aspectRatio = Screen.BackbufferWidth / (float)Screen.BackbufferHeight;
        Size = new Vector2(30, 30 / aspectRatio);
    }


    public SquaresTransition() : this(null)
    {
    }

    /// <summary>
    ///     color of the squares
    /// </summary>
    /// <value>The color of the square.</value>
    public Color SquareColor
    {
        set => _squaresEffect.Parameters["_color"].SetValue(value.ToVector4());
    }

    public float Smoothness
    {
        set => _squaresEffect.Parameters["_smoothness"].SetValue(value);
    }

    /// <summary>
    ///     size of the squares. If you want perfect squares use size, size / aspectRatio_of_screen
    /// </summary>
    /// <value>The size.</value>
    public Vector2 Size
    {
        set => _squaresEffect.Parameters["_size"].SetValue(value);
    }


    public override IEnumerator OnBeginTransition()
    {
        // create a single pixel transparent texture so we can do our squares out to the next scene
        _overlayTexture = Graphics.CreateSingleColorTexture(1, 1, Color.Transparent);

        // populate squares
        yield return Core.StartCoroutine(TickEffectProgressProperty(_squaresEffect, SquaresInDuration, EaseType));

        // load up the new Scene
        yield return Core.StartCoroutine(LoadNextScene());

        // dispose of our previousSceneRender. We dont need it anymore.
        PreviousSceneRender.Dispose();
        PreviousSceneRender = null;

        // delay
        yield return Coroutine.WaitForSeconds(DelayBeforeSquaresInDuration);

        // unpopulate squares
        yield return Core.StartCoroutine(TickEffectProgressProperty(_squaresEffect, SquaresOutDuration,
            EaseHelper.OppositeEaseType(EaseType), true));

        TransitionComplete();

        // cleanup
        _overlayTexture.Dispose();
        Core.Content.UnloadEffect(_squaresEffect.Name);
    }


    public override void Render(Batcher batcher)
    {
        Core.GraphicsDevice.SetRenderTarget(null);
        batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null,
            _squaresEffect);

        // we only render the previousSceneRender while populating the squares
        if (!_isNewSceneLoaded)
            batcher.Draw(PreviousSceneRender, _destinationRect, Color.White);
        else
            batcher.Draw(_overlayTexture, new Rectangle(0, 0, Screen.BackbufferWidth, Screen.BackbufferHeight),
                Color.Transparent);

        batcher.End();
    }
}