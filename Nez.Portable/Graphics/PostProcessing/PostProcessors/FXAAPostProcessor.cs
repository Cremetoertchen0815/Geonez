using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class FXAAPostProcessor : PostProcessor
{
    private readonly EffectParameter _paramEdgeThreshold;
    private readonly EffectParameter _paramEdgeThresholdMin;
    private readonly EffectParameter _paramInvViewportHeight;
    private readonly EffectParameter _paramInvViewportWidth;
    private readonly EffectParameter _paramSubpix;
    private float _edgeThreshold = 0.166f;
    private float _edgeThresholdMin = 0.0833f;

    private float _subpix = 0.75f;

    public FXAAPostProcessor(int executionOrder) : base(executionOrder)
    {
        SamplerState = SamplerState.LinearClamp;
        Effect = Core.Content.LoadEffect<Effect>("FXAA", EffectResource.FXAntiAliasing);

        var param = Effect.Parameters;
        _paramSubpix = param["fxaaSubpix"];
        _paramEdgeThreshold = param["fxaaEdgeThreshold"];
        _paramEdgeThresholdMin = param["fxaaEdgeThresholdMin"];
        _paramInvViewportWidth = param["invViewportWidth"];
        _paramInvViewportHeight = param["invViewportHeight"];

        //Set default values
        _paramSubpix.SetValue(_subpix);
        _paramEdgeThreshold.SetValue(_edgeThreshold);
        _paramEdgeThresholdMin.SetValue(_edgeThresholdMin);
    }

    public float Subpix
    {
        get => _subpix;
        set
        {
            _subpix = value;
            _paramSubpix.SetValue(value);
        }
    }

    public float EdgeThreshold
    {
        get => _edgeThreshold;
        set
        {
            _edgeThreshold = value;
            _paramEdgeThreshold.SetValue(value);
        }
    }

    public float EdgeThresholdMin
    {
        get => _edgeThresholdMin;
        set
        {
            _edgeThresholdMin = value;
            _paramEdgeThresholdMin.SetValue(value);
        }
    }

    public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        _paramInvViewportWidth.SetValue(1f / newWidth);
        _paramInvViewportHeight.SetValue(1f / newHeight);
    }
}