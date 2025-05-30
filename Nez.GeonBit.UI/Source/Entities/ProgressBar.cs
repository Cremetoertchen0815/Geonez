﻿#region File Description

//-----------------------------------------------------------------------------
// ProgressBar is a type of slider, but with a prograss bar graphics.
//
// It can be useful to show things like loading progress, HP left, XP needed
// until level up, etc.
//
// Please note:
// 1. By default, prograssbar are not locked and can be changed by the user.
//		To make the prograssbar locked (eg changeable only through code), use
//		the 'Locked' property.
// 2. The default color is greed, but you can easily change it via the
//		FillColor property.
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
///     A sub-class of the slider entity, with graphics more fitting for a progress bar or things like hp bar etc.
///     Behaves the same as a slider, if you want it to be for display only (and not changeable by user), simple set Locked
///     = true.
/// </summary>
[Serializable]
public class ProgressBar : Slider
{
    /// <summary>Default styling for progress bar. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default styling for the progress bar fill part. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultFillStyle = new();

    /// <summary>Default progressbar size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(0f, 52f);

    /// <summary>An optional caption to display over the center of the progress bar.</summary>
    public Label Caption;

    /// <summary>The fill part of the progress bar.</summary>
    public Image ProgressFill;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static ProgressBar()
    {
        MakeSerializable(typeof(ProgressBar));
    }

    /// <summary>
    ///     Create progress bar with size.
    /// </summary>
    /// <param name="min">Min value.</param>
    /// <param name="max">Max value.</param>
    /// <param name="size">Entity size.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public ProgressBar(uint min, uint max, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
        base(min, max, size, SliderSkin.Default, anchor, offset)
    {
        // update default styles
        UpdateStyle(DefaultStyle);

        if (!UserInterface.Active._isDeserializing)
        {
            // create the fill part
            Padding = Vector2.Zero;
            ProgressFill = new Image(Resources.ProgressBarFillTexture, Vector2.Zero, ImageDrawMode.Stretch,
                Anchor.CenterLeft);
            ProgressFill.UpdateStyle(DefaultFillStyle);
            ProgressFill._hiddenInternalEntity = true;
            ProgressFill.Identifier = "_progress_fill";
            AddChild(ProgressFill, true);

            // create caption on progressbar
            Caption = new Label(string.Empty, Anchor.Center)
            {
                ClickThrough = true,
                _hiddenInternalEntity = true,
                Identifier = "_progress_caption"
            };
            AddChild(Caption);
        }
    }

    /// <summary>
    ///     Create progress bar.
    /// </summary>
    /// <param name="min">Min value.</param>
    /// <param name="max">Max value.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public ProgressBar(uint min, uint max, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
        this(min, max, USE_DEFAULT_SIZE, anchor, offset)
    {
    }

    /// <summary>
    ///     Create progressbar with default params.
    /// </summary>
    public ProgressBar() : this(0, 10)
    {
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();
        Caption = Find<Label>("_progress_caption");
        Caption._hiddenInternalEntity = true;
        ProgressFill = Find<Image>("_progress_fill");
        ProgressFill._hiddenInternalEntity = true;
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // get progressbar frame width
        var progressbarFrameWidth = Resources.ProgressBarData.FrameWidth;

        // draw progress bar frame
        var barTexture = Resources.ProgressBarTexture;
        UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, barTexture, _destRect,
            new Vector2(progressbarFrameWidth, 0f), 1, FillColor);

        // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
        var frameSizeTexture = new Vector2(barTexture.Width * progressbarFrameWidth, barTexture.Height);
        var frameSizeRender = frameSizeTexture;
        var ScaleXfac = _destRect.Height / frameSizeRender.Y;

        // calc frame width in pixels
        _frameActualWidth = progressbarFrameWidth * barTexture.Width * ScaleXfac;

        // update the progress bar color and size
        var markWidth = (int)((_destRect.Width - _frameActualWidth * 2) * GetValueAsPercent());
        ProgressFill.SetOffset(new Vector2(_frameActualWidth / GlobalScale, 0));
        ProgressFill.Size = new Vector2(markWidth, _destRectInternal.Height) / GlobalScale;
        ProgressFill.Visible = markWidth > 0;
    }
}