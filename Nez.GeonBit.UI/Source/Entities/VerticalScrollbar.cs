﻿#region File Description

//-----------------------------------------------------------------------------
// Vertical scrollbar is used internally to scroll through lists etc.
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
///     Used internally as a scrollbar for lists, text boxes, etc..
/// </summary>
public class VerticalScrollbar : Slider
{
    /// <summary>Default scrollbar size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(30f, 0f);

    /// <summary>Default styling for vertical scrollbars. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    // frame and mark actual height
    private float _frameActualHeight;
    private int _markHeight = 20;

    /// <summary>
    ///     If true, will adjust max value automatically based on entities in parent.
    /// </summary>
    public bool AdjustMaxAutomatically;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static VerticalScrollbar()
    {
        MakeSerializable(typeof(VerticalScrollbar));
    }

    /// <summary>
    ///     Create the scrollbar.
    /// </summary>
    /// <param name="min">Min scrollbar value.</param>
    /// <param name="max">Max scrollbar value.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="adjustMaxAutomatically">
    ///     If true, the scrollbar will set its max value automatically based on entities in
    ///     its parent.
    /// </param>
    public VerticalScrollbar(uint min, uint max, Anchor anchor = Anchor.Auto, Vector2? offset = null,
        bool adjustMaxAutomatically = false) :
        base(0, 0, USE_DEFAULT_SIZE, SliderSkin.Default, anchor, offset)
    {
        // set this scrollbar to respond even when direct parent is locked
        DoEventsIfDirectParentIsLocked = true;

        // set if need to adjust max automatically
        AdjustMaxAutomatically = adjustMaxAutomatically;

        // update default style
        UpdateStyle(DefaultStyle);
    }

    /// <summary>
    ///     Create vertical scroll with default params.
    /// </summary>
    public VerticalScrollbar() : this(0, 10)
    {
    }

    /// <summary>
    ///     Handle mouse down event.
    ///     The Scrollbar entity override this function to handle sliding mark up and down, instead of left-right.
    /// </summary>
    protected override void DoOnMouseReleased()
    {
        // get mouse position and apply scroll value
        var mousePos = GetMousePos(_lastScrollVal.ToVector2());

        // if mouse is on the min side, decrease by 1
        if (mousePos.Y <= _destRect.Y + _frameActualHeight)
            Value = _value - GetStepSize();
        // else if mouse is on the max side, increase by 1
        else if (mousePos.Y >= _destRect.Bottom - _frameActualHeight) Value = _value + GetStepSize();

        // call base function
        base.DoOnMouseReleased();
    }

    /// <summary>
    ///     Handle while mouse is down event.
    ///     The Scrollbar entity override this function to handle sliding mark up and down, instead of left-right.
    /// </summary>
    protected override void DoWhileMouseDown()
    {
        // get mouse position and apply scroll value
        var mousePos = GetMousePos(_lastScrollVal.ToVector2());

        // if in the middle calculate value based on mouse position
        if (mousePos.Y >= _destRect.Y + _frameActualHeight * 0.5 &&
            mousePos.Y <= _destRect.Bottom - _frameActualHeight * 0.5)
        {
            var relativePos = mousePos.Y - _destRect.Y - _frameActualHeight * 0.5f - _markHeight * 0.5f;
            var internalHeight = _destRect.Height - _frameActualHeight - _markHeight * 0.5f;
            var relativeVal = relativePos / internalHeight;
            Value = (int)Math.Round(Min + relativeVal * (Max - Min));
        }

        // call event handler
        WhileMouseDown?.Invoke(this);
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // if needed, recalc max (but not if currently interacting with this object).
        if (UserInterface.Active.ActiveEntity != this) CalcAutoMaxValue();

        // get textures based on type
        var texture = Resources.VerticalScrollbarTexture;
        var markTexture = Resources.VerticalScrollbarMarkTexture;
        var FrameHeight = Resources.VerticalScrollbarData.FrameHeight;

        // draw scrollbar body
        UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, new Vector2(0f, FrameHeight), 1,
            FillColor);

        // calc frame actual height and scaling factor (this is needed to calc frame width in pixels)
        var frameSizeTexture = new Vector2(texture.Width, texture.Height * FrameHeight);
        var frameSizeRender = frameSizeTexture;
        var ScaleYfac = _destRect.Width / frameSizeRender.X;

        // calc the size of the mark piece
        var markWidth = _destRect.Width;
        _markHeight = (int)(markTexture.Height / (float)markTexture.Width * markWidth);

        // calc frame width in pixels
        _frameActualHeight = FrameHeight * texture.Height * ScaleYfac;

        // now draw mark
        var markY = _destRect.Y + _frameActualHeight + _markHeight * 0.5f +
                    (_destRect.Height - _frameActualHeight * 2 - _markHeight) * GetValueAsPercent();
        var markDest = new Rectangle(_destRect.X, (int)Math.Round(markY) - _markHeight / 2, markWidth, _markHeight);
        UserInterface.Active.DrawUtils.DrawImage(spriteBatch, markTexture, markDest, FillColor);
    }

    /// <summary>
    ///     Called every frame after update.
    ///     Scrollbar override this function to handle wheel scroll while pointing on parent entity - we still want to capture
    ///     that.
    /// </summary>
    protected override void DoAfterUpdate()
    {
        // if the active entity is self or parent, listen to mousewheel
        if (_isInteractable &&
            (UserInterface.Active.ActiveEntity == this ||
             UserInterface.Active.ActiveEntity == _parent ||
             (UserInterface.Active.ActiveEntity != null && UserInterface.Active.ActiveEntity.IsDeepChildOf(_parent))))
            if (Input.MouseWheelChange != 0)
                Value = _value - Input.MouseWheelChange * GetStepSize();
    }

    /// <summary>
    ///     Calculate max value based on siblings (note: only if AdjustMaxAutomatically is true)
    /// </summary>
    private void CalcAutoMaxValue()
    {
        // if need to adjust max automatically
        if (AdjustMaxAutomatically)
        {
            // get parent top
            var newMax = 0;
            var parentTop = Parent.InternalDestRect.Y;

            // iterate parent children to get the most bottom child
            foreach (var child in Parent.Children)
            {
                // skip self
                if (child == this) continue;

                // skip internals
                if (child._hiddenInternalEntity) continue;

                // get current child bottom
                var bottom = child.GetActualDestRect().Bottom;

                // calc new max value
                var currNewMax = bottom - parentTop;
                newMax = Math.Max(newMax, currNewMax);
            }

            // remove parent size from result (the -4 is to give extra pixels down)
            newMax -= Parent.InternalDestRect.Height - 4;
            newMax = Math.Max(newMax, 0);

            // set new max value
            if (newMax != Max) Max = (uint)newMax;

            // set steps count
            StepsCount = (Max - Min) / 80;
        }
    }

    /// <summary>
    ///     Handle when mouse wheel scroll and this entity is the active entity.
    ///     Note: Scrollbar entity override this function to change scrollbar value based on wheel scroll, which is inverted.
    /// </summary>
    protected override void DoOnMouseWheelScroll()
    {
        Value = _value - Input.MouseWheelChange * GetStepSize();
    }
}