﻿using System;
using Microsoft.Xna.Framework;

namespace Nez.UI;

public class Element : ILayout
{
    protected bool _debug;
    protected bool _layoutEnabled = true;

    protected bool _needsLayout = true;
    protected Stage _stage;
    protected bool _visible = true;
    internal Color color = Color.White;

    protected float originX, originY;
    internal Group parent;
    protected float rotation;
    protected float scaleX = 1, scaleY = 1;
    protected Touchable touchable = Touchable.Enabled;

    /// <summary>
    ///     use this to stuff any relevant data required for your UI setup
    /// </summary>
    public object UserData;

    internal float width, height;

    internal float x, y;

    /// <summary>
    ///     true if the widget's layout has been {@link #invalidate() invalidated}.
    /// </summary>
    /// <value><c>true</c> if needs layout; otherwise, <c>false</c>.</value>
    public bool NeedsLayout => _needsLayout;


    /// <summary>
    ///     If this method is overridden, the super method or {@link #validate()} should be called to ensure the widget is laid
    ///     out.
    /// </summary>
    /// <param name="batcher">Batcher.</param>
    /// <param name="parentAlpha">Parent alpha.</param>
    public virtual void Draw(Batcher batcher, float parentAlpha)
    {
        Validate();
    }

    protected virtual void SizeChanged()
    {
        Invalidate();
    }

    protected virtual void PositionChanged()
    {
    }

    protected virtual void RotationChanged()
    {
    }


    /// <summary>
    ///     returns the distance from point to the bounds of element in the largest dimension or a negative number if the point
    ///     is inside the bounds.
    ///     Note that point should be in the element's coordinate system already.
    /// </summary>
    /// <returns>The outside bounds to point.</returns>
    /// <param name="Point">Point.</param>
    protected float DistanceOutsideBoundsToPoint(Vector2 point)
    {
        var offsetX = Math.Max(-point.X, point.X - width);
        var offsetY = Math.Max(-point.Y, point.Y - height);

        return Math.Max(offsetX, offsetY);
    }

    /// <summary>
    ///     Draws this element's debug lines
    /// </summary>
    /// <param name="batcher">Batcher.</param>
    public virtual void DebugRender(Batcher batcher)
    {
        if (_debug)
            batcher.DrawHollowRect(x, y, width, height, Color.Red);
    }

    /// <summary>
    ///     returns true if this Element and all parent Elements are visible
    /// </summary>
    /// <returns><c>true</c>, if parents visible was ared, <c>false</c> otherwise.</returns>
    private bool AreParentsVisible()
    {
        if (!_visible)
            return false;

        if (parent != null)
            return parent.AreParentsVisible();

        return _visible;
    }

    public virtual Element Hit(Vector2 point)
    {
        // if we are not Touchable or us or any parent is not visible bail out
        if (touchable != Touchable.Enabled || !AreParentsVisible())
            return null;

        if (point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height)
            return this;

        return null;
    }

    /// <summary>
    ///     Removes this element from its parent, if it has a parent
    /// </summary>
    public bool Remove()
    {
        if (parent != null)
            return parent.RemoveElement(this);

        return false;
    }


    #region Getters/Setters

    /// <summary>
    ///     Returns the stage that this element is currently in, or null if not in a stage.
    /// </summary>
    /// <returns>The stage.</returns>
    public Stage GetStage()
    {
        return _stage;
    }

    /// <summary>
    ///     Called by the framework when this element or any parent is added to a group that is in the stage.
    ///     stage May be null if the element or any parent is no longer in a stage
    /// </summary>
    /// <param name="stage">Stage.</param>
    internal virtual void SetStage(Stage stage)
    {
        _stage = stage;
    }

    /// <summary>
    ///     Returns true if the element's parent is not null
    /// </summary>
    /// <returns><c>true</c>, if parent was hased, <c>false</c> otherwise.</returns>
    public bool HasParent()
    {
        return parent != null;
    }

    /// <summary>
    ///     Returns the parent element, or null if not in a group
    /// </summary>
    /// <returns>The parent.</returns>
    public Group GetParent()
    {
        return parent;
    }

    /// <summary>
    ///     Called by the framework when an element is added to or removed from a group.
    /// </summary>
    /// <param name="newParent">parent May be null if the element has been removed from the parent</param>
    internal void SetParent(Group newParent)
    {
        parent = newParent;
    }

    /// <summary>
    ///     Returns true if input events are processed by this element.
    /// </summary>
    /// <returns>The touchable.</returns>
    public bool IsTouchable()
    {
        return touchable == Touchable.Enabled;
    }

    public Touchable GetTouchable()
    {
        return touchable;
    }

    /// <summary>
    ///     Determines how touch events are distributed to this element. Default is {@link Touchable#enabled}.
    /// </summary>
    /// <param name="touchable">Touchable.</param>
    public void SetTouchable(Touchable touchable)
    {
        this.touchable = touchable;
    }

    public void SetIsVisible(bool visible)
    {
        _visible = visible;
    }

    public bool IsVisible()
    {
        return _visible;
    }

    /// <summary>
    ///     If false, the element will not be drawn and will not receive touch events. Default is true.
    /// </summary>
    /// <param name="visible">Visible.</param>
    public void SetVisible(bool visible)
    {
        _visible = visible;
    }

    /// <summary>
    ///     Returns the X position of the element's left edge
    /// </summary>
    /// <returns>The x.</returns>
    public float GetX()
    {
        return x;
    }

    /// <summary>
    ///     Returns the X position of the specified {@link Align alignment}.
    /// </summary>
    /// <returns>The x.</returns>
    /// <param name="alignment">Alignment.</param>
    public float GetX(int alignment)
    {
        var x = this.x;
        if ((alignment & AlignInternal.Right) != 0)
            x += width;
        else if ((alignment & AlignInternal.Left) == 0)
            x += width / 2;
        return x;
    }

    public Element SetX(float x)
    {
        if (this.x != x)
        {
            this.x = x;
            PositionChanged();
        }

        return this;
    }

    /// <summary>
    ///     Returns the Y position of the element's bottom edge
    /// </summary>
    /// <returns>The y.</returns>
    public float GetY()
    {
        return y;
    }

    /// <summary>
    ///     Returns the Y position of the specified {@link Align alignment}
    /// </summary>
    /// <returns>The y.</returns>
    /// <param name="alignment">Alignment.</param>
    public float GetY(int alignment)
    {
        var y = this.y;
        if ((alignment & AlignInternal.Bottom) != 0)
            y += height;
        else if ((alignment & AlignInternal.Top) == 0)
            y += height / 2;
        return y;
    }

    public Element SetY(float y)
    {
        if (this.y != y)
        {
            this.y = y;
            PositionChanged();
        }

        return this;
    }

    /// <summary>
    ///     Sets the position of the element's bottom left corner
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Element SetPosition(float x, float y)
    {
        if (this.x != x || this.y != y)
        {
            this.x = x;
            this.y = y;
            PositionChanged();
        }

        return this;
    }

    /// <summary>
    ///     Sets the position using the specified {@link Align alignment}. Note this may set the position to non-integer
    ///     coordinates
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="alignment">Alignment.</param>
    public void SetPosition(float x, float y, int alignment)
    {
        if ((alignment & AlignInternal.Right) != 0)
            x -= width;
        else if ((alignment & AlignInternal.Left) == 0) //
            x -= width / 2;

        if ((alignment & AlignInternal.Top) != 0)
            y -= height;
        else if ((alignment & AlignInternal.Bottom) == 0) //
            y -= height / 2;

        if (this.x != x || this.y != y)
        {
            this.x = x;
            this.y = y;
            PositionChanged();
        }
    }

    /// <summary>
    ///     Add x and y to current position
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public void MoveBy(float x, float y)
    {
        if (x != 0 || y != 0)
        {
            this.x += x;
            this.y += y;
            PositionChanged();
        }
    }

    public float GetWidth()
    {
        return width;
    }


    public void SetWidth(float width)
    {
        if (this.width != width)
        {
            this.width = width;
            SizeChanged();
        }
    }

    public float GetHeight()
    {
        return height;
    }

    public void SetHeight(float height)
    {
        if (this.height != height)
        {
            this.height = height;
            SizeChanged();
        }
    }

    public void SetSize(float width, float height)
    {
        if (this.width == width && this.height == height)
            return;

        this.width = width;
        this.height = height;
        SizeChanged();
    }


    /// <summary>
    ///     Returns y plus height
    /// </summary>
    /// <returns>The top.</returns>
    public float GetBottom()
    {
        return y + height;
    }


    /// <summary>
    ///     Returns x plus width
    /// </summary>
    /// <returns>The right.</returns>
    public float GetRight()
    {
        return x + width;
    }

    /// <summary>
    ///     Sets the x, y, width, and height.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    public void SetBounds(float x, float y, float width, float height)
    {
        if (this.x != x || this.y != y)
        {
            this.x = x;
            this.y = y;
            PositionChanged();
        }

        if (this.width != width || this.height != height)
        {
            this.width = width;
            this.height = height;
            SizeChanged();
        }
    }

    public float GetOriginX()
    {
        return originX;
    }

    public void SetOriginX(float originX)
    {
        this.originX = originX;
    }

    public float GetOriginY()
    {
        return originY;
    }

    public void SetOriginY(float originY)
    {
        this.originY = originY;
    }

    /// <summary>
    ///     Sets the origin position which is relative to the element's bottom left corner
    /// </summary>
    /// <param name="originX">Origin x.</param>
    /// <param name="originY">Origin y.</param>
    public void SetOrigin(float originX, float originY)
    {
        this.originX = originX;
        this.originY = originY;
    }

    /// <summary>
    ///     Sets the origin position to the specified {@link Align alignment}.
    /// </summary>
    /// <param name="alignment">Alignment.</param>
    public void SetOrigin(int alignment)
    {
        if ((alignment & AlignInternal.Left) != 0)
            originX = 0;
        else if ((alignment & AlignInternal.Right) != 0)
            originX = width;
        else
            originX = width / 2;

        if ((alignment & AlignInternal.Top) != 0)
            originY = 0;
        else if ((alignment & AlignInternal.Bottom) != 0)
            originY = height;
        else
            originY = height / 2;
    }

    public float GetScaleX()
    {
        return scaleX;
    }

    public void SetScaleX(float scaleX)
    {
        this.scaleX = scaleX;
    }

    public float GetScaleY()
    {
        return scaleY;
    }

    public void SetScaleY(float scaleY)
    {
        this.scaleY = scaleY;
    }

    /// <summary>
    ///     Sets the scale for both X and Y
    /// </summary>
    /// <param name="scaleXY">Scale X.</param>
    public void SetScale(float scaleXY)
    {
        scaleX = scaleXY;
        scaleY = scaleXY;
    }

    /// <summary>
    ///     Sets the scale X and scale Y
    /// </summary>
    /// <param name="scaleX">Scale x.</param>
    /// <param name="scaleY">Scale y.</param>
    public void SetScale(float scaleX, float scaleY)
    {
        this.scaleX = scaleX;
        this.scaleY = scaleY;
    }

    /// <summary>
    ///     Adds the specified scale to the current scale
    /// </summary>
    /// <param name="scale">Scale.</param>
    public void ScaleBy(float scale)
    {
        scaleX += scale;
        scaleY += scale;
    }

    /// <summary>
    ///     Adds the specified scale to the current scale
    /// </summary>
    /// <param name="scaleX">Scale x.</param>
    /// <param name="scaleY">Scale y.</param>
    public void ScaleBy(float scaleX, float scaleY)
    {
        this.scaleX += scaleX;
        this.scaleY += scaleY;
    }

    public float GetRotation()
    {
        return rotation;
    }

    public void SetRotation(float degrees)
    {
        if (rotation != degrees)
        {
            rotation = degrees;
            RotationChanged();
        }
    }

    /// <summary>
    ///     Adds the specified rotation to the current rotation
    /// </summary>
    /// <param name="amountInDegrees">Amount in degrees.</param>
    public void RotateBy(float amountInDegrees)
    {
        if (amountInDegrees != 0)
        {
            rotation += amountInDegrees;
            RotationChanged();
        }
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    /// <summary>
    ///     Returns the color the element will be tinted when drawn
    /// </summary>
    /// <returns>The color.</returns>
    public Color GetColor()
    {
        return color;
    }

    /// <summary>
    ///     Changes the z-order for this element so it is in front of all siblings
    /// </summary>
    public void ToFront()
    {
        SetZIndex(int.MaxValue);
    }

    /// <summary>
    ///     Changes the z-order for this element so it is in back of all siblings
    /// </summary>
    public void ToBack()
    {
        SetZIndex(0);
    }

    /// <summary>
    ///     Sets the z-index of this element. The z-index is the index into the parent's {@link Group#getChildren() children},
    ///     where a
    ///     lower index is below a higher index. Setting a z-index higher than the number of children will move the child to
    ///     the front.
    ///     Setting a z-index less than zero is invalid.
    /// </summary>
    /// <param name="index">Index.</param>
    public void SetZIndex(int index)
    {
        var parent = this.parent;
        if (parent == null)
            return;

        var children = parent.children;
        if (children.Count == 1)
            return;

        index = Math.Min(index, children.Count - 1);
        if (index == children.IndexOf(this))
            return;

        if (!children.Remove(this))
            return;

        children.Insert(index, this);
    }

    /// <summary>
    ///     Calls clipBegin(Batcher, float, float, float, float) to clip this actor's bounds
    /// </summary>
    /// <returns>The begin.</returns>
    public bool ClipBegin(Batcher batcher)
    {
        return ClipBegin(batcher, x, y, width, height);
    }

    /// <summary>
    ///     Clips the specified screen aligned rectangle, specified relative to the transform matrix of the stage's Batch. The
    ///     transform matrix and the stage's camera must not have rotational components. Calling this method must be followed
    ///     by a call
    ///     to clipEnd() if true is returned.
    /// </summary>
    public bool ClipBegin(Batcher batcher, float x, float y, float width, float height)
    {
        if (width <= 0 || height <= 0)
            return false;

        var tableBounds = RectangleExt.FromFloats(x, y, width, height);
        var scissorBounds =
            ScissorStack.CalculateScissors(_stage?.Entity?.Scene?.Camera, batcher.TransformMatrix, tableBounds);
        if (ScissorStack.PushScissors(scissorBounds))
        {
            batcher.EnableScissorTest(true);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Ends clipping begun by clipBegin(Batcher, float, float, float, float)
    /// </summary>
    /// <returns>The end.</returns>
    public void ClipEnd(Batcher batcher)
    {
        batcher.EnableScissorTest(false);
        ScissorStack.PopScissors();
    }

    /// <summary>
    ///     If true, {@link #debugDraw} will be called for this element
    /// </summary>
    /// <param name="enabled">Enabled.</param>
    public virtual void SetDebug(bool enabled)
    {
        _debug = enabled;
        if (enabled)
            Stage.Debug = true;
    }

    public bool GetDebug()
    {
        return _debug;
    }

    #endregion


    #region Coordinate conversion

    /// <summary>
    ///     Transforms the specified point in screen coordinates to the element's local coordinate system
    /// </summary>
    /// <returns>The to local coordinates.</returns>
    /// <param name="screenCoords">Screen coords.</param>
    public Vector2 ScreenToLocalCoordinates(Vector2 screenCoords)
    {
        if (_stage == null)
            return screenCoords;

        return StageToLocalCoordinates(_stage.ScreenToStageCoordinates(screenCoords));
    }

    /// <summary>
    ///     Transforms the specified point in the stage's coordinates to the element's local coordinate system.
    /// </summary>
    /// <returns>The to local coordinates.</returns>
    /// <param name="stageCoords">Stage coords.</param>
    public Vector2 StageToLocalCoordinates(Vector2 stageCoords)
    {
        if (parent != null)
            stageCoords = parent.StageToLocalCoordinates(stageCoords);

        stageCoords = ParentToLocalCoordinates(stageCoords);
        return stageCoords;
    }

    /// <summary>
    ///     Transforms the specified point in the element's coordinates to be in the stage's coordinates
    /// </summary>
    /// <returns>The to stage coordinates.</returns>
    /// <param name="localCoords">Local coords.</param>
    public Vector2 LocalToStageCoordinates(Vector2 localCoords)
    {
        return LocalToAscendantCoordinates(null, localCoords);
    }

    /// <summary>
    ///     Converts coordinates for this element to those of a parent element. The ascendant does not need to be a direct
    ///     parent
    /// </summary>
    /// <returns>The to ascendant coordinates.</returns>
    /// <param name="ascendant">Ascendant.</param>
    /// <param name="localCoords">Local coords.</param>
    public Vector2 LocalToAscendantCoordinates(Element ascendant, Vector2 localCoords)
    {
        var element = this;
        while (element != null)
        {
            localCoords = element.LocalToParentCoordinates(localCoords);
            element = element.parent;
            if (element == ascendant)
                break;
        }

        return localCoords;
    }

    /// <summary>
    ///     Converts the coordinates given in the parent's coordinate system to this element's coordinate system.
    /// </summary>
    /// <returns>The to local coordinates.</returns>
    /// <param name="parentCoords">Parent coords.</param>
    public Vector2 ParentToLocalCoordinates(Vector2 parentCoords)
    {
        if (rotation == 0)
        {
            if (scaleX == 1 && scaleY == 1)
            {
                parentCoords.X -= x;
                parentCoords.Y -= y;
            }
            else
            {
                parentCoords.X = (parentCoords.X - x - originX) / scaleX + originX;
                parentCoords.Y = (parentCoords.Y - y - originY) / scaleY + originY;
            }
        }
        else
        {
            var cos = Mathf.Cos(MathHelper.ToRadians(rotation));
            var sin = Mathf.Sin(MathHelper.ToRadians(rotation));
            var tox = parentCoords.X - x - originX;
            var toy = parentCoords.Y - y - originY;
            parentCoords.X = (tox * cos + toy * sin) / scaleX + originX;
            parentCoords.Y = (tox * -sin + toy * cos) / scaleY + originY;
        }

        return parentCoords;
    }

    /// <summary>
    ///     Transforms the specified point in the element's coordinates to be in the parent's coordinates.
    /// </summary>
    /// <returns>The to parent coordinates.</returns>
    /// <param name="localCoords">Local coords.</param>
    public Vector2 LocalToParentCoordinates(Vector2 localCoords)
    {
        var rotation = -this.rotation;

        if (rotation == 0)
        {
            if (scaleX == 1 && scaleY == 1)
            {
                localCoords.X += x;
                localCoords.Y += y;
            }
            else
            {
                localCoords.X = (localCoords.X - originX) * scaleX + originX + x;
                localCoords.Y = (localCoords.Y - originY) * scaleY + originY + y;
            }
        }
        else
        {
            var cos = Mathf.Cos(MathHelper.ToRadians(rotation));
            var sin = Mathf.Sin(MathHelper.ToRadians(rotation));

            var tox = (localCoords.X - originX) * scaleX;
            var toy = (localCoords.Y - originY) * scaleY;
            localCoords.X = tox * cos + toy * sin + originX + x;
            localCoords.Y = tox * -sin + toy * cos + originY + y;
        }

        return localCoords;
    }

    #endregion


    #region ILayout

    public bool FillParent { get; set; }

    public virtual bool LayoutEnabled
    {
        get => _layoutEnabled;
        set
        {
            if (_layoutEnabled != value)
            {
                _layoutEnabled = value;

                if (_layoutEnabled)
                    InvalidateHierarchy();
            }
        }
    }

    public virtual float MinWidth => PreferredWidth;

    public virtual float MinHeight => PreferredHeight;

    public virtual float PreferredWidth => 0;

    public virtual float PreferredHeight => 0;

    public virtual float MaxWidth => 0;

    public virtual float MaxHeight => 0;

    public virtual void Layout()
    {
    }

    public virtual void Invalidate()
    {
        _needsLayout = true;
    }

    public virtual void InvalidateHierarchy()
    {
        if (!_layoutEnabled)
            return;

        Invalidate();

        if (parent is ILayout)
            ((ILayout)parent).InvalidateHierarchy();
    }

    public void Validate()
    {
        if (!_layoutEnabled)
            return;

        if (FillParent && parent != null)
        {
            var stage = GetStage();
            float parentWidth, parentHeight;

            if (stage != null && parent == stage.GetRoot())
            {
                parentWidth = stage.GetWidth();
                parentHeight = stage.GetHeight();
            }
            else
            {
                parentWidth = parent.GetWidth();
                parentHeight = parent.GetHeight();
            }

            if (width != parentWidth || height != parentHeight)
            {
                SetSize(parentWidth, parentHeight);
                Invalidate();
            }
        }

        if (!_needsLayout)
            return;

        _needsLayout = false;
        Layout();
    }

    public virtual void Pack()
    {
        SetSize(PreferredWidth, PreferredHeight);
        Validate();
    }

    #endregion
}