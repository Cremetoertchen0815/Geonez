﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

/// <summary>
///     draws the sprite with just vertical or horizonal lines of the specified color. The effect works in screen space.
/// </summary>
public class SpriteLinesEffect : Effect
{
    private readonly EffectParameter _lineColorParam;
    private readonly EffectParameter _lineSizeParam;
    private bool _isVertical = true;

    private Vector4 _lineColor = new(1, 0, 0, 1);
    private float _lineSize = 5f;


    public SpriteLinesEffect() : base(Core.GraphicsDevice, EffectResource.SpriteLinesEffectBytes)
    {
        _lineColorParam = Parameters["_lineColor"];
        _lineSizeParam = Parameters["_lineSize"];

        _lineColorParam.SetValue(_lineColor);
        _lineSizeParam.SetValue(_lineSize);
    }

    /// <summary>
    ///     color of the lines. Defaults to red.
    /// </summary>
    /// <value>The color of the line.</value>
    public Color LineColor
    {
        get => new(_lineColor);
        set
        {
            var blinkVec = value.ToVector4();
            if (_lineColor != blinkVec)
            {
                _lineColor = blinkVec;
                _lineColorParam.SetValue(_lineColor);
            }
        }
    }

    /// <summary>
    ///     size of the lines in pixels. Defaults to 5.
    /// </summary>
    /// <value>The size of the line.</value>
    [Range(0, 100)]
    public float LineSize
    {
        get => _lineSize;
        set
        {
            if (_lineSize != value)
            {
                _lineSize = value;
                _lineSizeParam.SetValue(_lineSize);
            }
        }
    }

    /// <summary>
    ///     toggles vertical/horizontal lines
    /// </summary>
    /// <value><c>true</c> if is vertical; otherwise, <c>false</c>.</value>
    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            if (_isVertical != value)
            {
                _isVertical = value;
                CurrentTechnique = _isVertical ? Techniques["VerticalLines"] : Techniques["HorizontalLines"];
            }
        }
    }
}