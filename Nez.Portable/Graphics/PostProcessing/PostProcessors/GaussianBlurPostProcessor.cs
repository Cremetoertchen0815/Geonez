using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez;

public class GaussianBlurPostProcessor(int executionOrder) : PostProcessor<GaussianBlurEffect>(executionOrder)
{
    private float _blurAmount = 2f;

    private float _renderTargetScale = 1f;


    /// <summary>
    ///     scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough.
    ///     Defaults to 1.
    /// </summary>
    public float RenderTargetScale
    {
        get => _renderTargetScale;
        set
        {
            if (value <= 0) return;
            if (_renderTargetScale != value)
            {
                _renderTargetScale = value;
                if (_scene is null) return;
                UpdateEffectDeltas();
            }
        }
    }

    public float BlurAmount
    {
        get => Effect?.BlurAmount ?? _blurAmount;
        set
        {
            _blurAmount = value;
            if (Effect is null) return;
            Effect.BlurAmount = value;
        }
    }

    public override void OnAddedToScene(Scene scene)
    {
        base.OnAddedToScene(scene);
        Effect = _scene.Content.LoadNezEffect<GaussianBlurEffect>();
        Effect.BlurAmount = _blurAmount;
        UpdateEffectDeltas();
    }

    public override void Unload()
    {
        _scene?.Content.UnloadEffect(Effect);
    }

    public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        UpdateEffectDeltas();
    }

    /// <summary>
    ///     updates the Effect with the new vertical and horizontal deltas
    /// </summary>
    private void UpdateEffectDeltas()
    {
        var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
        Effect.HorizontalBlurDelta = 1f / (sceneRenderTargetSize.X * _renderTargetScale);
        Effect.VerticalBlurDelta = 1f / (sceneRenderTargetSize.Y * _renderTargetScale);
    }

    public override void Process(RenderTarget2D source, RenderTarget2D destination)
    {
        // aquire a temporary rendertarget for the processing. It can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
        // the resolution in this way doesn't hurt quality, because we are going to be blurring the images in any case.
        var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
        var tempRenderTarget = RenderTarget.GetTemporary((int)(sceneRenderTargetSize.X * _renderTargetScale),
            (int)(sceneRenderTargetSize.Y * _renderTargetScale), DepthFormat.None);


        // Pass 1: draw from source into tempRenderTarget, applying a horizontal gaussian blur filter.
        Effect.PrepareForHorizontalBlur();
        DrawFullscreenQuad(source, tempRenderTarget, Effect);

        // Pass 2: draw from tempRenderTarget into destination, applying a vertical gaussian blur filter.
        Effect.PrepareForVerticalBlur();
        DrawFullscreenQuad(tempRenderTarget, destination, Effect);

        RenderTarget.ReleaseTemporary(tempRenderTarget);
    }
}