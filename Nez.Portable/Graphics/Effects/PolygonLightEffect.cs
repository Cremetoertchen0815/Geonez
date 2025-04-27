using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class PolygonLightEffect : Effect
{
    private readonly EffectParameter _lightColorParam;
    private readonly EffectParameter _lightRadius;
    private readonly EffectParameter _lightSourceParam;

    private readonly EffectParameter _viewProjectionMatrixParam;

    public PolygonLightEffect() : base(Core.GraphicsDevice, EffectResource.PolygonLightBytes)
    {
        _viewProjectionMatrixParam = Parameters["viewProjectionMatrix"];
        _lightSourceParam = Parameters["lightSource"];
        _lightColorParam = Parameters["lightColor"];
        _lightRadius = Parameters["lightRadius"];
    }

    public Matrix ViewProjectionMatrix
    {
        set => _viewProjectionMatrixParam.SetValue(value);
    }

    public Vector2 LightSource
    {
        set => _lightSourceParam.SetValue(value);
    }

    public Vector3 LightColor
    {
        set => _lightColorParam.SetValue(value);
    }

    public float LightRadius
    {
        set => _lightRadius.SetValue(value);
    }
}