using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class SimpleColorGradePostProcessor : PostProcessor
{
    private readonly EffectParameter _paramBrightness;
    private readonly EffectParameter _paramContrast;
    private readonly EffectParameter _paramGamma;
    private readonly EffectParameter _paramSaturation;
    private float _brightness;
    private float _contrast = 1f;
    private float _gamma = 1f;
    private float _saturation = 1f;

    public SimpleColorGradePostProcessor(int executionOrder) : base(executionOrder)
    {
        Effect = Core.Content.LoadEffect<Effect>("SimpleColorGrade", EffectResource.SimpleColorGrade);
        var parms = Effect.Parameters;
        _paramBrightness = parms["Brightness"];
        _paramContrast = parms["Contrast"];
        _paramSaturation = parms["Saturation"];
        _paramGamma = parms["Gamma"];
        _paramBrightness?.SetValue(_brightness);
        _paramContrast?.SetValue(_contrast);
        _paramSaturation?.SetValue(_saturation);
        _paramGamma?.SetValue(_gamma);
    }

    public float Brightness
    {
        get => _brightness;
        set
        {
            _brightness = value;
            _paramBrightness.SetValue(value);
        }
    }

    public float Contrast
    {
        get => _contrast;
        set
        {
            _contrast = value;
            _paramContrast.SetValue(value);
        }
    }

    public float Saturation
    {
        get => _saturation;
        set
        {
            _saturation = value;
            _paramSaturation.SetValue(value);
        }
    }

    public float Gamma
    {
        get => _gamma;
        set
        {
            _gamma = value;
            _paramGamma.SetValue(value);
        }
    }
}