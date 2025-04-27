#region File Description

//-----------------------------------------------------------------------------
// Base UI entity. Every widget inherit from this class.
// The base entity implement the following key functionality:
// 1. Drawing basic stuff like tiled textures with frames etc.
// 2. Positioning and calculating destination rect.
// 3. Basic events.
// 4. Visibility / Disabled / Locked modes.
// 5. Managing child entities.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ExtendedContent.DataTypes;
using Nez.GeonBit.UI.Exceptions;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     GeonBit.UI.Entities contains all the UI elements you can create and use in your layouts.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     Different draw phases of the entity.
/// </summary>
public enum DrawPhase
{
	/// <summary>
	///     Drawing the entity itself.
	/// </summary>
	Base,

	/// <summary>
	///     Drawing entity outline.
	/// </summary>
	Outline,

	/// <summary>
	///     Drawing entity's shadow.
	/// </summary>
	Shadow
}

/// <summary>
///     Static strings with all common style property names, to reduce string creations.
/// </summary>
internal static class StylePropertyIds
{
    public static readonly string SpaceAfter = "SpaceAfter";
    public static readonly string SpaceBefore = "SpaceBefore";
    public static readonly string FillColor = "FillColor";
    public static readonly string Scale = "Scale";
    public static readonly string Padding = "Padding";
    public static readonly string ShadowColor = "ShadowColor";
    public static readonly string ShadowScale = "ShadowScale";
    public static readonly string ShadowOffset = "ShadowOffset";
    public static readonly string OutlineColor = "OutlineColor";
    public static readonly string OutlineWidth = "OutlineWidth";
}

/// <summary>
///     An Anchor is a pre-defined position in parent entity that we use to position a child.
///     For eample, we can use anchors to position an entity at the bottom-center point of its parent.
///     Note: anchor affect both the position relative to parent and also the offset origin point of the entity.
/// </summary>
public enum Anchor
{
    /// <summary>Center of parent element.</summary>
    Center,

    /// <summary>Top-Left corner of parent element.</summary>
    TopLeft,

    /// <summary>Top-Right corner of parent element.</summary>
    TopRight,

    /// <summary>Top-Center of parent element.</summary>
    TopCenter,

    /// <summary>Bottom-Left corner of parent element.</summary>
    BottomLeft,

    /// <summary>Bottom-Right corner of parent element.</summary>
    BottomRight,

    /// <summary>Bottom-Center of parent element.</summary>
    BottomCenter,

    /// <summary>Center-Left of parent element.</summary>
    CenterLeft,

    /// <summary>Center-Right of parent element.</summary>
    CenterRight,

    /// <summary>Automatically position this entity below its older sibling.</summary>
    Auto,

    /// <summary>
    ///     Automatically position this entity to the right side of its older sibling, and begin a new row whenever
    ///     exceeding the parent container width.
    /// </summary>
    AutoInline,

    /// <summary>
    ///     Automatically position this entity to the right side of its older sibling, even if exceeding parent container
    ///     width.
    /// </summary>
    AutoInlineNoBreak,

    /// <summary>
    ///     Position of the older sibling bottom, eg align this entity based on its older sibling, but center on X axis.
    ///     Use this property to place entities one after another but keep them aligned to center (especially paragraphs).
    /// </summary>
    AutoCenter
}

/// <summary>
///     Possible entity states and interactions with user.
/// </summary>
public enum EntityState
{
    /// <summary>Default state, eg currently not interacting.</summary>
    Default = 0,

    /// <summary>Mouse is hovering over this entity.</summary>
    MouseHover = 1,

    /// <summary>Mouse button is pressed down over this entity.</summary>
    MouseDown = 2
}

/// <summary>
///     Basic UI entity.
///     All entities inherit from this class and share this API.
/// </summary>
[Serializable]
public abstract class Entity
{
    // all child types
    internal static List<Type> _serializableTypes = new();

    /// <summary>
    ///     A special size used value to use when you want to get the entity default size.
    /// </summary>
    public static readonly Vector2 USE_DEFAULT_SIZE = new(-1, -1);

    /// <summary>Basic default style that all entities share. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultStyle = new();

    /// <summary>Default size this entity will have when no size is provided or when -1 is set for either width or height.</summary>
    public static Vector2 DefaultSize = Vector2.Zero;

    /// <summary>Anchor to position this entity based on (see Anchor enum for more info).</summary>
    protected Anchor _anchor;

    // optional background object for this entity.
    // the background will be rendered on the full size of this entity, behind it, and will not respond to events etc.
    private Entity _background;

    // list of child elements
    private List<Entity> _children = new();

    /// <summary>Currently calculated destination rect (eg the region this entity is drawn on).</summary>
    protected Rectangle _destRect;

    /// <summary>Currently calculated internal destination rect (eg the region this entity children are positioned in).</summary>
    protected Rectangle _destRectInternal;

    /// <summary>
    ///     Every time we update destination rect and internal destination rect view the update function, we increase this
    ///     counter.
    ///     This is so our children will know we did an update and they need to update too.
    /// </summary>
    internal uint _destRectVersion;

    /// <summary>
    ///     Determines if this entity should select a first entity.
    /// </summary>
    private bool _doFirstSelection;

    // is this entity draggable?
    private bool _draggable;

    // current dragging offset.
    private Vector2 _dragOffset = Vector2.Zero;

    /// <summary>Current entity state.</summary>
    protected EntityState _entityState = EntityState.Default;

    /// <summary>
    ///     If true, will not show this entity when searching.
    ///     Used for internal entities.
    /// </summary>
    internal bool _hiddenInternalEntity = false;

    /// <summary>Index inside parent.</summary>
    protected int _indexInParent;

    // true if this entity is currently being dragged.
    private bool _isBeingDragged;

    /// <summary>Is this entity currently disabled?</summary>
    private bool _isCurrentlyDisabled;

    /// <summary>
    ///     Mark if this entity is dirty and need to recalculate its destination rect.
    /// </summary>
    private bool _isDirty = true;

    // mark the first draw call on this entity (automatically set by MakeFirstSelection()).
    private bool _isFirstAfterDraw;

    /// <summary>
    ///     Is this entity the first selected entity.
    /// </summary>
    private bool _isFirstSelection;

    // mark the first update call on this entity.
    private bool _isFirstUpdate = true;

    // is this entity currently focused?
    private bool _isFocused;

    /// <summary>Is the entity currently interactable.</summary>
    protected bool _isInteractable;

    /// <summary>Is mouse currently pointing on this entity.</summary>
    protected bool _isMouseOver;

    /// <summary>Is the entity currently selectable.</summary>
    private bool _isSelectable;

    /// <summary>
    ///     Last known scroll value, when entities are inside scrollable panels.
    /// </summary>
    protected Point _lastScrollVal = Point.Zero;

    // optional max size.
    private Vector2? _maxSize;

    // optional min size.
    private Vector2? _minSize;

    // do we need to init drag offset from current position?
    private bool _needToSetDragOffset;

    // do we need to update sorted children list?
    internal bool _needToSortChildren = true;

    /// <summary>Offset, in pixels, from the anchor position.</summary>
    protected Vector2 _offset;

    /// <summary>The direct parent of this entity.</summary>
    protected Entity _parent;

    /// <summary>
    ///     The last known version we have of the parent dest rect version.
    ///     If this number does not match our parent's _destRectVersion, we will recalculate destination rect.
    /// </summary>
    private uint _parentLastDestRectVersion;

    /// <summary>Entity size (in pixels). Value of 0 will take parent's full size. -1 will take defaults.</summary>
    protected Vector2 _size;

    // list of sorted children
    private List<Entity> _sortedChildren;

    // entity current style properties
    private StyleSheet _style = new();

    /// <summary>Is the entity currently visible.</summary>
    private bool _visible = true;

    /// <summary>Callback to execute every frame after this entity is rendered.</summary>
    [XmlIgnore] public EventCallback AfterDraw;

    /// <summary>Callback to execute every frame after this entity updates.</summary>
    [XmlIgnore] public EventCallback AfterUpdate;

    /// <summary>Callback to execute every frame before this entity is rendered.</summary>
    [XmlIgnore] public EventCallback BeforeDraw;

    /// <summary>Callback to execute every frame before this entity updates.</summary>
    [XmlIgnore] public EventCallback BeforeUpdate;

    /// <summary>
    ///     If this boolean is true, events will just "go through" this entity to its children or entities behind it.
    ///     This bool comes to solve conditions where you have two panels without skin that hide each other but you want
    ///     users to be able to click on the bottom panel through the upper panel, provided it doesn't hit any of the first
    ///     panel's children.
    /// </summary>
    public bool ClickThrough = false;

    /// <summary>
    ///     If this set to true, this entity will still react to events if its direct parent is locked.
    ///     This setting is mostly for scrollbars etc, that even if parent is locked should still be scrollable.
    /// </summary>
    protected bool DoEventsIfDirectParentIsLocked = false;

    /// <summary>Is the entity currently enabled? If false, will not be interactive and be rendered with a greyscale effect.</summary>
    public bool Enabled = true;

    /// <summary>
    ///     Adds extra space outside the dest rect for collision detection.
    ///     In other words, if extra margin is set to 10 and the user points with its mouse 5 pixels above this entity,
    ///     it would still think the user points on the entity.
    /// </summary>
    public Point ExtraMargin = Point.Zero;

    /// <summary>Optional identifier you can attach to entities so you can later search and retrieve by.</summary>
    public string Identifier = string.Empty;

    /// <summary>
    ///     If true, this entity will always inherit its parent state.
    ///     This is useful for stuff like a paragraph that's attached to a button etc.
    ///     NOTE!!! entities that inherit parent state will not trigger any events either.
    /// </summary>
    public bool InheritParentState;

    /// <summary>
    ///     Is the A button held for a slider or progress bar drag?
    /// </summary>
    private bool IsGamepadAButtonHeldForDrag;

    /// <summary>If true, users will not be able to drag this entity outside its parent boundaries.</summary>
    public bool LimitDraggingToParentBoundaries = true;

    /// <summary>
    ///     If true, this entity and its children will not respond to events (but will be drawn normally, unlike when
    ///     disabled).
    /// </summary>
    public bool Locked = false;

    /// <summary>Callback to execute when user clicks on this entity (eg release mouse over it).</summary>
    [XmlIgnore] public EventCallback OnClick;

    /// <summary>Callback to execute every time this entity focus / unfocus.</summary>
    [XmlIgnore] public EventCallback OnFocusChange = null;

    /// <summary>Callback to execute when mouse button is pressed over this entity (called once when button is pressed).</summary>
    public EventCallback OnGamePadPressed = null;

    /// <summary>Callback to execute when mouse button is released over this entity (called once when button is pressed).</summary>
    public EventCallback OnGamePadReleased = null;

    /// <summary>Callback to execute when mouse button is pressed over this entity (called once when button is pressed).</summary>
    [XmlIgnore] public EventCallback OnMouseDown;

    /// <summary>Callback to execute when mouse start hovering over this entity (eg enters its region).</summary>
    [XmlIgnore] public EventCallback OnMouseEnter;

    /// <summary>Callback to execute when mouse stop hovering over this entity (eg leaves its region).</summary>
    [XmlIgnore] public EventCallback OnMouseLeave;

    /// <summary>Callback to execute when mouse button is released over this entity (called once when button is released).</summary>
    [XmlIgnore] public EventCallback OnMouseReleased;

    /// <summary>Callback to execute when mouse wheel scrolls and this entity is the active entity.</summary>
    [XmlIgnore] public EventCallback OnMouseWheelScroll;

    /// <summary>Callback to execute when user clicks on this entity with right mouse button (eg release mouse over it).</summary>
    [XmlIgnore] public EventCallback OnRightClick;

    /// <summary>Callback to execute when right mouse button is pressed over this entity (called once when button is pressed).</summary>
    [XmlIgnore] public EventCallback OnRightMouseDown;

    /// <summary>Called when entity starts getting dragged (only if draggable).</summary>
    [XmlIgnore] public EventCallback OnStartDrag;

    /// <summary>Called when entity stop getting dragged (only if draggable).</summary>
    [XmlIgnore] public EventCallback OnStopDrag;

    /// <summary>Callback to execute when entity value changes (relevant only for entities with value).</summary>
    [XmlIgnore] public EventCallback OnValueChange;

    /// <summary>
    ///     Callback to execute every time the visibility of this entity changes (also invokes when parent becomes
    ///     invisible / visible again).
    /// </summary>
    [XmlIgnore] public EventCallback OnVisiblityChange = null;

    /// <summary>
    ///     Optional extra drawing priority, to bring certain objects before others.
    /// </summary>
    public int PriorityBonus = 0;

    /// <summary>
    ///     If in promiscuous mode, mouse button is pressed *outside* the entity and then released on the entity, click event
    ///     will be fired.
    ///     If false, in order to fire click event the mouse button must be pressed AND released over this entity (but can
    ///     travel outside while being
    ///     held down, as long as its released inside).
    ///     Note: Windows default behavior is non promiscuous mode.
    /// </summary>
    public bool PromiscuousClicksMode = false;

    /// <summary>
    ///     Optional tooltip text to show if the user points on this entity for long enough.
    /// </summary>
    public string ToolTipText;

    /// <summary>
    ///     If true (default), will use the actual object size for collision detection. If false, will use the size property.
    ///     This is useful for paragraphs, for example, where the actual width is based on text content and can vary and be
    ///     totally
    ///     different than the size set in the constructor.
    /// </summary>
    public bool UseActualSizeForCollision = true;

    /// <summary>Called every frame while the entity is being dragged.</summary>
    [XmlIgnore] public EventCallback WhileDragging;

    /// <summary>Callback to execute every frame while mouse button is pressed over the entity.</summary>
    [XmlIgnore] public EventCallback WhileMouseDown;

    /// <summary>
    ///     Callback to execute every frame while mouse is hovering over the entity (not called while mouse button is
    ///     down).
    /// </summary>
    [XmlIgnore] public EventCallback WhileMouseHover;

    /// <summary>Callback to execute every frame while mouse is hovering over the entity, even if mouse is down.</summary>
    [XmlIgnore] public EventCallback WhileMouseHoverOrDown;

    /// <summary>Callback to execute every frame while right mouse button is pressed over the entity.</summary>
    [XmlIgnore] public EventCallback WhileRightMouseDown;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Entity()
    {
        MakeSerializable(typeof(Entity));
    }

    /// <summary>
    ///     Create the entity.
    /// </summary>
    /// <param name="size">Entity size, in pixels.</param>
    /// <param name="anchor">Poisition anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Entity(Vector2? size = null, Anchor anchor = Anchor.Auto, Vector2? offset = null)
    {
        // set as dirty (eg need to recalculate destination rect)
        MarkAsDirty();

        // store size, anchor and offset
        var defaultSize = EntityDefaultSize;
        _size = size ?? defaultSize;
        _offset = offset ?? Vector2.Zero;
        _anchor = anchor;

        // set basic default style
        UpdateStyle(DefaultStyle);

        // check default size on specific axises
        if (_size.X == -1) _size.X = defaultSize.X;
        if (_size.Y == -1) _size.Y = defaultSize.Y;
    }

    /// <summary>
    ///     Get / set children list.
    /// </summary>
    public List<Entity> Children
    {
        get => _children;
        set
        {
            ClearChildren();
            foreach (var child in value) AddChild(child);
        }
    }

    /// <summary>
    ///     Is the entity currently selectable (use it to define entities, which should / should not be selectable by the
    ///     gamepad).
    /// </summary>
    public bool Selectable
    {
        get => _isSelectable;
        set => _isSelectable = value;
    }

    /// <summary>
    ///     Checks if this entity is the first selection within his parent (starting point for gamepad controls).
    /// </summary>
    public bool IsFirstSelection
    {
        get => _isFirstSelection;
        set => _isFirstSelection = value;
    }

    /// <summary>
    ///     Checks if this entity should do a first selection in his children (starting point for gamepad controls).
    /// </summary>
    private bool DoFirstSelection
    {
        get => _doFirstSelection;
        set => _doFirstSelection = value;
    }

    /// <summary>
    ///     Get / set raw stylesheet.
    /// </summary>
    public StyleSheet RawStyleSheet
    {
        get => _style;
        set => _style = value;
    }

    /// <summary>
    ///     Get overflow scrollbar value.
    /// </summary>
    protected virtual Point OverflowScrollVal => Point.Zero;

    /// <summary>
    ///     If defined, will limit the minimum size of this entity when calculating size.
    ///     This is especially useful for entities with size that depends on their parent entity size, for example
    ///     if you define an entity to take 20% of its parent space but can't be less than 200 pixels width.
    /// </summary>
    public Vector2? MinSize
    {
        get => _minSize;
        set
        {
            _minSize = value;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     If defined, will limit the maximum size of this entity when calculating size.
    ///     This is especially useful for entities with size that depends on their parent entity size, for example
    ///     if you define an entity to take 20% of its parent space but can't be more than 200 pixels width.
    /// </summary>
    public Vector2? MaxSize
    {
        get => _maxSize;
        set
        {
            _maxSize = value;
            MarkAsDirty();
        }
    }

    /// <summary>Optional data you can attach to this entity and retrieve later (for example when handling events).</summary>
    [XmlIgnore]
    public object AttachedData { get; set; } = null;

    /// <summary>
    ///     Set / get offset.
    /// </summary>
    public Vector2 Offset
    {
        get => _offset;
        set => SetOffset(value);
    }

    /// <summary>
    ///     Set / get anchor.
    /// </summary>
    public Anchor Anchor
    {
        get => _anchor;
        set => SetAnchor(value);
    }

    /// <summary>Disable entities - will be removed in future versions!</summary>
    [Obsolete("'Disabled' is deprecated, please use 'Enabled' instead.")]
    public bool Disabled
    {
        get => !Enabled;
        set => Enabled = !value;
    }

    /// <summary>Does this entity or one of its children currently focused?</summary>
    [XmlIgnore]
    public bool IsFocused
    {
        // get if focused
        get => _isFocused;

        // set if focused
        set
        {
            if (_isFocused != value)
            {
                _isFocused = value;
                DoOnFocusChange();
            }
        }
    }

    /// <summary>
    ///     Get internal destination rect.
    /// </summary>
    public Rectangle InternalDestRect => _destRectInternal;

    private bool _selectionOnDstCalc { get; set; }

    public float MilkFactor { get; set; } = 0f;

    /// <summary>
    ///     Return the default size for this entity.
    /// </summary>
    public Vector2 EntityDefaultSize
    {
        get
        {
            // get current class type
            var type = GetType();

            // try to get default size static property, and if not found, climb to parent class until DefaultSize is defined.
            // note: eventually it will stop at Entity, since we have defined default size here.
            while (true)
            {
                // try to get DefaultSize field and if found return it
                var field = type.GetField("DefaultSize", BindingFlags.Public | BindingFlags.Static);
                if (field != null) return (Vector2)field.GetValue(null);

                // if not found climb up to parent
                type = type.BaseType;
            }
        }
    }

    /// <summary>
    ///     Get input helper from active user interface.
    /// </summary>
    protected InputHelper Input => UserInterface.Input;

    /// <summary>
    ///     Get the active user interface global scale.
    /// </summary>
    protected float GlobalScale => UserInterface.Active.GlobalScale;

    /// <summary>
    ///     If true, will add debug drawing to UI system to show offsets, margin, etc.
    /// </summary>
    protected bool DebugDraw => UserInterface.Active.DebugDraw;

    /// <summary>Get extra space after with current UI scale applied. </summary>
    protected Vector2 _scaledSpaceAfter => SpaceAfter * GlobalScale;

    /// <summary>Get extra space before with current UI scale applied. </summary>
    protected Vector2 _scaledSpaceBefore => SpaceBefore * GlobalScale;

    /// <summary>Get size with current UI scale applied. </summary>
    protected Vector2 _scaledSize => _size * GlobalScale;

    /// <summary>Get offset with current UI scale applied. </summary>
    protected Vector2 _scaledOffset => _offset * GlobalScale;

    /// <summary>Get offset with current UI scale applied. </summary>
    protected Vector2 _scaledPadding => Padding * GlobalScale;

    /// <summary>
    ///     Set / get visibility.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            DoOnVisibilityChange();
        }
    }

    /// <summary>
    ///     Return entity priority in drawing order and event handling.
    /// </summary>
    public virtual int Priority => _indexInParent + PriorityBonus;

    /// <summary>
    ///     Get if this entity needs to recalculate destination rect.
    /// </summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    ///     Is the entity draggable (eg can a user grab it and drag it around).
    /// </summary>
    public bool Draggable
    {
        get => _draggable;
        set
        {
            _needToSetDragOffset = _draggable != value;
            _draggable = value;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     Optional background entity that will not respond to events and will always be rendered right behind this entity.
    /// </summary>
    public Entity Background
    {
        get => _background;
        set
        {
            if (value != null && value._parent != null)
                throw new InvalidStateException("Cannot set background entity that have a parent!");
            _background = value;
        }
    }

    /// <summary>
    ///     Current entity state (default / mouse hover / mouse down..).
    /// </summary>
    [XmlIgnore]
    public EntityState State
    {
        get => _entityState;
        set => _entityState = value;
    }

    /// <summary>
    ///     Entity current size property.
    /// </summary>
    public Vector2 Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                MarkAsDirty();
            }
        }
    }

    /// <summary>
    ///     Extra space (in pixels) to reserve *after* this entity when using Auto Anchors.
    /// </summary>
    [XmlIgnore]
    public Vector2 SpaceAfter
    {
        set => SetStyleProperty(StylePropertyIds.SpaceAfter, new StyleProperty(value));
        get => GetActiveStyle(StylePropertyIds.SpaceAfter).asVector;
    }

    /// <summary>
    ///     Extra space (in pixels) to reserve *before* this entity when using Auto Anchors.
    /// </summary>
    [XmlIgnore]
    public Vector2 SpaceBefore
    {
        set => SetStyleProperty(StylePropertyIds.SpaceBefore, new StyleProperty(value));
        get => GetActiveStyle(StylePropertyIds.SpaceBefore).asVector;
    }

    /// <summary>
    ///     Entity fill color - this is just a sugarcoat to access the default fill color style property.
    /// </summary>
    [XmlIgnore]
    public Color FillColor
    {
        set => SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.FillColor).asColor;
    }

    /// <summary>
    ///     Entity fill color opacity - this is just a sugarcoat to access the default fill color alpha style property.
    /// </summary>
    [XmlIgnore]
    public byte Opacity
    {
        set
        {
            var col = FillColor;
            col.A = value;
            SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(col), markAsDirty: false);
        }
        get => FillColor.A;
    }

    /// <summary>
    ///     Entity outline color opacity - this is just a sugarcoat to access the default outline color alpha style property.
    /// </summary>
    [XmlIgnore]
    public byte OutlineOpacity
    {
        set
        {
            var col = OutlineColor;
            col.A = value;
            SetStyleProperty(StylePropertyIds.OutlineColor, new StyleProperty(col), markAsDirty: false);
        }
        get => OutlineColor.A;
    }

    /// <summary>
    ///     Entity padding - this is just a sugarcoat to access the default padding style property.
    /// </summary>
    [XmlIgnore]
    public Vector2 Padding
    {
        set => SetStyleProperty(StylePropertyIds.Padding, new StyleProperty(value));
        get => GetActiveStyle(StylePropertyIds.Padding).asVector;
    }

    /// <summary>
    ///     Entity shadow color - this is just a sugarcoat to access the default shadow color style property.
    /// </summary>
    [XmlIgnore]
    public Color ShadowColor
    {
        set => SetStyleProperty(StylePropertyIds.ShadowColor, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.ShadowColor).asColor;
    }

    /// <summary>
    ///     Entity shadow scale - this is just a sugarcoat to access the default shadow scale style property.
    /// </summary>
    [XmlIgnore]
    public float ShadowScale
    {
        set => SetStyleProperty(StylePropertyIds.ShadowScale, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.ShadowScale).asFloat;
    }

    /// <summary>
    ///     Entity shadow offset - this is just a sugarcoat to access the default shadow offset style property.
    /// </summary>
    [XmlIgnore]
    public Vector2 ShadowOffset
    {
        set => SetStyleProperty(StylePropertyIds.ShadowOffset, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.ShadowOffset).asVector;
    }

    /// <summary>
    ///     Entity scale - this is just a sugarcoat to access the default scale style property.
    /// </summary>
    public float Scale
    {
        set => SetStyleProperty(StylePropertyIds.Scale, new StyleProperty(value));
        get => GetActiveStyle(StylePropertyIds.Scale).asFloat;
    }

    /// <summary>
    ///     Entity outline color - this is just a sugarcoat to access the default outline color style property.
    /// </summary>
    [XmlIgnore]
    public Color OutlineColor
    {
        set => SetStyleProperty(StylePropertyIds.OutlineColor, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.OutlineColor).asColor;
    }

    /// <summary>
    ///     Entity outline width - this is just a sugarcoat to access the default outline color style property.
    /// </summary>
    [XmlIgnore]
    public int OutlineWidth
    {
        set => SetStyleProperty(StylePropertyIds.OutlineWidth, new StyleProperty(value), markAsDirty: false);
        get => GetActiveStyle(StylePropertyIds.OutlineWidth).asInt;
    }

    /// <summary>
    ///     Get the direct parent of this entity.
    /// </summary>
    public Entity Parent => _parent;

    /// <summary>
    ///     Return if the mouse is currently pressing on this entity (eg over it and left mouse button is down).
    /// </summary>
    public bool IsMouseDown => _entityState == EntityState.MouseDown;

    /// <summary>
    ///     Return if the mouse is currently over this entity (regardless of whether or not mouse button is down).
    /// </summary>
    public bool IsMouseOver => _isMouseOver;

    /// <summary>
    ///     Make an entity type serializable.
    /// </summary>
    /// <param name="type">Entity type to make serializable.</param>
    public static void MakeSerializable(Type type)
    {
        _serializableTypes.Add(type);
    }

    /// <summary>
    ///     This entity will select the first visible and selectable entity.
    /// </summary>
    public void MakeFirstSelection()
    {
        _doFirstSelection = true;
        _isFirstAfterDraw = true;
    }

    /// <summary>
    ///     Get current mouse position.
    /// </summary>
    /// <param name="addVector">Optional vector to add to cursor position.</param>
    /// <returns>Mouse position.</returns>
    protected Vector2 GetMousePos(Vector2? addVector = null)
    {
        return UserInterface.Active.GetTransformedCursorPos(addVector);
    }

    /// <summary>
    ///     Call this function when the first update occures.
    /// </summary>
    protected virtual void DoOnFirstUpdate()
    {
        // call the spawn event
        UserInterface.Active.OnEntitySpawn?.Invoke(this);

        // make parent dirty
        if (_parent != null) _parent.MarkAsDirty();
    }

    /// <summary>
    ///     Call this function when the first after draw event occurs.
    /// </summary>
    protected virtual void DoOnFirstAfterDraw()
    {
        if (DoFirstSelection) SelectFirstEntity(this, false);
    }

    /// <summary>
    ///     Return stylesheet property for a given state.
    /// </summary>
    /// <param name="property">Property identifier.</param>
    /// <param name="state">State to get property for (if undefined will fallback to default state).</param>
    /// <param name="fallbackToDefault">If true and property not found for given state, will fallback to default state.</param>
    /// <returns>Style property value for given state or default, or null if undefined.</returns>
    public StyleProperty GetStyleProperty(string property, EntityState state = EntityState.Default,
        bool fallbackToDefault = true)
    {
        return _style.GetStyleProperty(property, state, fallbackToDefault);
    }

    /// <summary>
    ///     Set a stylesheet property.
    /// </summary>
    /// <param name="property">Property identifier.</param>
    /// <param name="value">Property value.</param>
    /// <param name="state">State to set property for.</param>
    /// <param name="markAsDirty">If true, will mark this entity as dirty after this style change.</param>
    public void SetStyleProperty(string property, StyleProperty value, EntityState state = EntityState.Default,
        bool markAsDirty = true)
    {
        _style.SetStyleProperty(property, value, state);
        if (markAsDirty) MarkAsDirty();
    }

    /// <summary>
    ///     Return stylesheet property for current entity state (or default if undefined for state).
    /// </summary>
    /// <param name="property">Property identifier.</param>
    /// <returns>Stylesheet property value for current entity state, or default if not defined.</returns>
    public StyleProperty GetActiveStyle(string property)
    {
        return GetStyleProperty(property, _entityState);
    }

    /// <summary>
    ///     Update the entire stylesheet from a different stylesheet.
    /// </summary>
    /// <param name="updates">Stylesheet to update from.</param>
    public void UpdateStyle(StyleSheet updates)
    {
        _style.UpdateFrom(updates);
        MarkAsDirty();
    }

    /// <summary>
    ///     Find and return first occurance of a child entity with a given identifier and specific type.
    /// </summary>
    /// <typeparam name="T">Entity type to get.</typeparam>
    /// <param name="identifier">Identifier to find.</param>
    /// <param name="recursive">
    ///     If true, will search recursively in children of children. If false, will search only in direct
    ///     children.
    /// </param>
    /// <returns>First found entity with given identifier and type, or null if nothing found.</returns>
    public T Find<T>(string identifier, bool recursive = false) where T : Entity
    {
        // should we return any entity type?
        var anyType = typeof(T) == typeof(Entity);

        // iterate children
        foreach (var child in _children)
        {
            // skip hidden entities
            if (child._hiddenInternalEntity)
                continue;

            // check if identifier and type matches - if so, return it
            if (child.Identifier == identifier && (anyType || child.GetType() == typeof(T))) return (T)child;

            // if recursive, search in child
            if (recursive)
            {
                // search in child
                var ret = child.Find<T>(identifier, recursive);

                // if found return it
                if (ret != null) return ret;
            }
        }

        // not found?
        return null;
    }

    /// <summary>
    ///     Find and return first occurance of a child entity with a given identifier.
    /// </summary>
    /// <param name="identifier">Identifier to find.</param>
    /// <param name="recursive">
    ///     If true, will search recursively in children of children. If false, will search only in direct
    ///     children.
    /// </param>
    /// <returns>First found entity with given identifier, or null if nothing found.</returns>
    public Entity Find(string identifier, bool recursive = false)
    {
        return Find<Entity>(identifier, recursive);
    }

    /// <summary>
    ///     Iterate over children and call 'callback' for every direct child of this entity.
    /// </summary>
    /// <param name="callback">Callback function to call with every child of this entity.</param>
    public void IterateChildren(EventCallback callback)
    {
        foreach (var child in _children) callback(child);
    }

    /// <summary>
    ///     Return if this entity is currently disabled, due to self or one of the parents / grandparents being disabled.
    /// </summary>
    /// <returns>True if entity is disabled.</returns>
    public bool IsDisabled()
    {
        // iterate over parents until root, starting with self.
        // if any entity along the way is disabled we return true.
        var parent = this;
        while (parent != null)
        {
            if (!parent.Enabled) return true;
            parent = parent._parent;
        }

        // not disabled
        return false;
    }


    /// <summary>
    ///     Check if this entity is a descendant of another entity.
    ///     This goes up all the way to root.
    /// </summary>
    /// <param name="other">Entity to check if this entity is descendant of.</param>
    /// <returns>True if this entity is descendant of the other entity.</returns>
    public bool IsDeepChildOf(Entity other)
    {
        // iterate over parents until root, starting with self.
        // if any entity along the way is child of 'other', we return true.
        var parent = this;
        while (parent != null)
        {
            if (parent._parent == other) return true;
            parent = parent._parent;
        }

        // not child of
        return false;
    }

    /// <summary>
    ///     Return if this entity is currently locked, due to self or one of the parents / grandparents being locked.
    /// </summary>
    /// <returns>True if entity is disabled.</returns>
    public bool IsLocked()
    {
        // iterate over parents until root, starting with self.
        // if any entity along the way is locked we return true.
        var parent = this;
        while (parent != null)
        {
            if (parent.Locked)
            {
                // special case - if should do events even when parent is locked and direct parent, skip
                if (DoEventsIfDirectParentIsLocked)
                    if (parent == _parent)
                    {
                        parent = parent._parent;
                        continue;
                    }

                // if parent locked return true
                return true;
            }

            // advance to next parent
            parent = parent._parent;
        }

        // not disabled
        return false;
    }

    /// <summary>
    ///     Return if this entity is currently visible, eg this and all its parents and grandparents are visible.
    /// </summary>
    /// <returns>True if entity is really visible.</returns>
    public bool IsVisible()
    {
        // iterate over parents until root, starting with self.
        // if any entity along the way is not visible we return false.
        var parent = this;
        while (parent != null)
        {
            if (!parent.Visible) return false;
            parent = parent._parent;
        }

        // visible!
        return true;
    }

    public void SetEnabled(bool value)
    {
        Enabled = value;
    }

    /// <summary>
    ///     Set the position and anchor of this entity.
    /// </summary>
    /// <param name="anchor">New anchor to set.</param>
    /// <param name="offset">Offset from new anchor position.</param>
    public void SetPosition(Anchor anchor, Vector2 offset)
    {
        SetAnchor(anchor);
        SetOffset(offset);
    }

    /// <summary>
    ///     Set the anchor of this entity.
    /// </summary>
    /// <param name="anchor">New anchor to set.</param>
    public void SetAnchor(Anchor anchor)
    {
        _anchor = anchor;
        MarkAsDirty();
    }

    /// <summary>
    ///     Set the offset of this entity.
    /// </summary>
    /// <param name="offset">New offset to set.</param>
    public void SetOffset(Vector2 offset)
    {
        if (_offset != offset || _dragOffset != offset)
        {
            _dragOffset = _offset = offset;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     Return children in a sorted list by priority.
    /// </summary>
    /// <returns>List of children sorted by priority.</returns>
    protected List<Entity> GetSortedChildren()
    {
        // if need to sort children, rebuild the sorted list
        if (_needToSortChildren)
        {
            // create list to sort and return
            _sortedChildren = new List<Entity>(_children);

            // get children in a sorted list
            _sortedChildren.Sort((x, y) =>
                x.Priority.CompareTo(y.Priority));

            // no longer need to sort
            _needToSortChildren = false;
        }

        // return the sorted list
        return _sortedChildren;
    }

    /// <summary>
    ///     Update dest rect and internal dest rect.
    ///     This is called internally whenever a change is made to the entity or its parent.
    /// </summary>
    public virtual void UpdateDestinationRects()
    {
        // update dest and internal dest rects
        _destRect = CalcDestRect();
        _destRectInternal = CalcInternalRect();

        // mark as no longer dirty
        _isDirty = false;

        // increase dest rect version and update parent last known
        _destRectVersion++;
        if (_parent != null) _parentLastDestRectVersion = _parent._destRectVersion;
    }

    /// <summary>
    ///     Update dest rect and internal dest rect, but only if needed (eg if something changed since last update).
    /// </summary>
    public virtual void UpdateDestinationRectsIfDirty()
    {
        // if dirty, update destination rectangles
        if (_parent != null && (_isDirty || _parentLastDestRectVersion != _parent._destRectVersion))
            UpdateDestinationRects();
    }

    /// <summary>
    ///     Draw this entity and its children.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        // if not visible skip
        if (!Visible) return;

        // update if disabled
        _isCurrentlyDisabled = IsDisabled();

        // do before draw event
        OnBeforeDraw(spriteBatch);

        // draw background
        if (Background != null)
        {
            _background._parent = this;
            _background._indexInParent = 0;
            _background.Draw(spriteBatch);
            _background._parent = null;
        }

        // calc desination rects (if needed)
        UpdateDestinationRectsIfDirty();

        // draw shadow
        DrawEntityShadow(spriteBatch);

        // draw entity outline
        DrawEntityOutline(spriteBatch);

        // draw the entity itself
        UserInterface.Active.DrawUtils.StartDraw(spriteBatch, _isCurrentlyDisabled);
        DrawEntity(spriteBatch, DrawPhase.Base);
        UserInterface.Active.DrawUtils.EndDraw(spriteBatch);

        // do debug drawing
        if (DebugDraw) DrawDebugStuff(spriteBatch);

        // draw all child entities
        DrawChildren(spriteBatch);

        // do after draw event
        OnAfterDraw(spriteBatch);
    }

    /// <summary>
    ///     Draw debug stuff for this entity.
    /// </summary>
    /// <param name="spriteBatch">Spritebatch to use for drawing.</param>
    protected virtual void DrawDebugStuff(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

        // first draw whole dest rect
        var destRectCol = new Color(0f, 1f, 0.25f, 0.05f);
        spriteBatch.Draw(Resources.WhiteTexture, _destRect, destRectCol);

        // now draw internal dest rect
        var internalCol = new Color(1f, 0.5f, 0f, 0.5f);
        spriteBatch.Draw(Resources.WhiteTexture, _destRectInternal, internalCol);

        // draw space before
        var spaceColor = new Color(0f, 0f, 0.5f, 0.5f);
        if (SpaceBefore.X > 0)
            spriteBatch.Draw(Resources.WhiteTexture,
                new Rectangle((int)(_destRect.Left - _scaledSpaceBefore.X), _destRect.Y, (int)_scaledSpaceBefore.X,
                    _destRect.Height), spaceColor);
        if (SpaceBefore.Y > 0)
            spriteBatch.Draw(Resources.WhiteTexture,
                new Rectangle(_destRect.X, (int)(_destRect.Top - _scaledSpaceBefore.Y), _destRect.Width,
                    (int)_scaledSpaceBefore.Y), spaceColor);

        // draw space after
        spaceColor = new Color(0.5f, 0f, 0.5f, 0.5f);
        if (SpaceAfter.X > 0)
            spriteBatch.Draw(Resources.WhiteTexture,
                new Rectangle(_destRect.Right, _destRect.Y, (int)_scaledSpaceAfter.X, _destRect.Height), spaceColor);
        if (SpaceAfter.Y > 0)
            spriteBatch.Draw(Resources.WhiteTexture,
                new Rectangle(_destRect.X, _destRect.Bottom, _destRect.Width, (int)_scaledSpaceAfter.Y), spaceColor);

        spriteBatch.End();
    }

    /// <summary>
    ///     Draw all children.
    /// </summary>
    /// <param name="spriteBatch"></param>
    protected virtual void DrawChildren(SpriteBatch spriteBatch)
    {
        // do stuff before drawing children
        BeforeDrawChildren(spriteBatch);

        // get sorted children list
        var childrenSorted = GetSortedChildren();

        // draw all children
        foreach (var child in childrenSorted) child.Draw(spriteBatch);

        // do stuff after drawing children
        AfterDrawChildren(spriteBatch);
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal virtual void InitAfterDeserialize()
    {
        // fix children parent
        var temp = _children;
        _children = new List<Entity>();
        foreach (var child in temp)
        {
            child._parent = null;
            AddChild(child, child.InheritParentState);
        }

        // mark as dirty
        MarkAsDirty();

        // update all children
        foreach (var child in _children) child.InitAfterDeserialize();
    }

    /// <summary>
    ///     Put all entities that have identifier property in a dictionary.
    ///     Note: if multiple entities share the same identifier, the deepest entity in hirarchy will end up in dict.
    /// </summary>
    /// <param name="dict">Dictionary to put entities into.</param>
    public void PopulateDict(ref Dictionary<string, Entity> dict)
    {
        // add self if got identifier
        if (Identifier != null && Identifier.Length > 0)
            dict[Identifier] = this;

        // iterate children
        foreach (var child in _children) child.PopulateDict(ref dict);
    }

    /// <summary>
    ///     Called before drawing child entities of this entity.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
    protected virtual void BeforeDrawChildren(SpriteBatch spriteBatch)
    {
    }

    /// <summary>
    ///     Called after drawing child entities of this entity.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used to draw entities.</param>
    protected virtual void AfterDrawChildren(SpriteBatch spriteBatch)
    {
    }

    /// <summary>
    ///     Draw entity shadow (if defined shadow).
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    protected virtual void DrawEntityShadow(SpriteBatch spriteBatch)
    {
        // store current 'is-dirty' flag, because it changes internally while drawing shadow
        var isDirty = _isDirty;

        // get current shadow color and if transparent skip
        var shadowColor = ShadowColor;
        if (shadowColor.A == 0) return;

        // get shadow scale
        var shadowScale = ShadowScale;

        // update position to draw shadow
        _destRect.X += (int)ShadowOffset.X;
        _destRect.Y += (int)ShadowOffset.Y;

        // store previous state and colors
        var oldFill = FillColor;
        var oldOutline = OutlineColor;
        var oldScale = Scale;
        var oldOutlineWidth = OutlineWidth;
        var oldState = _entityState;

        // set default colors and state for shadow pass
        FillColor = shadowColor;
        OutlineColor = Color.Transparent;
        OutlineWidth = 0;
        Scale = shadowScale;
        _entityState = EntityState.Default;

        // if disabled, turn color into greyscale
        if (_isCurrentlyDisabled)
            FillColor = new Color(Color.White * ((shadowColor.R + shadowColor.G + shadowColor.B) / 3f / 255f),
                shadowColor.A);

        // draw with shadow effect
        UserInterface.Active.DrawUtils.StartDrawSilhouette(spriteBatch);
        DrawEntity(spriteBatch, DrawPhase.Shadow);
        UserInterface.Active.DrawUtils.EndDraw(spriteBatch);

        // return position and colors back to what they were
        _destRect.X -= (int)ShadowOffset.X;
        _destRect.Y -= (int)ShadowOffset.Y;
        FillColor = oldFill;
        Scale = oldScale;
        OutlineColor = oldOutline;
        OutlineWidth = oldOutlineWidth;
        _entityState = oldState;

        // restore is-dirty flag
        _isDirty = isDirty;
    }

    /// <summary>
    ///     Draw entity outline.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    protected virtual void DrawEntityOutline(SpriteBatch spriteBatch)
    {
        // get outline width and if 0 return
        var outlineWidth = OutlineWidth;
        if (OutlineWidth == 0) return;

        // get outline color
        var outlineColor = OutlineColor;

        // if disabled, turn outline to grey
        if (_isCurrentlyDisabled)
            outlineColor = new Color(Color.White * ((outlineColor.R + outlineColor.G + outlineColor.B) / 3f / 255f),
                outlineColor.A);

        // store previous fill color
        var oldFill = FillColor;

        // store original destination rect
        var originalDest = _destRect;
        var originalIntDest = _destRectInternal;

        // store entity previous state
        var oldState = _entityState;

        spriteBatch.Begin();
        spriteBatch.DrawOutline(originalDest, OutlineColor, OutlineWidth);
        spriteBatch.End();

        // turn back to previous fill color
        SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(oldFill), oldState, false);

        // return to the original destination rect
        _destRect = originalDest;
        _destRectInternal = originalIntDest;
    }

    /// <summary>
    ///     The internal function to draw the entity itself.
    ///     Implemented by inheriting entity types.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected virtual void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
    }

    /// <summary>
    ///     Called every frame after drawing is done.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw on.</param>
    protected virtual void OnAfterDraw(SpriteBatch spriteBatch)
    {
        AfterDraw?.Invoke(this);
        UserInterface.Active.AfterDraw?.Invoke(this);

        if (_isFirstAfterDraw)
        {
            DoOnFirstAfterDraw();
            _isFirstAfterDraw = false;
        }
    }

    /// <summary>
    ///     Called every frame before drawing is done.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to draw on.</param>
    protected virtual void OnBeforeDraw(SpriteBatch spriteBatch)
    {
        BeforeDraw?.Invoke(this);
        UserInterface.Active.BeforeDraw?.Invoke(this);
    }

    /// <summary>
    ///     Add a child entity.
    /// </summary>
    /// <param name="child">Entity to add as child.</param>
    /// <param name="inheritParentState">
    ///     If true, this entity will inherit the parent's state (set InheritParentState
    ///     property).
    /// </param>
    /// <param name="index">If provided, will be the index in the children array to push the new entity.</param>
    /// <returns>The newly added entity.</returns>
    public T AddChild<T>(T child, bool inheritParentState = false, int index = -1) where T : Entity
    {
        // make sure don't already have a parent
        if (child._parent != null)
        {
            if (UserInterface.Active.SilentSoftErrors) return child;
            throw new InvalidStateException("Child element to add already got a parent!");
        }

        // need to sort children
        _needToSortChildren = true;

        // set inherit parent mode
        child.InheritParentState = inheritParentState;

        // set child's new parent
        child._parent = this;

        // if index is -1 or out of range, set to last item in childrens list
        if (index == -1 || index >= _children.Count) index = _children.Count;

        // add child at index
        child._indexInParent = index;
        _children.Insert(index, child);

        // update any siblings which need their index updating
        for (var i = index + 1; i < _children.Count; i++) _children[i]._indexInParent += 1;

        // reset child parent dest rect version
        child._parentLastDestRectVersion = _destRectVersion - 1;

        // mark child as dirty
        child.MarkAsDirty();
        MarkAsDirty();
        return child;
    }

    /// <summary>
    ///     Bring this entity to be on front (inside its parent).
    /// </summary>
    public void BringToFront()
    {
        var parent = _parent;
        parent.RemoveChild(this);
        parent.AddChild(this);
    }

    /// <summary>
    ///     Remove child entity.
    /// </summary>
    /// <param name="child">Entity to remove.</param>
    public void RemoveChild(Entity child)
    {
        // make sure don't already have a parent
        if (child._parent != this)
        {
            if (UserInterface.Active.SilentSoftErrors) return;
            throw new InvalidStateException("Child element to remove does not belong to this entity!");
        }

        // need to sort children
        _needToSortChildren = true;

        // set parent to null and remove
        child._parent = null;
        child._indexInParent = -1;
        _children.Remove(child);

        // reset index for all children
        var index = 0;
        foreach (var itrChild in _children) itrChild._indexInParent = index++;

        // mark child and self as dirty
        child.MarkAsDirty();
        MarkAsDirty();
    }

    /// <summary>
    ///     Remove all children entities.
    /// </summary>
    public void ClearChildren()
    {
        // remove all children
        foreach (var child in _children)
        {
            child._parent = null;
            child._indexInParent = -1;
            child.MarkAsDirty();
        }

        _children.Clear();

        // mark self as dirty
        _needToSortChildren = true;
        MarkAsDirty();
    }

    /// <summary>
    ///     Calculate and return the internal destination rectangle (note: this relay on the dest rect having a valid value
    ///     first).
    /// </summary>
    /// <returns>Internal destination rectangle.</returns>
    public virtual Rectangle CalcInternalRect()
    {
        // calculate the internal destination rect, eg after padding
        var padding = _scaledPadding;
        _destRectInternal = GetActualDestRect();
        _destRectInternal.X += (int)padding.X;
        _destRectInternal.Y += (int)padding.Y;
        _destRectInternal.Width -= (int)padding.X * 2;
        _destRectInternal.Height -= (int)padding.Y * 2;
        return _destRectInternal;
    }

    /// <summary>
    ///     Takes a size value in vector, that can be in percents or units, and convert it to absolute
    ///     size in pixels. For example, if given size is 0.5f this will calculate it to be half its parent
    ///     size, as it should be.
    /// </summary>
    /// <param name="size">Size to calculate.</param>
    /// <returns>Actual size in pixels.</returns>
    protected Point CalcActualSizeInPixels(Vector2 size)
    {
        // simple case: if size is not in percents, just return as-is
        if (size.X > 1f && size.Y > 1f)
            return size.ToPoint();

        // get parent internal destination rectangle
        _parent.UpdateDestinationRectsIfDirty();
        var parentDest = _parent._destRectInternal;

        // calc and return size
        return new Point(
            size.X == 0f ? parentDest.Width :
            size.X > 0f && size.X < 1f ? (int)(parentDest.Width * size.X) : (int)size.X,
            size.Y == 0f ? parentDest.Height :
            size.Y > 0f && size.Y < 1f ? (int)(parentDest.Height * size.Y) : (int)size.Y);
    }

    /// <summary>
    ///     Calculate and return the destination rectangle, eg the space this entity is rendered on.
    /// </summary>
    /// <returns>Destination rectangle.</returns>
    public virtual Rectangle CalcDestRect()
    {
        // create new rectangle
        var ret = new Rectangle();

        // no parent? stop here and return empty rect
        if (_parent == null) return ret;

        // get parent internal destination rectangle
        _parent.UpdateDestinationRectsIfDirty();
        var parentDest = _parent._destRectInternal;

        // set size:
        // 0: takes whole parent size.
        // 0.0 - 1.0: takes percent of parent size.
        // > 1.0: size in pixels.
        var size = _scaledSize;
        var sizeInPixels = CalcActualSizeInPixels(size);

        // apply min size
        if (MinSize != null)
        {
            var minInPixels = CalcActualSizeInPixels(MinSize.Value);
            sizeInPixels.X = Math.Max(minInPixels.X, sizeInPixels.X);
            sizeInPixels.Y = Math.Max(minInPixels.Y, sizeInPixels.Y);
        }

        // apply max size
        if (MaxSize != null)
        {
            var maxInPixels = CalcActualSizeInPixels(MaxSize.Value);
            sizeInPixels.X = Math.Min(maxInPixels.X, sizeInPixels.X);
            sizeInPixels.Y = Math.Min(maxInPixels.Y, sizeInPixels.Y);
        }

        // set return rect size
        ret.Width = sizeInPixels.X;
        ret.Height = sizeInPixels.Y;

        // make sure its a legal size
        if (ret.Width < 1) ret.Width = 1;
        if (ret.Height < 1) ret.Height = 1;

        // first calc some helpers
        var parent_left = parentDest.X;
        var parent_top = parentDest.Y;
        var parent_right = parent_left + parentDest.Width;
        var parent_bottom = parent_top + parentDest.Height;
        var parent_center_x = parent_left + parentDest.Width / 2;
        var parent_center_y = parent_top + parentDest.Height / 2;

        // get anchor and offset
        var anchor = _anchor;
        var offset = _scaledOffset;

        // if we are in dragging mode we do a little hack to use top-left anchor with the dragged offset
        // note: but only if drag offset was previously set.
        if (_draggable && !_needToSetDragOffset)
        {
            anchor = Anchor.TopLeft;
            offset = _dragOffset;
        }

        // calculate position based on anchor, parent and offset
        switch (anchor)
        {
            case Anchor.Auto:
            case Anchor.AutoInline:
            case Anchor.AutoInlineNoBreak:
            case Anchor.TopLeft:
                ret.X = parent_left + (int)offset.X;
                ret.Y = parent_top + (int)offset.Y;
                break;

            case Anchor.TopRight:
                ret.X = parent_right - ret.Width - (int)offset.X;
                ret.Y = parent_top + (int)offset.Y;
                break;

            case Anchor.TopCenter:
            case Anchor.AutoCenter:
                ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                ret.Y = parent_top + (int)offset.Y;
                break;

            case Anchor.BottomLeft:
                ret.X = parent_left + (int)offset.X;
                ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                break;

            case Anchor.BottomRight:
                ret.X = parent_right - ret.Width - (int)offset.X;
                ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                break;

            case Anchor.BottomCenter:
                ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                ret.Y = parent_bottom - ret.Height - (int)offset.Y;
                break;

            case Anchor.CenterLeft:
                ret.X = parent_left + (int)offset.X;
                ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                break;

            case Anchor.CenterRight:
                ret.X = parent_right - ret.Width - (int)offset.X;
                ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                break;

            case Anchor.Center:
                ret.X = parent_center_x - ret.Width / 2 + (int)offset.X;
                ret.Y = parent_center_y - ret.Height / 2 + (int)offset.Y;
                break;
        }

        // special case for auto anchors
        if ((anchor == Anchor.Auto || anchor == Anchor.AutoInline || anchor == Anchor.AutoCenter ||
             anchor == Anchor.AutoInlineNoBreak) && _parent != null)
        {
            // get previous entity before this
            var prevEntity = GetPreviousEntity(true);

            // if found entity before this one, align based on it
            if (prevEntity != null)
            {
                // make sure sibling is up-to-date
                prevEntity.UpdateDestinationRectsIfDirty();

                // handle inline align
                if (anchor == Anchor.AutoInline || anchor == Anchor.AutoInlineNoBreak)
                {
                    ret.X = prevEntity._destRect.Right +
                            (int)(offset.X + prevEntity._scaledSpaceAfter.X + _scaledSpaceBefore.X);
                    ret.Y = prevEntity._destRect.Y;
                }

                // handle inline align that ran out of width / or auto anchor not inline
                if ((anchor == Anchor.AutoInline && ret.Right > _parent._destRectInternal.Right) ||
                    anchor == Anchor.Auto || anchor == Anchor.AutoCenter)
                {
                    // align x
                    if (anchor != Anchor.AutoCenter) ret.X = parent_left + (int)offset.X;

                    // align y
                    ret.Y = prevEntity.GetDestRectForAutoAnchors().Bottom + (int)(offset.Y +
                        prevEntity._scaledSpaceAfter.Y +
                        _scaledSpaceBefore.Y);
                }
            }
            // if this is the first entity in parent, apply space-before only
            else
            {
                ret.X += (int)_scaledSpaceBefore.X;
                ret.Y += (int)_scaledSpaceBefore.Y;
            }
        }

        // some extra logic for draggables
        if (_draggable)
        {
            // if need to init dragged offset, set it
            // this trick is used so if an object is draggable, we first evaluate its position based on anchor etc, and we use that
            // position as starting point for the dragging
            if (_needToSetDragOffset)
            {
                _dragOffset.X = ret.X - parent_left;
                _dragOffset.Y = ret.Y - parent_top;
                _needToSetDragOffset = false;
            }

            // if draggable and need to be contained inside parent, validate it
            if (LimitDraggingToParentBoundaries)
            {
                if (ret.X < parent_left)
                {
                    ret.X = parent_left;
                    _dragOffset.X = 0;
                }

                if (ret.Y < parent_top)
                {
                    ret.Y = parent_top;
                    _dragOffset.Y = 0;
                }

                if (ret.Right > parent_right)
                {
                    _dragOffset.X -= ret.Right - parent_right;
                    ;
                    ret.X -= ret.Right - parent_right;
                }

                if (ret.Bottom > parent_bottom)
                {
                    _dragOffset.Y -= ret.Bottom - parent_bottom;
                    ret.Y -= ret.Bottom - parent_bottom;
                }
            }
        }

        // return the newly created rectangle
        _destRect = ret;

        if (_selectionOnDstCalc)
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
            SetCursorPosition(this);
            _selectionOnDstCalc = false;
        }

        return ret;
    }

    /// <summary>
    ///     Return actual destination rectangle.
    ///     This can be override and implemented by things like Paragraph, where the actual destination rect is based on
    ///     text content, font and word-wrap.
    /// </summary>
    /// <returns>The actual destination rectangle.</returns>
    public virtual Rectangle GetActualDestRect()
    {
        return _destRect;
    }

    /// <summary>
    ///     Return the actual dest rect for auto-anchoring purposes.
    ///     This is useful for things like DropDown, that when opened they take a larger part of the screen, but we don't
    ///     want it to push down other entities.
    /// </summary>
    protected virtual Rectangle GetDestRectForAutoAnchors()
    {
        return GetActualDestRect();
    }

    /// <summary>
    ///     Remove this entity from its parent.
    /// </summary>
    public void RemoveFromParent()
    {
        if (_parent != null) _parent.RemoveChild(this);
    }

    /// <summary>
    ///     Propagate all events trigger by this entity to a given other entity.
    ///     For example, if "OnClick" will be called on this entity, it will trigger OnClick on 'other' as well.
    /// </summary>
    /// <param name="other">Entity to propagate events to.</param>
    public virtual void PropagateEventsTo(Entity other)
    {
        OnMouseDown += entity => { other.OnMouseDown?.Invoke(other); };
        OnRightMouseDown += entity => { other.OnRightMouseDown?.Invoke(other); };
        OnMouseReleased += entity => { other.OnMouseReleased?.Invoke(other); };
        WhileMouseDown += entity => { other.WhileMouseDown?.Invoke(other); };
        WhileRightMouseDown += entity => { other.WhileRightMouseDown?.Invoke(other); };
        WhileMouseHover += entity => { other.WhileMouseHover?.Invoke(other); };
        WhileMouseHoverOrDown += entity => { other.WhileMouseHoverOrDown?.Invoke(other); };
        OnRightClick += entity => { other.OnRightClick?.Invoke(other); };
        OnClick += entity => { other.OnClick?.Invoke(other); };
        OnValueChange += entity => { other.OnValueChange?.Invoke(other); };
        OnMouseEnter += entity => { other.OnMouseEnter?.Invoke(other); };
        OnMouseLeave += entity => { other.OnMouseLeave?.Invoke(other); };
        OnMouseWheelScroll += entity => { other.OnMouseWheelScroll?.Invoke(other); };
        OnStartDrag += entity => { other.OnStartDrag?.Invoke(other); };
        OnStopDrag += entity => { other.OnStopDrag?.Invoke(other); };
        WhileDragging += entity => { other.WhileDragging?.Invoke(other); };
        BeforeDraw += entity => { other.BeforeDraw?.Invoke(other); };
        AfterDraw += entity => { other.AfterDraw?.Invoke(other); };
        BeforeUpdate += entity => { other.BeforeUpdate?.Invoke(other); };
        AfterUpdate += entity => { other.AfterUpdate?.Invoke(other); };
    }

    /// <summary>
    ///     Return the relative offset, in pixels, from parent top-left corner.
    /// </summary>
    /// <remarks>
    ///     This return the offset between the top left corner of this entity regardless of anchor type.
    /// </remarks>
    /// <returns>Calculated offset from parent top-left corner.</returns>
    public Vector2 GetRelativeOffset()
    {
        var dest = GetActualDestRect();
        var parentDest = _parent.GetActualDestRect();
        return new Vector2(dest.X - parentDest.X, dest.Y - parentDest.Y);
    }

    /// <summary>
    ///     Return the entity before this one in parent container, aka the next older sibling.
    /// </summary>
    /// <returns>Entity before this in parent, or null if first in parent or if orphan entity.</returns>
    /// <param name="skipInvisibles">If true, will skip invisible entities, eg will return the first visible older sibling.</param>
    protected Entity GetPreviousEntity(bool skipInvisibles = false)
    {
        // no parent? skip
        if (_parent == null) return null;

        // get siblings and iterate them
        var siblings = _parent.Children;
        Entity prev = null;
        foreach (var sibling in siblings)
        {
            // when getting to self, break the loop
            if (sibling == this) break;

            // if older sibling is invisible, skip it
            if (skipInvisibles && !sibling.Visible) continue;

            // set prev
            prev = sibling;
        }

        // return the previous entity (or null if wasn't found)
        return prev;
    }

    /// <summary>
    ///     Select the first visible and selectable entity to make him the "First Selection".
    /// </summary>
    /// <returns>The first selected and visible entity</returns>
    private void SelectFirstEntity(Entity entity, bool searchInChildren)
    {
        var firstSelectableEntityFound = false;
        var siblings = searchInChildren ? entity._children : _children;

        foreach (var sibling in siblings)
            // if sibling is visible and selectable, use it
            if (sibling.Visible && sibling.Selectable && sibling.Enabled)
            {
                firstSelectableEntityFound = true;

                // reset the IsFirstSelection status of all entities
                if (searchInChildren)
                    entity._children.ForEach(a => a.IsFirstSelection = false);
                else
                    _children.ForEach(a => a.IsFirstSelection = false);

                // if we came from PanelTabs, we need to reset their IsFirstSelection status too
                if (entity.AttachedData != null && entity.AttachedData is TabData)
                    if (((TabData)entity.AttachedData).button.AttachedData != null &&
                        ((TabData)entity.AttachedData).button.AttachedData is PanelTabs)
                    {
                        // resetting the IsFirstSelection status of all tabs 
                        ((PanelTabs)((TabData)entity.AttachedData).button.AttachedData)._children[0]._children
                            .ForEach(a => a.IsFirstSelection = false);

                        // revert the IsFirstSelection status for the current active tab
                        ((PanelTabs)((TabData)entity.AttachedData).button.AttachedData).ActiveTab.button
                            .IsFirstSelection = true;

                        // select the tab
                        ((TabData)entity.AttachedData).button.Checked = true;
                    }

                // make this sibling the first selection in _children
                sibling.IsFirstSelection = true;

                // if it's a button then we want to directly select the corresponding tab
                if (sibling is Button)
                {
                    if (sibling.AttachedData != null && sibling.AttachedData is PanelTabs)
                    {
                        // reset the "_isFirstAfterDraw" value of the current tab so the first selection can happen one more time
                        ((PanelTabs)sibling.AttachedData).ActiveTab.panel._isFirstAfterDraw = true;
                        // select the tab (cursor will select automatically the first entity in the panel)
                        ((Button)sibling).Checked = true;
                    }
                    else
                    {
                        SetCursorPosition(sibling); // if it's not a panel tabs button the set the cursor to it
                    }
                }
                else
                {
                    // set the cursor position to match the entity position
                    SetCursorPosition(sibling);
                }

                // break the chain
                break;
            }

        // if no first selectable entity was found then fallback to the panel tab if possible
        if (firstSelectableEntityFound == false)
            if (entity.AttachedData != null && entity.AttachedData is TabData)
                SetCursorPosition(((TabData)entity.AttachedData).button);
    }

    /// <summary>
    ///     Select the last entity
    /// </summary>
    private void SelectLastEntity(Entity entity, bool searchInChildren)
    {
        // Find the last selectable, visible and enabled entity
        var entityLast = searchInChildren
            ? entity._children.FindLast(e => e.Selectable && e.Visible && !e.IsDisabled())
            : _children.FindLast(e => e.Selectable && e.Visible && !e.IsDisabled());

        if (entityLast != null)
        {
            // removes the "IsFirstSelection" status of this entity
            IsFirstSelection = false;

            // gives the new entity the "IsFirstSelection" status
            entityLast.IsFirstSelection = true;

            // if it's a button then we want to directly select the corresponding tab
            if (entityLast is Button)
            {
                if (entityLast.AttachedData != null && entityLast.AttachedData is PanelTabs)
                {
                    // resetting the IsFirstSelection status of the current tab 
                    ((PanelTabs)entityLast.AttachedData).ActiveTab.button.IsFirstSelection = false;
                    // reset the "_isFirstAfterDraw" value of the current tab so the first selection can happen one more time
                    ((PanelTabs)entityLast.AttachedData).ActiveTab.panel._isFirstAfterDraw = true;

                    // select the tab (cursor will select automatically the first entity in the panel)
                    ((Button)entityLast).Checked = true;
                    // make this tab the first selection
                    ((Button)entityLast).IsFirstSelection = true;
                }
                else
                {
                    SetCursorPosition(entityLast); // if it's not a panel tabs button the set the cursor to it
                }
            }
            else
            {
                // set the cursor position to match the entity position
                SetCursorPosition(entityLast);
            }
        }
    }

    /// <summary>
    ///     Attempts to find the selected entity in all nested children of a root entity
    /// </summary>
    /// <param name="rootEntity"></param>
    /// <returns></returns>
    private Entity RecursiveFindFirstSelection(Entity rootEntity)
    {
        if (rootEntity == null) return null;

        if (rootEntity.IsFirstSelection) return rootEntity;

        foreach (var child in rootEntity._children)
        {
            var searchOutcome = RecursiveFindFirstSelection(child);
            if (searchOutcome != null) return searchOutcome;
        }

        return null;
    }

    /// <summary>
    ///     Select the next visible and selectable entity
    /// </summary>
    private void SelectNextEntity(Entity parent)
    {
        if (Parent != null && Parent._children != null && Parent is RootPanel == false)
        {
            // find the first selection
            var currentEntity = parent._children.Find(x => x.IsFirstSelection);

            // Attempt to find selected entity in immediate children failed, so try all entities
            if (currentEntity == null) currentEntity = RecursiveFindFirstSelection(UserInterface.Active.Root);

            if (currentEntity == null)
                throw new Exception(
                    "No GamePad entry point was found. At least one entity in a panel must have the 'IsFirstSelection' flag.");

            var nextSelectionFound = false;
            for (var i = currentEntity._indexInParent + 1; i < parent._children.Count; i++)
            {
                var nextEntity = parent._children[i];

                // check if the next entity is not null, but visible, selectable and enabled
                if (nextEntity != null &&
                    nextEntity.Visible &&
                    nextEntity.Selectable &&
                    nextEntity.Enabled)
                {
                    nextSelectionFound = true;

                    // removes the "IsFirstSelection" status of this entity
                    currentEntity.IsFirstSelection = false;

                    // gives the new entity the "IsFirstSelection" status
                    nextEntity.IsFirstSelection = true;

                    // if it's a button then we want to directly select the corresponding tab
                    if (nextEntity is Button)
                    {
                        if (nextEntity.AttachedData is PanelTabs)
                        {
                            if (parent.Parent is PanelTabs)
                                // reset the "_isFirstAfterDraw" value so the first selection can happen one more time
                                ((PanelTabs)parent.Parent).ActiveTab.panel._isFirstAfterDraw = true;
                            // select the tab (cursor will select automatically the first entity in the panel)
                            ((Button)nextEntity).Checked = true;
                        }
                        else
                        {
                            SetCursorPosition(nextEntity); // if it's not a panel tabs button the set the cursor to it
                        }
                    }
                    else
                    {
                        // set the cursor position to match the entity position
                        SetCursorPosition(nextEntity);
                    }

                    break;
                }
            }

            // if no selection was found, then select the first entity
            if (!nextSelectionFound) SelectFirstEntity(currentEntity.Parent, true);
        }
    }

    /// <summary>
    ///     Select the previous visible and selectable entity
    /// </summary>
    private void SelectPreviousEntity(Entity parent)
    {
        if (parent != null && parent._children != null && parent is RootPanel == false)
        {
            // find the first selection
            var currentEntity = parent._children.Find(e => e.IsFirstSelection);

            // Attempt to find selected entity in immediate children failed, so try all entities
            if (currentEntity == null) currentEntity = RecursiveFindFirstSelection(UserInterface.Active.Root);

            if (currentEntity == null)
                throw new Exception(
                    "No GamePad entry point was found. At least one entity in a panel must have the 'IsFirstSelection' flag.");

            var previousSelectionFound = false;
            for (var i = currentEntity._indexInParent - 1; i >= 0; i--)
            {
                var previousEntity = parent._children[i];

                // check if the previous entity is not null, but visible, selectable and enabled
                if (previousEntity != null &&
                    previousEntity.Visible &&
                    previousEntity.Selectable &&
                    previousEntity.Enabled)
                {
                    previousSelectionFound = true;

                    // removes the "IsFirstSelection" status of this entity
                    currentEntity.IsFirstSelection = false;

                    // gives the new entity the "IsFirstSelection" status
                    previousEntity.IsFirstSelection = true;

                    // if it's a button then we want to directly select the corresponding tab
                    if (previousEntity is Button)
                    {
                        if (previousEntity.AttachedData is PanelTabs)
                        {
                            if (parent.Parent is PanelTabs)
                                // reset the "_isFirstAfterDraw" value so the first selection can happen one more time
                                ((PanelTabs)parent.Parent).ActiveTab.panel._isFirstAfterDraw = true;
                            // select the tab (cursor will select automatically the first entity in the panel)
                            ((Button)previousEntity).Checked = true;
                        }
                        else
                        {
                            SetCursorPosition(
                                previousEntity); // if it's not a panel tabs button the set the cursor to it
                        }
                    }
                    else
                    {
                        // set the cursor position to match the entity position
                        SetCursorPosition(previousEntity);
                    }

                    break;
                }
            }

            if (!previousSelectionFound) SelectLastEntity(currentEntity.Parent, true);
        }
    }

    private void SelectLeftEntity()
    {
        var lst = new List<Entity>();
        RecursiveSearch(lst, UserInterface.Active.Root, x => x._destRect.Right <= _destRect.Left && x != this);
        if (lst.Count < 1) return;
        var next = lst.OrderByDescending(a => a._destRect.Right)
            .ThenBy(b => Math.Abs(b._destRect.Center.Y - _destRect.Center.Y)).First();
        next.Select();
        next.CalcDestRect();
        next.IsFocused = true;
    }

    private void SelectRightEntity()
    {
        var lst = new List<Entity>();
        RecursiveSearch(lst, UserInterface.Active.Root, x => x._destRect.Left >= _destRect.Right && x != this);
        if (lst.Count < 1) return;
        var next = lst.OrderBy(a => a._destRect.Left).ThenBy(b => Math.Abs(b._destRect.Center.Y - _destRect.Center.Y))
            .First();
        next.Select();
        next.CalcDestRect();
        next.IsFocused = true;
    }

    private void SelectAboveEntity()
    {
        var lst = new List<Entity>();
        RecursiveSearch(lst, UserInterface.Active.Root, x => x._destRect.Bottom <= _destRect.Top && x != this);
        if (lst.Count < 1) return;
        var next = lst.OrderByDescending(a => a._destRect.Bottom)
            .ThenBy(b => Math.Abs(b._destRect.Center.X - _destRect.Center.X)).First();
        next.Select();
        next.CalcDestRect();
        next.IsFocused = true;
    }

    private void SelectBelowEntity()
    {
        var lst = new List<Entity>();
        RecursiveSearch(lst, UserInterface.Active.Root, x => x._destRect.Top >= _destRect.Bottom && x != this);
        if (lst.Count < 1) return;
        var next = lst.OrderBy(a => a._destRect.Top).ThenBy(b => Math.Abs(b._destRect.Center.X - _destRect.Center.X))
            .First();
        next.Select();
        next.CalcDestRect();
        next.IsFocused = true;
    }

    private void RecursiveSearch(List<Entity> entities, Entity current, Func<Entity, bool> predicate)
    {
        for (var i = 0; i < current.Children.Count; i++)
            RecursiveSearch(entities, current.Children[i], predicate);
        if (current.Selectable && current.Visible && predicate(current) && current is Panel == false)
            entities.Add(current);
    }

    /// <summary>
    ///     Sets the new cursor position in the UserInterface.
    /// </summary>
    private void SetCursorPosition(Entity entity)
    {
        if (UserInterface.GetCursorMode == UserInterface.CursorMode.Snapping)
        {
            var ptA = Vector2.Transform(entity._destRect.Location.ToVector2(), Core.Scene.ScreenTransformMatrix);
            var ptD = Vector2.Transform((entity._destRect.Location + entity._destRect.Size).ToVector2(),
                Core.Scene.ScreenTransformMatrix);
            var transRect = new Rectangle((int)ptA.X, (int)ptA.Y, (int)(ptD.X - ptA.X), (int)(ptD.Y - ptA.Y));
            if ((entity is Slider == false || entity is ProgressBar) && entity is VerticalScrollbar == false)
            {
                var cursorDestination = new Vector2(transRect.Center.X, transRect.Center.Y + transRect.Height / 4);

                // Set the cursor position on the center of the entity if it's not a slider or vertical scrollbar
                UserInterface.SetCursorPosition(cursorDestination);
            }
            else if (entity is Slider && entity is ProgressBar == false)
            {
                // Set the cursor position on the center of the mark of the entity if it's a slider or vertical scrollbar
                UserInterface.SetCursorPosition(new Vector2(
                    ((Slider)entity).MarkRec.Center.X, ((Slider)entity).MarkRec.Center.Y + transRect.Height / 4));
            }
        }
    }

    /// <summary>
    ///     Handle gamepad button press event.
    /// </summary>
    protected virtual void DoOnGamePadButtonPressed()
    {
        OnGamePadPressed?.Invoke(this);
        UserInterface.Active.OnGamePadPressed?.Invoke(this);
    }

    internal void CheckForGamepadMovement()
    {
        if (Input.GamePadButtonPressed(GamePadButton.DPadDown))
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
            SelectBelowEntity();
        }
        else if (Input.GamePadButtonPressed(GamePadButton.DPadUp))
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
            SelectAboveEntity();
        }
        else if (Input.GamePadButtonPressed(GamePadButton.DPadLeft))
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
            SelectLeftEntity();
        }
        else if (Input.GamePadButtonPressed(GamePadButton.DPadRight))
        {
            UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
            SelectRightEntity();
        }
    }

    private Panel GetNextPanelInTabs(Panel currentPanel)
    {
        var possiblePanels = currentPanel.Parent.Children.Select(x => (Panel)x).ToList();
        return currentPanel._indexInParent + 1 >= possiblePanels.Count
            ? currentPanel
            : possiblePanels[currentPanel._indexInParent + 1];
    }

    private Panel GetPreviousPanelInTabs(Panel currentPanel)
    {
        var possiblePanels = currentPanel.Parent.Children.Select(x => (Panel)x).ToList();
        return currentPanel._indexInParent - 1 < 0 ? currentPanel : possiblePanels[currentPanel._indexInParent - 1];
    }

    public void Select()
    {
        _selectionOnDstCalc = true;
    }

    /// <summary>
    ///     Update Slider state and mouse cursor position.
    /// </summary>
    private void UpdateSlider()
    {
        if (Input.ThumbStickLeftCanDrag)
            if (this is Slider || this is VerticalScrollbar)
            {
                ((Slider)this).DoOnGamePadMove();
                SetCursorPosition(this);
                Input.ThumbStickLeftCanDrag = false;
                MarkAsDirty();
            }
    }

    /// <summary>
    ///     Handle gamepad up event.
    /// </summary>
    protected virtual void DoOnGamePadReleased()
    {
        OnGamePadReleased?.Invoke(this);
        UserInterface.Active.OnGamePadReleased?.Invoke(this);

        if (!Input.GamePadButtonHeldDown(GamePadButton.DPadLeft) &&
            !Input.GamePadButtonHeldDown(GamePadButton.DPadRight)) IsGamepadAButtonHeldForDrag = false;
    }

    /// <summary>
    ///     Handle mouse down event.
    /// </summary>
    protected virtual void DoOnMouseDown()
    {
        OnMouseDown?.Invoke(this);
        UserInterface.Active.OnMouseDown?.Invoke(this);
    }

    /// <summary>
    ///     Handle mouse up event.
    /// </summary>
    protected virtual void DoOnMouseReleased()
    {
        OnMouseReleased?.Invoke(this);
        UserInterface.Active.OnMouseReleased?.Invoke(this);
    }

    /// <summary>
    ///     Handle mouse click event.
    /// </summary>
    protected virtual void DoOnClick()
    {
        OnClick?.Invoke(this);
        UserInterface.Active.OnClick?.Invoke(this);
    }

    public void SimulateClick()
    {
        var inspectedElement = this;
        while (inspectedElement is not null)
        {
            if (!inspectedElement.Enabled || !inspectedElement.Visible) return;
            inspectedElement = inspectedElement.Parent;
        }

        DoOnClick();
    }

    /// <summary>
    ///     Handle mouse down event, called every frame while down.
    /// </summary>
    protected virtual void DoWhileMouseDown()
    {
        WhileMouseDown?.Invoke(this);
        UserInterface.Active.WhileMouseDown?.Invoke(this);

        if (!Input.GamePadButtonHeldDown(GamePadButton.DPadLeft) && !Input.GamePadButtonHeldDown(
                                                                     GamePadButton.DPadRight)
                                                                 && !Input.GamePadButtonHeldDown(GamePadButton
                                                                     .DPadUp) && !Input.GamePadButtonHeldDown(
                                                                     GamePadButton.DPadDown))
            UpdateSlider();
    }

    /// <summary>
    ///     Handle mouse hover event, called every frame while hover.
    /// </summary>
    protected virtual void DoWhileMouseHover()
    {
        WhileMouseHover?.Invoke(this);
        UserInterface.Active.WhileMouseHover?.Invoke(this);
    }

    /// <summary>
    ///     Handle mouse hover or down event, called every frame while hover.
    /// </summary>
    protected virtual void DoWhileMouseHoverOrDown()
    {
        // invoke callback and global callback
        WhileMouseHoverOrDown?.Invoke(this);
        UserInterface.Active.WhileMouseHoverOrDown?.Invoke(this);

        // do right mouse click event
        if (Input.MouseButtonClick(MouseButton.Right))
        {
            OnRightClick?.Invoke(this);
            UserInterface.Active.OnRightClick?.Invoke(this);
        }
        // do right mouse down event
        else if (Input.MouseButtonPressed(MouseButton.Right))
        {
            OnRightMouseDown?.Invoke(this);
            UserInterface.Active.OnRightMouseDown?.Invoke(this);
        }
        // do while right mouse down even
        else if (Input.MouseButtonHeldDown(MouseButton.Right))
        {
            WhileRightMouseDown?.Invoke(this);
            UserInterface.Active.WhileRightMouseDown?.Invoke(this);
        }
    }

    /// <summary>
    ///     Handle value change event (for entities with value).
    /// </summary>
    protected virtual void DoOnValueChange()
    {
        OnValueChange?.Invoke(this);
        UserInterface.Active.OnValueChange?.Invoke(this);
    }

    /// <summary>
    ///     Handle mouse enter event.
    /// </summary>
    protected virtual void DoOnMouseEnter()
    {
        OnMouseEnter?.Invoke(this);
        UserInterface.Active.OnMouseEnter?.Invoke(this);
    }

    /// <summary>
    ///     Handle mouse leave event.
    /// </summary>
    protected virtual void DoOnMouseLeave()
    {
        OnMouseLeave?.Invoke(this);
        UserInterface.Active.OnMouseLeave?.Invoke(this);
    }

    /// <summary>
    ///     Handle start dragging event.
    /// </summary>
    protected virtual void DoOnStartDrag()
    {
        OnStartDrag?.Invoke(this);
        UserInterface.Active.OnStartDrag?.Invoke(this);
    }

    /// <summary>
    ///     Handle end dragging event.
    /// </summary>
    protected virtual void DoOnStopDrag()
    {
        OnStopDrag?.Invoke(this);
        UserInterface.Active.OnStopDrag?.Invoke(this);
    }

    /// <summary>
    ///     Handle the while-dragging event.
    /// </summary>
    protected virtual void DoWhileDragging()
    {
        WhileDragging?.Invoke(this);
        UserInterface.Active.WhileDragging?.Invoke(this);
    }

    /// <summary>
    ///     Handle when mouse wheel scroll and this entity is the active entity.
    /// </summary>
    protected virtual void DoOnMouseWheelScroll()
    {
        OnMouseWheelScroll?.Invoke(this);
        UserInterface.Active.OnMouseWheelScroll?.Invoke(this);
    }

    /// <summary>
    ///     Called every frame after update.
    /// </summary>
    protected virtual void DoAfterUpdate()
    {
        AfterUpdate?.Invoke(this);
        UserInterface.Active.AfterUpdate?.Invoke(this);
    }

    /// <summary>
    ///     Called every time the visibility property of this entity changes.
    /// </summary>
    protected virtual void DoOnVisibilityChange()
    {
        // invoke callbacks
        OnVisiblityChange?.Invoke(this);
        UserInterface.Active.OnVisiblityChange?.Invoke(this);
    }

    /// <summary>
    ///     Called every frame before update.
    /// </summary>
    protected virtual void DoBeforeUpdate()
    {
        BeforeUpdate?.Invoke(this);
        UserInterface.Active.BeforeUpdate?.Invoke(this);
    }

    /// <summary>
    ///     Called every time this entity is focused / unfocused.
    /// </summary>
    protected virtual void DoOnFocusChange()
    {
        OnFocusChange?.Invoke(this);
        UserInterface.Active.OnFocusChange?.Invoke(this);
    }

    /// <summary>
    ///     Test if a given point is inside entity's boundaries.
    /// </summary>
    /// <remarks>This function result is affected by the 'UseActualSizeForCollision' flag.</remarks>
    /// <param name="point">Point to test.</param>
    /// <returns>True if point is in entity's boundaries (destination rectangle)</returns>
    public virtual bool IsInsideEntity(Vector2 point)
    {
        // adjust scrolling
        point += _lastScrollVal.ToVector2();

        // get rectangle for the test
        var rect = UseActualSizeForCollision ? GetActualDestRect() : _destRect;

        // now test detection
        return point.X >= rect.Left - ExtraMargin.X && point.X <= rect.Right + ExtraMargin.X &&
               point.Y >= rect.Top - ExtraMargin.Y && point.Y <= rect.Bottom + ExtraMargin.Y;
    }

    /// <summary>
    ///     Return true if this entity is naturally interactable, like buttons, lists, etc.
    ///     Entities that are not naturally interactable are things like paragraph, colored rectangle, icon, etc.
    /// </summary>
    /// <remarks>
    ///     This function should be overrided and implemented by different entities, and either return constant True or
    ///     False.
    /// </remarks>
    /// <returns>True if entity is naturally interactable.</returns>
    public virtual bool IsNaturallyInteractable()
    {
        return false;
    }

    /// <summary>
    ///     Mark that this entity boundaries or style changed and it need to recalculate cached destination rect and other
    ///     things.
    /// </summary>
    public void MarkAsDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    ///     Remove the IsDirty flag.
    /// </summary>
    /// <param name="updateChildren">If true, will set a flag that will still make children update.</param>
    internal void ClearDirtyFlag(bool updateChildren = false)
    {
        _isDirty = false;
        if (updateChildren) _destRectVersion++;
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
    protected virtual void UpdateChildren(ref Entity targetEntity, ref Entity dragTargetEntity,
        ref bool wasEventHandled, Point scrollVal)
    {
        // update all children (note: we go in reverse order so that entities on front will receive events before entites on back.
        var childrenSorted = GetSortedChildren();
        for (var i = childrenSorted.Count - 1; i >= 0; i--)
            childrenSorted[i].Update(ref targetEntity, ref dragTargetEntity, ref wasEventHandled, scrollVal);
    }

    /// <summary>
    ///     Called every frame to update entity state and call events.
    /// </summary>
    /// <param name="targetEntity">The deepest child entity with highest priority that we point on and can be interacted with.</param>
    /// <param name="dragTargetEntity">
    ///     The deepest child dragable entity with highest priority that we point on and can be drag
    ///     if mouse down.
    /// </param>
    /// <param name="wasEventHandled">Set to true if current event was already handled by a deeper child.</param>
    /// <param name="scrollVal">Combined scrolling value (panels with scrollbar etc) of all parents.</param>
    public virtual void Update(ref Entity targetEntity, ref Entity dragTargetEntity, ref bool wasEventHandled,
        Point scrollVal)
    {
        // set last scroll var
        _lastScrollVal = scrollVal;

        // check if should invoke the spawn effect
        if (_isFirstUpdate)
        {
            DoOnFirstUpdate();
            _isFirstUpdate = false;
        }

        // if inherit parent state just copy it and stop
        if (InheritParentState)
        {
            _entityState = _parent._entityState;
            _isMouseOver = _parent._isMouseOver;
            IsFocused = _parent.IsFocused;
            _isCurrentlyDisabled = _parent._isCurrentlyDisabled;
            return;
        }

        // get mouse position
        var mousePos = GetMousePos();

        // add our own scroll value to the combined scroll val before updating children
        scrollVal += OverflowScrollVal;

        // get if disabled
        _isCurrentlyDisabled = IsDisabled();

        // if disabled, invisible, or locked - skip
        if (_isCurrentlyDisabled || IsLocked() || !IsVisible())
        {
            // if this very entity is locked (eg not locked due to parent being locked),
            // iterate children and invoke those with DoEventsIfDirectParentIsLocked setting
            if (Locked)
                for (var i = _children.Count - 1; i >= 0; i--)
                    if (_children[i].DoEventsIfDirectParentIsLocked)
                        _children[i].Update(ref targetEntity, ref dragTargetEntity, ref wasEventHandled, scrollVal);

            // if was previously interactable, we might need to trigger some events
            if (_isInteractable)
            {
                // if mouse was over, trigger mouse leave event
                if (_entityState == EntityState.MouseHover)
                {
                    DoOnMouseLeave();
                }
                // if mouse was down, trigger mouse up and leave events
                else if (_entityState == EntityState.MouseDown)
                {
                    DoOnMouseReleased();
                    DoOnMouseLeave();
                }
            }

            // set to default and return
            _isInteractable = false;
            _entityState = EntityState.Default;
            return;
        }

        // if click-through is true, update children and stop here
        if (ClickThrough)
        {
            UpdateChildren(ref targetEntity, ref dragTargetEntity, ref wasEventHandled, scrollVal);
            return;
        }

        // update dest rect if needed (dest rect is calculated before draw, but is used for mouse collision detection as well,
        // so its better to calculate it here too in case something changed).
        UpdateDestinationRectsIfDirty();

        // set if interactable
        _isInteractable = true;

        // do before update event
        DoBeforeUpdate();

        // store previous state
        var prevState = _entityState;

        // store previous mouse-over state
        var prevMouseOver = _isMouseOver;

        // STEP 1: FIRST WE CALCULATE ENTITY STATE (EG MOUSE HOVER / MOUSE DOWN / ..)

        // only if event was not already catched by another entity, check for events
        if (!wasEventHandled)
        {
            // if need to calculate state locally:
            if (!InheritParentState)
            {
                // reset the mouse-over flag
                _isMouseOver = false;
                _entityState = EntityState.Default;

                // set mouse state
                if (IsInsideEntity(mousePos))
                {
                    // set self as the current target, unless a sibling got the event first
                    if (targetEntity == null || targetEntity._parent != _parent) targetEntity = this;

                    // mouse is over entity
                    _isMouseOver = true;

                    // update mouse state
                    _entityState = (IsFocused || PromiscuousClicksMode || Input.MouseButtonPressed()) &&
                                   Input.MouseButtonHeldDown()
                        ? EntityState.MouseDown
                        : EntityState.MouseHover;

                    if (((IsFocused || PromiscuousClicksMode ||
                          Input.MouseButtonPressed() || Input.GamePadButtonPressed(GamePadButton.A_Button)) &&
                         Input.MouseButtonHeldDown()) || Input.GamePadButtonHeldDown(GamePadButton.A_Button))
                        _entityState = EntityState.MouseDown;
                    else
                        _entityState = EntityState.MouseHover;
                }
            }

            // set if focused
            if (Input.MouseButtonPressed()) IsFocused = _isMouseOver;
        }
        // if currently other entity is targeted and mouse clicked, set focused to false
        else if (Input.MouseButtonClick())
        {
            IsFocused = false;
        }

        // STEP 2: NOW WE CALL ALL CHILDREN'S UPDATE

        // update all children
        UpdateChildren(ref targetEntity, ref dragTargetEntity, ref wasEventHandled, scrollVal);

        // check dragging after children so that the most nested entity gets priority
        if ((_draggable || IsNaturallyInteractable()) && dragTargetEntity == null && _isMouseOver &&
            Input.MouseButtonPressed())
            dragTargetEntity = this;
        if ((_draggable || IsNaturallyInteractable()) && dragTargetEntity == null && _isMouseOver &&
            Input.GamePadButtonPressed(GamePadButton.A_Button))
            dragTargetEntity = this;

        // If A is being held for a slider drag
        if ((Input.GamePadButtonPressed(GamePadButton.DPadRight) || Input.GamePadButtonPressed(GamePadButton.DPadLeft))
            && Input.GamePadButtonHeldDown(GamePadButton.A_Button))
            IsGamepadAButtonHeldForDrag = true;

        // STEP 3: CALL EVENTS
        // if selected target is this
        if (targetEntity == this)
        {
            // handled events
            wasEventHandled = true;

            // call the while-mouse-hover-or-down handler
            DoWhileMouseHoverOrDown();

            // set mouse enter / mouse leave
            if (_isMouseOver && !prevMouseOver) DoOnMouseEnter();

            if (this is Slider && IsGamepadAButtonHeldForDrag)
            {
                UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
                UpdateSlider();
            }


            if (_destRect.Contains(Input.MousePosition)) CheckForGamepadMovement();

            // generate events
            if (prevState != _entityState)
            {
                // mouse down
                if (Input.MouseButtonPressed() || Input.GamePadButtonPressed(GamePadButton.A_Button)) DoOnMouseDown();

                // gamepad down
                if (Input.GamePadButtonPressed(GamePadButton.DPadDown) ||
                    Input.GamePadButtonPressed(GamePadButton.DPadUp) ||
                    Input.GamePadButtonPressed(GamePadButton.DPadLeft) ||
                    Input.GamePadButtonPressed(GamePadButton.DPadRight))
                    DoOnGamePadButtonPressed();

                // gamepad up
                if (Input.GamePadButtonReleased(GamePadButton.A_Button)) DoOnGamePadReleased();
                // mouse up
                if (Input.MouseButtonReleased()) DoOnMouseReleased();
                // mouse click
                if (Input.MouseButtonClick() || Input.GamePadButtonPressed(GamePadButton.A_Button))
                {
                    if (Input.GamePadButtonPressed(GamePadButton.A_Button))
                        UserInterface.GetCursorMode = UserInterface.CursorMode.Snapping;
                    DoOnClick();
                }
            }

            // call the while-mouse-down / while-mouse-hover events
            if (_entityState == EntityState.MouseDown &&
                UserInterface.GetCursorMode == UserInterface.CursorMode.Roaming)
                DoWhileMouseDown();
            else
                DoWhileMouseHover();
        }
        // if not current target, clear entity state
        else
        {
            _entityState = EntityState.Default;
        }

        // mouse leave events
        if (!_isMouseOver && prevMouseOver) DoOnMouseLeave();

        // handle mouse wheel scroll over this entity
        if (targetEntity == this || UserInterface.Active.ActiveEntity == this)
            if (Input.MouseWheelChange != 0)
                DoOnMouseWheelScroll();

        // STEP 4: HANDLE DRAGGING FOR DRAGABLES

        // if draggable, and after calling all the children target is self, it means we are being dragged!
        if (_draggable && dragTargetEntity == this && IsFocused)
        {
            // check if we need to start dragging the entity that was not dragged before
            if (!_isBeingDragged && Input.MousePositionDiff.Length() != 0)
            {
                // remove self from parent and add again. this trick is to keep the dragged entity always on-top
                var parent = _parent;
                RemoveFromParent();
                parent.AddChild(this);

                // set dragging mode = true, and call the do-start-dragging event
                _isBeingDragged = true;
                DoOnStartDrag();
            }

            // if being dragged..
            if (_isBeingDragged)
            {
                // update drag offset and call the dragging event
                _dragOffset += Input.MousePositionDiff;
                DoWhileDragging();
            }
        }
        // if not currently dragged but was dragged last frane, call the end dragging event
        else if (_isBeingDragged)
        {
            _isBeingDragged = false;
            DoOnStopDrag();
            MarkAsDirty();
        }

        // if being dragged, mark as dirty
        if (_isBeingDragged) MarkAsDirty();

        // do after-update events
        DoAfterUpdate();
    }
}