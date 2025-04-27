using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class NoiseEffect : Effect
{
    private readonly EffectParameter _noiseParam;

    private float _noise = 1f;


    public NoiseEffect() : base(Core.GraphicsDevice, EffectResource.NoiseBytes)
    {
        _noiseParam = Parameters["noise"];
        _noiseParam.SetValue(_noise);
    }

    /// <summary>
    ///     Intensity of the noise. Defaults to 1.
    /// </summary>
    [Range(0, 10)]
    public float Noise
    {
        get => _noise;
        set
        {
            if (_noise != value)
            {
                _noise = value;
                _noiseParam.SetValue(_noise);
            }
        }
    }
}