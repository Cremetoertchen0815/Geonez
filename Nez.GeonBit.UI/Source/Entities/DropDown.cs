﻿#region File Description

//-----------------------------------------------------------------------------
// DropDown is a sub-class of SelectList - it works the same, but in DropDown we
// only see the currently selected value in a special box, and only when its
// clicked the list becomes visible and you can select from it.
//
// DropDown gives you list functionality, for much less UI space!
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     DropDown is just like a list, but it only shows the currently selected value unless clicked on (the list is
///     only revealed while interacted with).
/// </summary>
[Serializable]
public class DropDown : Entity
{
    /// <summary>Default style for the dropdown itself. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default styling for dropdown labels. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultParagraphStyle = new();

    /// <summary>Default styling for the dropdown currently-selected label. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultSelectedParagraphStyle = new();

    /// <summary>Default select list size in pixels.</summary>
    public new static Vector2 DefaultSize = new(0f, 220f);

    /// <summary>
    ///     Default height, in pixels, of the selected text panel.
    /// </summary>
    public static int SelectedPanelHeight = 67;

    /// <summary>
    ///     Size of the arrow to show on the side of the Selected Text Panel.
    /// </summary>
    public static int ArrowSize = 30;

    // last known selected index
    private int _lastSelected = -1;

    // dictionary of special events for specific items selection
    private Dictionary<string, Action> _perItemCallbacks = new();

    // text used as placeholder when nothing is selected.
    private string _placeholderText = "Click to Select";

    // internal panel and paragraph to show selected value.

    // an internal select list used when dropdown is opened.

    /// <summary>
    ///     If true, will auto-set the internal list height based on number of options.
    /// </summary>
    public bool AutoSetListHeight = false;

    /// <summary>
    ///     If set to true, whenever user select an item it will trigger event but jump back to placeholder value.
    /// </summary>
    public bool DontKeepSelection = false;

    /// <summary>Special callback to execute when list size changes.</summary>
    [XmlIgnore] public EventCallback OnListChange = null;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static DropDown()
    {
        MakeSerializable(typeof(DropDown));
    }

    /// <summary>
    ///     Create the DropDown list.
    /// </summary>
    /// <param name="size">List size (refers to the whole size of the list + the header when dropdown list is opened).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="skin">Panel skin to use for this DropDown list and header.</param>
    /// <param name="listSkin">An optional skin to use for the dropdown list only (if you want a different skin for the list).</param>
    /// <param name="showArrow">If true, will show an up/down arrow next to the dropdown text.</param>
    public DropDown(Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null,
        PanelSkin skin = PanelSkin.ListBackground, PanelSkin? listSkin = null, bool showArrow = true) :
        base(size, anchor, offset)
    {
        // default padding of self is 0
        Padding = Vector2.Zero;

        // to get collision right when list is opened
        UseActualSizeForCollision = true;

        if (!UserInterface.Active._isDeserializing)
        {
            // create the panel and paragraph used to show currently selected value (what's shown when drop-down is closed)
            SelectedTextPanel = new Panel(new Vector2(0, SelectedPanelHeight), skin, Anchor.TopLeft);
            SelectedTextPanelParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.CenterLeft);
            SelectedTextPanelParagraph.UseActualSizeForCollision = false;
            SelectedTextPanelParagraph.UpdateStyle(SelectList.DefaultParagraphStyle);
            SelectedTextPanelParagraph.UpdateStyle(DefaultParagraphStyle);
            SelectedTextPanelParagraph.UpdateStyle(DefaultSelectedParagraphStyle);
            SelectedTextPanelParagraph.Identifier = "_selectedTextParagraph";
            SelectedTextPanel.AddChild(SelectedTextPanelParagraph, true);
            SelectedTextPanel._hiddenInternalEntity = true;
            SelectedTextPanel.Identifier = "_selectedTextPanel";

            // create the arrow down icon
            ArrowDownImage = new Image(Resources.ArrowDown, new Vector2(ArrowSize, ArrowSize), ImageDrawMode.Stretch,
                Anchor.CenterRight, new Vector2(-10, 0));
            SelectedTextPanel.AddChild(ArrowDownImage, true);
            ArrowDownImage._hiddenInternalEntity = true;
            ArrowDownImage.Identifier = "_arrowDownImage";
            ArrowDownImage.Visible = showArrow;

            // create the list component
            SelectList = new SelectList(new Vector2(0f, size.Y), Anchor.TopCenter, Vector2.Zero, listSkin ?? skin);

            // update list offset and space before
            SelectList.SetOffset(new Vector2(0, SelectedPanelHeight));
            SelectList.SpaceBefore = Vector2.Zero;
            SelectList._hiddenInternalEntity = true;
            SelectList.Identifier = "_selectList";

            // add the header and select list as children
            AddChild(SelectedTextPanel);
            AddChild(SelectList);

            InitEvents();
        }
        // if during serialization create just a temp placeholder
        else
        {
            SelectList = new SelectList(new Vector2(0f, size.Y), Anchor.TopCenter, Vector2.Zero, listSkin ?? skin);
        }
    }

    /// <summary>
    ///     Create default dropdown.
    /// </summary>
    public DropDown() : this(new Vector2(0, 200))
    {
    }

    /// <summary>Default text to show when no value is selected from the list.</summary>
    public string DefaultText
    {
        get => _placeholderText;
        set => _placeholderText = value;
    }

    /// <summary>
    ///     Get the selected text panel (what's shown when DropDown is closed).
    /// </summary>
    public Panel SelectedTextPanel { get; private set; }

    /// <summary>
    ///     If true and user clicks on the item currently selected item, it will still invoke value change event as if
    ///     a new value was selected.
    /// </summary>
    public bool AllowReselectValue
    {
        get => SelectList.AllowReselectValue;
        set => SelectList.AllowReselectValue = value;
    }

    /// <summary>
    ///     Get the drop-down list component.
    /// </summary>
    public SelectList SelectList { get; private set; }

    /// <summary>
    ///     Get the selected text panel paragraph (the text that's shown when DropDown is closed).
    /// </summary>
    public Paragraph SelectedTextPanelParagraph { get; private set; }

    /// <summary>
    ///     Get the image entity of the arrow on the side of the Selected Text Panel.
    /// </summary>
    public Image ArrowDownImage { get; private set; }

    /// <summary>
    ///     Is the DropDown list currentle opened (visible).
    /// </summary>
    public bool ListVisible
    {
        // get if the list is visible
        get => SelectList.Visible;

        // show / hide the list
        set
        {
            // show / hide list
            SelectList.Visible = value;
            OnDropDownVisibilityChange();
        }
    }

    /// <summary>
    ///     Set entity render and update priority.
    ///     DropDown entity override this function to give some bonus priority, since when list is opened it needs to override
    ///     entities
    ///     under it, which usually have bigger index in container.
    /// </summary>
    public override int Priority => 100 - _indexInParent + PriorityBonus;

    /// <summary>
    ///     Return if currently have a selected value.
    /// </summary>
    public bool HasSelectedValue => SelectedIndex != -1;

    /// <summary>
    ///     Currently selected item value (or null if none is selected).
    /// </summary>
    public string SelectedValue
    {
        get => SelectList.SelectedValue;
        set => SelectList.SelectedValue = value;
    }

    /// <summary>
    ///     Currently selected item index (or -1 if none is selected).
    /// </summary>
    public int SelectedIndex
    {
        get => SelectList.SelectedIndex;
        set => SelectList.SelectedIndex = value;
    }

    /// <summary>
    ///     Current scrollbar position.
    /// </summary>
    public int ScrollPosition
    {
        get => SelectList.ScrollPosition;
        set => SelectList.ScrollPosition = value;
    }

    /// <summary>
    ///     How many items currently in the list.
    /// </summary>
    public int Count => SelectList.Count;

    /// <summary>
    ///     Is the list currently empty.
    /// </summary>
    public bool Empty => SelectList.Empty;

    /// <summary>
    ///     Init event-related stuff after all sub-entities are created.
    /// </summary>
    private void InitEvents()
    {
        // add callback on list value change
        SelectList.OnValueChange = _ =>
        {
            // hide list
            ListVisible = false;

            // set selected text
            SelectedTextPanelParagraph.Text = SelectedValue ?? DefaultText;
        };

        // on click, always hide the selectlist
        SelectList.OnClick = _ => { ListVisible = false; };

        // hide the list by default
        SelectList.Visible = false;

        // setup the callback to show / hide the list when clicking the dropbox
        SelectedTextPanel.OnClick = _ =>
        {
            // change visibility
            ListVisible = !ListVisible;
        };

        // set starting text
        SelectedTextPanelParagraph.Text = SelectedValue ?? DefaultText;

        // update styles
        SelectList.UpdateStyle(DefaultStyle);

        // make the list events trigger the dropdown events
        SelectList.PropagateEventsTo(this);

        // make the selected value panel trigger the dropdown events
        SelectedTextPanel.PropagateEventsTo(this);
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();

        SelectedTextPanel = Find<Panel>("_selectedTextPanel");
        SelectedTextPanel._hiddenInternalEntity = true;

        ArrowDownImage = SelectedTextPanel.Find<Image>("_arrowDownImage");
        ArrowDownImage._hiddenInternalEntity = true;

        SelectList = Find<SelectList>("_selectList");
        SelectList._hiddenInternalEntity = true;

        SelectedTextPanelParagraph = SelectedTextPanel.Find("_selectedTextParagraph") as Paragraph;

        InitEvents();
    }

    /// <summary>
    ///     Set special callback to trigger if a specific value is selected.
    /// </summary>
    /// <param name="itemValue">Item text to trigger event.</param>
    /// <param name="action">Event to trigger.</param>
    public void OnSelectedSpecificItem(string itemValue, Action action)
    {
        _perItemCallbacks[itemValue] = action;
    }

    /// <summary>
    ///     Clear all the per-item specific events.
    /// </summary>
    public void ClearSpecificItemEvents()
    {
        _perItemCallbacks.Clear();
    }

    /// <summary>
    ///     Return the actual dest rect for auto-anchoring purposes.
    ///     This is useful for things like DropDown, that when opened they take a larger part of the screen, but we don't
    ///     want it to push down other entities.
    /// </summary>
    protected override Rectangle GetDestRectForAutoAnchors()
    {
        SelectedTextPanel.UpdateDestinationRectsIfDirty();
        return SelectedTextPanel.GetActualDestRect();
    }

    /// <summary>
    ///     Test if a given point is inside entity's boundaries.
    /// </summary>
    /// <remarks>This function result is affected by the 'UseActualSizeForCollision' flag.</remarks>
    /// <param name="point">Point to test.</param>
    /// <returns>True if point is in entity's boundaries (destination rectangle)</returns>
    public override bool IsInsideEntity(Vector2 point)
    {
        // adjust scrolling
        point += _lastScrollVal.ToVector2();

        // get destination rect based on whether the dropdown is opened or closed
        Rectangle rect;

        // if list is currently visible, use the full size
        if (ListVisible)
        {
            SelectList.UpdateDestinationRectsIfDirty();
            rect = SelectList.GetActualDestRect();
            rect.Height += SelectedPanelHeight;
            rect.Y -= SelectedPanelHeight;
        }
        // if list is not currently visible, use the header size
        else
        {
            SelectedTextPanel.UpdateDestinationRectsIfDirty();
            rect = SelectedTextPanel.GetActualDestRect();
        }

        // now test detection
        return point.X >= rect.Left && point.X <= rect.Right &&
               point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    /// <summary>
    ///     Called whenever the dropdown list is shown / hidden.
    ///     Note: called *after* _isListVisible is set.
    /// </summary>
    private void OnDropDownVisibilityChange()
    {
        // if during deserialize, skip
        if (UserInterface.Active._isDeserializing)
            return;

        // update arrow image
        ArrowDownImage.Texture = ListVisible ? Resources.ArrowUp : Resources.ArrowDown;

        // focus on selectlist
        SelectList.IsFocused = true;
        UserInterface.Active.ActiveEntity = SelectList;

        // update destination rectangles
        SelectList.UpdateDestinationRects();

        // if turned visible, scroll to selected
        if (SelectList.Visible) SelectList.ScrollToSelected();

        // mark self as dirty
        MarkAsDirty();

        // do auto-height
        if (AutoSetListHeight) SelectList.MatchHeightToList();
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        if (SelectedIndex == -1 && SelectedTextPanelParagraph.Text != _placeholderText)
            SelectedTextPanelParagraph.Text = _placeholderText;
    }

    /// <summary>
    ///     Called every frame after update.
    ///     DropDown entity override this function to close the list if necessary and to remove the selected item panel from
    ///     self.
    /// </summary>
    protected override void DoAfterUpdate()
    {
        // if list currently visible we want to check if we need to close it
        if (ListVisible)
        {
            // check if mouse down and not inside list
            var mousePosition = GetMousePos();
            if (Input.AnyMouseButtonPressed() && !IsInsideEntity(mousePosition)) ListVisible = false;
        }

        // call base do-before-update
        base.DoAfterUpdate();

        // do we have a selected item?
        if (HasSelectedValue)
        {
            // trigger per-item events, but only if value changed
            if (SelectedIndex != _lastSelected)
                if (_perItemCallbacks.TryGetValue(SelectList.SelectedValue, out var callback))
                    callback.Invoke();

            // if set to not keep selected value, return to original placeholder
            if (DontKeepSelection && SelectedIndex != -1) Unselect();
        }

        // store last known index
        _lastSelected = SelectedIndex;
    }

    /// <summary>
    ///     Clear current selection.
    /// </summary>
    public void Unselect()
    {
        SelectList.Unselect();
    }

    /// <summary>
    ///     Add value to list.
    /// </summary>
    /// <remarks>
    ///     Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value
    ///     (will always pick the first found).
    /// </remarks>
    /// <param name="value">Value to add.</param>
    public void AddItem(string value)
    {
        SelectList.AddItem(value);
    }

    /// <summary>
    ///     Add value to list at a specific index.
    /// </summary>
    /// <remarks>
    ///     Values can be duplicated, however, this will cause annoying behavior when trying to delete or select by value
    ///     (will always pick the first found).
    /// </remarks>
    /// <param name="value">Value to add.</param>
    /// <param name="index">Index to insert the new item into.</param>
    public void AddItem(string value, int index)
    {
        SelectList.AddItem(value, index);
    }

    /// <summary>
    ///     Remove value from the list.
    /// </summary>
    /// <param name="value">Value to remove.</param>
    public void RemoveItem(string value)
    {
        SelectList.RemoveItem(value);
    }

    /// <summary>
    ///     Remove item from the list, by index.
    /// </summary>
    /// <param name="index">Index of the item to remove.</param>
    public void RemoveItem(int index)
    {
        SelectList.RemoveItem(index);
    }

    /// <summary>
    ///     Remove all items from the list.
    /// </summary>
    public void ClearItems()
    {
        SelectList.ClearItems();
    }

    /// <summary>
    ///     Is the list a natrually-interactable entity.
    /// </summary>
    /// <returns>True.</returns>
    public override bool IsNaturallyInteractable()
    {
        return true;
    }

    /// <summary>
    ///     Move scrollbar to currently selected item.
    /// </summary>
    public void ScrollToSelected()
    {
        SelectList.ScrollToSelected();
    }

    /// <summary>
    ///     Move scrollbar to last item in list.
    /// </summary>
    public void scrollToEnd()
    {
        SelectList.scrollToEnd();
    }
}