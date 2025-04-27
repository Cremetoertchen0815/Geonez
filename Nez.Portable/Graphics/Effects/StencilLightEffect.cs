using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class StencilLightEffect : Effect
{
    private readonly EffectParameter _lightColorParam;

    private readonly EffectParameter _lightPositionParam;
    private readonly EffectParameter _lightRadius;
    private readonly EffectParameter _viewProjectionMatrixParam;

    public StencilLightEffect() : base(Core.GraphicsDevice, EffectResource.StencilLightBytes)
    {
        _lightPositionParam = Parameters["_lightSource"];
        _lightColorParam = Parameters["_lightColor"];
        _lightRadius = Parameters["_lightRadius"];
        _viewProjectionMatrixParam = Parameters["_viewProjectionMatrix"];
    }

    public Vector2 LightPosition
    {
        set => _lightPositionParam.SetValue(value);
    }

    public Color Color
    {
        set => _lightColorParam.SetValue(value.ToVector3());
    }

    public float Radius
    {
        set => _lightRadius.SetValue(value);
    }

    public Matrix ViewProjectionMatrix
    {
        set => _viewProjectionMatrixParam.SetValue(value);
    }
}