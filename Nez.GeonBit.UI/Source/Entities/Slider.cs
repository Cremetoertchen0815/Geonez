﻿#region File Description

//-----------------------------------------------------------------------------
// Sliders are horizontal bars, similar to scrollbar, that lets user pick
// numeric values in pre-defined range.
// For example, a slider is often use to pick settings like volume, gamma, etc..
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     Different sliders skins (textures).
/// </summary>
public enum SliderSkin
{
    /// <summary>Default, thin slider skin.</summary>
    Default = 0,

    /// <summary>More fancy, thicker slider skin.</summary>
    Fancy = 1
}

/// <summary>
///     Slider entity looks like a horizontal scrollbar that the user can drag left and right to select a numeric value
///     from range.
/// </summary>
[Serializable]
public class Slider : Entity
{
    /// <summary>Default styling for the slider itself. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default slider size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(0f, 30f);

    /// <summary>Actual frame width in pixels (used internally).</summary>
    protected float _frameActualWidth;

    /// <summary>Actual mark width in pixels (used internally).</summary>
    protected int _markWidth = 20;

    /// <summary>
    ///     The mark on the slider
    /// </summary>
    private Rectangle _marRec;

    /// <summary>Max slider value.</summary>
    protected uint _max;

    /// <summary>Min slider value.</summary>
    protected uint _min;

    // slider style
    private SliderSkin _skin;

    /// <summary>How many steps (ticks) are in range.</summary>
    protected uint _stepsCount;

    /// <summary>Current value.</summary>
    protected int _value;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Slider()
    {
        MakeSerializable(typeof(Slider));
    }

    /// <summary>
    ///     Create the slider.
    /// </summary>
    /// <param name="min">Min value (inclusive).</param>
    /// <param name="max">Max value (inclusive).</param>
    /// <param name="size">Slider size.</param>
    /// <param name="skin">Slider skin (texture).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Slider(uint min, uint max, Vector2 size, SliderSkin skin = SliderSkin.Default, Anchor anchor = Anchor.Auto,
        Vector2? offset = null) :
        base(size, anchor, offset)
    {
        // store style
        _skin = skin;

        // store min and max and set default value
        Min = min;
        Max = max;

        // set default steps count
        _stepsCount = Max - Min;

        // set starting value to center
        _value = (int)(Min + (Max - Min) / 2);

        // update default style
        UpdateStyle(DefaultStyle);
    }

    /// <summary>
    ///     Create slider with default size.
    /// </summary>
    /// <param name="min">Min value (inclusive).</param>
    /// <param name="max">Max value (inclusive).</param>
    /// <param name="skin">Slider skin (texture).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Slider(uint min, uint max, SliderSkin skin = SliderSkin.Default, Anchor anchor = Anchor.Auto,
        Vector2? offset = null) :
        this(min, max, USE_DEFAULT_SIZE, skin, anchor, offset)
    {
    }

    /// <summary>
    ///     Create default slider.
    /// </summary>
    public Slider() : this(0, 10)
    {
    }

    public bool UseDottedDesign { get; set; }

    /// <summary>
    ///     Get the mark of the slider as a Rectangle
    /// </summary>
    public Rectangle MarkRec
    {
        private set => _marRec = value;
        get => _marRec;
    }

    /// <summary>
    ///     Get / set slider skin.
    /// </summary>
    public SliderSkin SliderSkin
    {
        get => _skin;
        set => _skin = value;
    }

    /// <summary>
    ///     Current slider value.
    /// </summary>
    public int Value
    {
        // get current value
        get => _value;

        // set new value
        set
        {
            var prevVal = _value;
            _value = NormalizeValue(value);
            if (prevVal != _value) DoOnValueChange();
        }
    }

    /// <summary>
    ///     Slider min value (inclusive).
    /// </summary>
    public uint Min
    {
        get => _min;
        set
        {
            if (_min != value)
            {
                _min = value;
                if (Value < _min) Value = (int)_min;
            }
        }
    }

    /// <summary>
    ///     Slider max value (inclusive).
    /// </summary>
    public uint Max
    {
        get => _max;
        set
        {
            if (_max != value)
            {
                _max = value;
                if (Value > _max) Value = (int)_max;
            }
        }
    }

    /// <summary>
    ///     How many steps (ticks) in slider range.
    /// </summary>
    public uint StepsCount
    {
        // get current steps count
        get => _stepsCount;

        // set steps count and call Value = Value to normalize current value to new steps count.
        set
        {
            _stepsCount = value;
            Value = Value;
        }
    }

    /// <summary>
    ///     Get the size of a single step.
    /// </summary>
    /// <returns>Size of a single step, eg how much value changes in a step.</returns>
    public int GetStepSize()
    {
        if (StepsCount > 0)
        {
            if (Max - Min == StepsCount) return 1;
            return (int)Math.Max((Max - Min) / StepsCount + 1, 2);
        }

        return 1;
    }

    /// <summary>
    ///     Normalize value to fit slider range and be multiply of steps size.
    /// </summary>
    /// <param name="value">Value to normalize.</param>
    /// <returns>Normalized value.</returns>
    protected int NormalizeValue(int value)
    {
        if (!UserInterface.Active._isDeserializing)
        {
            // round to steps
            float stepSize = GetStepSize();
            value = (int)(Math.Round((double)value / stepSize) * stepSize);

            // camp between min and max
            value = (int)Math.Min(Math.Max(value, Min), Max);
        }

        // return normalized value
        return value;
    }

    /// <summary>
    ///     Is the slider a natrually-interactable entity.
    /// </summary>
    /// <returns>True.</returns>
    public override bool IsNaturallyInteractable()
    {
        return true;
    }

    /// <summary>
    ///     Called every frame while mouse button is down over this entity.
    ///     The slider entity override this function to handle slider value change (eg slider mark dragging).
    /// </summary>
    protected override void DoWhileMouseDown()
    {
        // get mouse position and apply scroll value
        var mousePos = GetMousePos();
        mousePos += _lastScrollVal.ToVector2();

        // if mouse x is on the 0 side set to min
        if (mousePos.X <= _destRect.X + _frameActualWidth)
        {
            Value = (int)Min;
        }
        // else if mouse x is on the max side, set to max
        else if (mousePos.X >= _destRect.Right - _frameActualWidth)
        {
            Value = (int)Max;
        }
        // if in the middle calculate value based on mouse position
        else
        {
            var val = (mousePos.X - _destRect.X - _frameActualWidth + _markWidth / 2) /
                      (_destRect.Width - _frameActualWidth * 2);
            Value = (int)(Min + val * (Max - Min));
        }

        // call base handler
        base.DoWhileMouseDown();
    }

    /// <summary>
    ///     Return current value as a percent between min and max.
    /// </summary>
    /// <returns>Current value as percent between min and max (0f-1f).</returns>
    public float GetValueAsPercent()
    {
        return (_value - Min) / (float)(Max - Min);
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // get textures based on skin
        var texture = Resources.SliderTextures[_skin];
        var markTexture = Resources.SliderMarkTextures[_skin];


        // get slider metadata
        var data = Resources.SliderData[(int)_skin];
        var frameWidth = data.FrameWidth;

        // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
        var frameSizeTexture = new Vector2(texture.Width * frameWidth, texture.Height);
        var ScaleXfac = _destRect.Height / frameSizeTexture.Y;

        // calc the size of the mark piece
        var markHeight = _destRect.Height;
        _markWidth = (int)(markTexture.Width / (float)markTexture.Height * markHeight);

        // calc frame width in pixels
        _frameActualWidth = frameWidth * texture.Width * ScaleXfac;

        // draw slider body
        if (UseDottedDesign)
        {
            for (var i = Min; i <= Max; i++)
            {
                if (i == Value) continue;

                var posX = _destRect.X + _frameActualWidth + (_destRect.Width - _frameActualWidth * 2) *
                    (((float)i - Min) / ((float)Max - Min));

                MarkRec = new Rectangle((int)(Math.Round(posX) - frameSizeTexture.X / 2f),
                    _destRect.Y + (int)(frameSizeTexture.Y * 0.5f), (int)frameSizeTexture.X, (int)frameSizeTexture.Y);
                UserInterface.Active.DrawUtils.DrawImage(spriteBatch, texture, MarkRec, FillColor);
            }

            // now draw mark
            var perc = GetValueAsPercent();
            var markX = _destRect.X + _frameActualWidth + (_destRect.Width - _frameActualWidth * 2) * perc;

            MarkRec = new Rectangle((int)Math.Round(markX) - _markWidth / 2, _destRect.Y, _markWidth, markHeight);
            UserInterface.Active.DrawUtils.DrawImage(spriteBatch, markTexture, MarkRec, FillColor);
        }
        else
        {
            UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, new Vector2(frameWidth, 0f), 1,
                FillColor);

            // now draw mark
            var markX = _destRect.X + _frameActualWidth + _markWidth * 0.5f +
                        (_destRect.Width - _frameActualWidth * 2 - _markWidth) * GetValueAsPercent();

            MarkRec = new Rectangle((int)Math.Round(markX) - _markWidth / 2, _destRect.Y, _markWidth, markHeight);
            UserInterface.Active.DrawUtils.DrawImage(spriteBatch, markTexture, MarkRec, FillColor);
        }

        // call base draw function
        base.DrawEntity(spriteBatch, phase);
    }

    /// <summary>
    ///     Handle when mouse wheel scroll and this entity is the active entity.
    ///     Note: Slider entity override this function to change slider value based on wheel scroll.
    /// </summary>
    protected override void DoOnMouseWheelScroll()
    {
        Value = _value + Input.MouseWheelChange * GetStepSize();
    }

    /// <summary>
    ///     Handle when gamepad "A" button is pressed and left thumbstick moves.
    /// </summary>
    public void DoOnGamePadMove()
    {
        // In which direction should the mark move
        var stepDirection = 0;

        // stepDirection > 0 -> moves the mark to the right
        // stepDirection < 0 -> moves the mark to the left
        if (Input.ThumbStickLeftChange.X > 0 || Input.GamePadButtonHeldDown(GamePadButton.DPadRight) ||
            Input.GamePadButtonPressed(GamePadButton.DPadRight))
            stepDirection = 1;
        else if (Input.ThumbStickLeftChange.X < 0 || Input.GamePadButtonHeldDown(GamePadButton.DPadLeft) ||
                 Input.GamePadButtonPressed(GamePadButton.DPadLeft)) stepDirection = -1;

        var testValue = _value + stepDirection * GetStepSize();

        if (testValue <= Max && testValue >= 0)
            Value = testValue;
        else if (testValue <= Max)
            Value = (int)Max;
        else Value = 0;
    }
}