using Microsoft.Xna.Framework.Graphics;

namespace Nez;
public class FXAAPostProcessor : PostProcessor
{
    private EffectParameter _paramSubpix;
    private EffectParameter _paramEdgeThreshold;
    private EffectParameter _paramEdgeThresholdMin;
    private EffectParameter _paramInvViewportWidth;
    private EffectParameter _paramInvViewportHeight;

    private float _subpix = 0.75f;
    private float _edgeThreshold = 0.166f;
    private float _edgeThresholdMin = 0.0833f;

    public FXAAPostProcessor(int executionOrder) : base(executionOrder)
    {
        SamplerState = SamplerState.LinearClamp;
        Effect = Core.Content.LoadEffect<Effect>("FXAA", EffectResource.FXAntiAliasing);

        var param = Effect.Parameters;
        _paramSubpix =  param["fxaaSubpix"];
        _paramEdgeThreshold = param["fxaaEdgeThreshold"];
        _paramEdgeThresholdMin = param["fxaaEdgeThresholdMin"];
        _paramInvViewportWidth = param["invViewportWidth"];
        _paramInvViewportHeight = param["invViewportHeight"];

        //Set default values
        _paramSubpix.SetValue(_subpix);
        _paramEdgeThreshold.SetValue(_edgeThreshold);
        _paramEdgeThresholdMin.SetValue(_edgeThresholdMin);
    }

    public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        _paramInvViewportWidth.SetValue(1f / newWidth);
        _paramInvViewportHeight.SetValue(1f / newHeight);
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
}
