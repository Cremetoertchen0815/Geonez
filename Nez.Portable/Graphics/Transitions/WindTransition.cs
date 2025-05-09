﻿using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;

namespace Nez;

/// <summary>
///     sweeps wind accross the screen revealing the new Scene
/// </summary>
public class WindTransition : SceneTransition
{
    private readonly Rectangle _destinationRect;
    private readonly Effect _windEffect;

    /// <summary>
    ///     duration for the wind transition
    /// </summary>
    public float Duration = 1f;

    /// <summary>
    ///     ease equation to use for the animation
    /// </summary>
    public EaseType EaseType = EaseType.QuartOut;


    public WindTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction)
    {
        _destinationRect = PreviousSceneRender.Bounds;

        // load Effect and set defaults
        _windEffect = Core.Content.LoadEffect("nez/effects/transitions/Wind.mgfxo");
        Size = 0.3f;
        WindSegments = 100;
    }


    public WindTransition() : this(null)
    {
    }

    /// <summary>
    ///     how many wind segments should be used. Defaults to 100. (1 - 1000)
    /// </summary>
    /// <value>The wind segments.</value>
    public float WindSegments
    {
        set => _windEffect.Parameters["_windSegments"].SetValue(value);
    }

    /// <summary>
    ///     size of the wind streaks. defaults to 0.3. (0.1 - 1)
    /// </summary>
    /// <value>The size.</value>
    public float Size
    {
        set => _windEffect.Parameters["_size"].SetValue(value);
    }


    public override IEnumerator OnBeginTransition()
    {
        // load up the new Scene
        yield return Core.StartCoroutine(LoadNextScene());

        // wind to the new Scene
        yield return Core.StartCoroutine(TickEffectProgressProperty(_windEffect, Duration, EaseType));

        TransitionComplete();

        // cleanup
        Core.Content.UnloadEffect(_windEffect.Name);
    }


    public override void Render(Batcher batcher)
    {
        Core.GraphicsDevice.SetRenderTarget(null);
        batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null, _windEffect);
        batcher.Draw(PreviousSceneRender, _destinationRect, Color.White);
        batcher.End();
    }
}