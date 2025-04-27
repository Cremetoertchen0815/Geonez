using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class SepiaEffect : Effect
{
    private readonly EffectParameter _sepiaToneParam;

    private Vector3 _sepiaTone = new(1.2f, 1.0f, 0.8f);


    public SepiaEffect() : base(Core.GraphicsDevice, EffectResource.SepiaBytes)
    {
        _sepiaToneParam = Parameters["_sepiaTone"];
        _sepiaToneParam.SetValue(_sepiaTone);
    }

    /// <summary>
    ///     multiplied by the grayscale value for the final output. Defaults to 1.2f, 1.0f, 0.8f
    /// </summary>
    /// <value>The sepia tone.</value>
    public Vector3 SepiaTone
    {
        get => _sepiaTone;
        set
        {
            _sepiaTone = value;
            _sepiaToneParam.SetValue(_sepiaTone);
        }
    }
}