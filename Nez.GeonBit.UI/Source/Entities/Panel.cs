﻿#region File Description

//-----------------------------------------------------------------------------
// A panel is a surface you can add elements on. Its a graphical way to group
// together entities with a common logic.
// 
// Panels can have different styles (see PanelSkin for more info), or be
// invisible and just serve as an anchor and control group for its child
// entities.
//
// Usually you'd want to build your UI layout with panels. They are also very
// useful for things like message boxes etc.
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
///     How to treat entities that overflow panel boundaries.
/// </summary>
public enum PanelOverflowBehavior
{
	/// <summary>
	///     Entity will be rendered as usual outside the panel boundaries.
	/// </summary>
	Overflow,

	/// <summary>
	///     Entities that exceed panel boundaries will be clipped.
	///     Note: Requires render targets.
	/// </summary>
	Clipped,

	/// <summary>
	///     Entities that exceed panel on Y axis will create a scrollbar. Exceeding on X axis will be hidden.
	///     Note: Requires render targets.
	/// </summary>
	VerticalScroll
}

/// <summary>
///     A graphical panel or form you can create and add entities to.
///     Used to group together entities with common logic.
/// </summary>
[Serializable]
public class Panel : PanelBase
{
    /// <summary>Default styling for panels. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>
    ///     Store the original destination rectangle if changing due to render target.
    /// </summary>
    private Rectangle _originalInternalDestRect;

    // how the panel draw entities that exceed boundaries.
    private PanelOverflowBehavior _overflowMode = PanelOverflowBehavior.Overflow;

    /// <summary>If panel got scrollbars, use this render target to scroll.</summary>
    protected RenderTarget2D _renderTarget;

    /// <summary>
    ///     Panel scrollbar for specific overflow modes.
    /// </summary>
    protected VerticalScrollbar _scrollbar;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Panel()
    {
        MakeSerializable(typeof(Panel));
    }

    /// <summary>
    ///     Create the panel.
    /// </summary>
    /// <param name="size">Panel size.</param>
    /// <param name="skin">Panel skin (texture to use). Use PanelSkin.None for invisible panels.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Panel(Vector2 size, PanelSkin skin = PanelSkin.Default, Anchor anchor = Anchor.Center,
        Vector2? offset = null) :
        base(size, skin, anchor, offset)
    {
        UpdateStyle(DefaultStyle);

        if (skin is PanelSkin.None or PanelSkin.Simple)
        {
            OutlineColor = Color.Transparent;
            OutlineWidth = 0;
        }
    }

    /// <summary>
    ///     Create the panel with default params.
    /// </summary>
    public Panel() :
        this(new Vector2(500, 500))
    {
    }

    /// <summary>
    ///     Get the scrollbar of this panel.
    /// </summary>
    public VerticalScrollbar Scrollbar => _scrollbar;

    /// <summary>
    ///     Set / get panel overflow behavior.
    ///     Note: some modes require Render Targets, eg setting the 'UseRenderTarget' to true.
    /// </summary>
    public virtual PanelOverflowBehavior PanelOverflowBehavior
    {
        get => _overflowMode;
        set
        {
            _overflowMode = value;
            UpdateOverflowMode();
        }
    }

    /// <summary>
    ///     Get overflow scrollbar value.
    /// </summary>
    protected override Point OverflowScrollVal => _scrollbar == null ? Point.Zero : new Point(0, _scrollbar.Value);

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();
        var scrollbar = Find<VerticalScrollbar>("__scrollbar");
        if (scrollbar != null) RemoveChild(scrollbar);
        UpdateOverflowMode();
    }

    /// <summary>
    ///     Panel destructor.
    /// </summary>
    ~Panel()
    {
        Dispose();
    }

    /// <summary>
    ///     Dispose unmanaged resources related to this panel (render target).
    /// </summary>
    public void Dispose()
    {
        DisposeRenderTarget();
    }

    /// <summary>
    ///     Get the rectangle used for target texture for this panel.
    /// </summary>
    /// <returns>Destination rect for target texture.</returns>
    private Rectangle GetRenderTargetRect()
    {
        var ret = _destRectInternal;
        ret.Width += GetScrollbarWidth() * 2;
        return ret;
    }

    /// <summary>
    ///     Called before drawing child entities of this entity.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
    protected override void BeforeDrawChildren(SpriteBatch spriteBatch)
    {
        // if overflow mode is simply overflow, do nothing.
        if (_overflowMode == PanelOverflowBehavior.Overflow)
        {
            DisposeRenderTarget();
            return;
        }

        // create the render target for this panel
        var targetRect = GetRenderTargetRect();
        if (_renderTarget == null ||
            _renderTarget.Width != targetRect.Width ||
            _renderTarget.Height != targetRect.Height)
        {
            // recreate render target
            DisposeRenderTarget();
            _renderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice,
                targetRect.Width, targetRect.Height, false,
                spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat,
                spriteBatch.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0,
                RenderTargetUsage.PreserveContents);
        }

        // clear render target
        spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.GraphicsDevice.Clear(Color.Transparent);

        // bind the render target
        UserInterface.Active.DrawUtils.PushRenderTarget(_renderTarget);

        // set internal dest rect
        _originalInternalDestRect = _destRectInternal;
        _destRectInternal.X = 2;
        _destRectInternal.Y = 2;
        _destRectInternal.Width -= 2;
        _destRectInternal.Height -= 2;

        // if in scrolling mode, set scrollbar
        if (_overflowMode == PanelOverflowBehavior.VerticalScroll)
        {
            // move items position based on scrollbar
            _destRectInternal.Y -= _scrollbar.Value;

            // update scrollbar position
            _scrollbar.SetAnchor(Anchor.CenterLeft);
            _scrollbar.SetOffset(new Vector2(_destRectInternal.Width + 5, -_destRectInternal.Y) / GlobalScale);
            if (_scrollbar.Parent != null)
                _scrollbar.BringToFront();
            else
                AddChild(_scrollbar);
        }

        // to make sure the dest rect will not be recalculated while drawing children
        ClearDirtyFlag(true);
    }

    /// <summary>
    ///     Calculate and return the internal destination rectangle (note: this relay on the dest rect having a valid value
    ///     first).
    /// </summary>
    /// <returns>Internal destination rectangle.</returns>
    public override Rectangle CalcInternalRect()
    {
        base.CalcInternalRect();
        _destRectInternal.Width -= GetScrollbarWidth();
        return _destRectInternal;
    }

    /// <summary>
    ///     Get scrollbar width in pixels.
    /// </summary>
    /// <returns>Scrollbar width, or 0 if have no scrollbar.</returns>
    private int GetScrollbarWidth()
    {
        return _scrollbar != null ? _scrollbar.GetActualDestRect().Width : 0;
    }

    /// <summary>
    ///     Called after drawing child entities of this entity.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
    protected override void AfterDrawChildren(SpriteBatch spriteBatch)
    {
        // if overflow mode is simply overflow, do nothing.
        if (_overflowMode == PanelOverflowBehavior.Overflow) return;

        // return dest rect back to normal
        _destRectInternal = _originalInternalDestRect;
        _destRectVersion++;

        // if this panel got a render target
        if (_renderTarget != null)
        {
            // unbind the render target
            UserInterface.Active.DrawUtils.PopRenderTarget();

            // draw the render target itself
            UserInterface.Active.DrawUtils.StartDraw(spriteBatch, IsDisabled());
            spriteBatch.Draw(_renderTarget, GetRenderTargetRect(), Color.White);
            UserInterface.Active.DrawUtils.EndDraw(spriteBatch);

            // fix scrollbar positioning
            if (_scrollbar != null)
            {
                _destRectInternal.Y -= _scrollbar.Value;
                _destRectInternal.Width -= _scrollbar.GetActualDestRect().Width;
                _scrollbar.UpdateDestinationRects();

                // set destination rect back to normal
                _destRectInternal = _originalInternalDestRect;
            }
        }
    }

    /// <summary>
    ///     Called after a change in overflow mode.
    /// </summary>
    private void UpdateOverflowMode()
    {
        // if its vertical scroll mode:
        if (_overflowMode == PanelOverflowBehavior.VerticalScroll)
        {
            // if need to create scrollbar
            if (_scrollbar == null)
            {
                // create scrollbar
                _scrollbar = new VerticalScrollbar(0, 0, Anchor.TopRight)
                {
                    Padding = Vector2.Zero,
                    AdjustMaxAutomatically = true,
                    Identifier = "__scrollbar",
                    _hiddenInternalEntity = true
                };
                var prev_needToSortChildren = _needToSortChildren;
                AddChild(_scrollbar);
                _needToSortChildren = prev_needToSortChildren;
            }
        }
        // if its not vertical scroll but we have scrollbar, remove it
        else
        {
            if (_scrollbar != null) _scrollbar.RemoveFromParent();
        }
    }

    /// <summary>
    ///     Dispose the render target (only if use) and set it to null.
    /// </summary>
    private void DisposeRenderTarget()
    {
        if (_renderTarget != null)
        {
            _renderTarget.Dispose();
            _renderTarget = null;
        }
    }

    /// <summary>
    ///     Called every frame to update the children of this entity.
    /// </summary>
    /// <param name="targetEntity">The deepest child entity with highest priority that we point on and can be interacted with.</param>
    /// <param name="dragTargetEntity">
    ///     The deepest child dragable entity with highest priority that we point on and can be drag
    ///     if mouse down.
    /// </param>
    /// <param name="wasEventHandled">Set to true if current event was already handled by a deeper child.</param>
    /// <param name="scrollVal">Combined scrolling value (panels with scrollbar etc) of all parents.</param>
    protected override void UpdateChildren(ref Entity targetEntity, ref Entity dragTargetEntity,
        ref bool wasEventHandled, Point scrollVal)
    {
        // if not in overflow mode and mouse not on this panel boundaries, skip calling children
        var skipChildren = false;
        if (_overflowMode != PanelOverflowBehavior.Overflow)
        {
            var mousePos = GetMousePos();
            if (mousePos.X < _destRectInternal.Left || mousePos.X > _destRectInternal.Right ||
                mousePos.Y < _destRectInternal.Top || mousePos.Y > _destRectInternal.Bottom)
                skipChildren = true;
        }

        // before updating children, disable scrollbar
        if (_scrollbar != null) _scrollbar.Enabled = false;

        // call base update children function
        if (!skipChildren) base.UpdateChildren(ref targetEntity, ref dragTargetEntity, ref wasEventHandled, scrollVal);

        // re-enable scrollbar and update it
        if (_scrollbar != null)
        {
            _scrollbar.Enabled = true;
            _scrollbar.Update(ref targetEntity, ref dragTargetEntity, ref wasEventHandled,
                scrollVal - OverflowScrollVal);
        }
    }
}