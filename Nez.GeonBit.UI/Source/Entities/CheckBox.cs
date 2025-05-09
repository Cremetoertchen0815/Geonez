﻿#region File Description

//-----------------------------------------------------------------------------
// CheckBoxes are inline paragraphs with a little square next to them that can either
// be checked or unchecked.
// 
// They are useful for binary settings, like fullscreen mode, mute / unmute,
// play hardcore mode, etc.
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
///     A checkbox entity, eg a label with a square you can mark as checked or uncheck.
///     Holds a boolean value.
/// </summary>
[Serializable]
public class CheckBox : Entity
{
    // checkbox widget size (the graphic box part)
    private static Vector2 CHECKBOX_SIZE = new(35, 35);

    /// <summary>Default checkbox size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(0f, 40f);

    /// <summary>Default styling for the checkbox itself. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default styling for checkbox label. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultParagraphStyle = new();

    /// <summary>Current checkbox value.</summary>
    protected bool _value;

    /// <summary>CheckBox label. Use this if you want to change the checkbox text or font style.</summary>
    public Paragraph TextParagraph;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static CheckBox()
    {
        MakeSerializable(typeof(CheckBox));
    }

    /// <summary>
    ///     Create a new checkbox entity.
    /// </summary>
    /// <param name="text">CheckBox label text.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="size">CheckBox size.</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="isChecked">If true, this checkbox will be created as 'checked'.</param>
    public CheckBox(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null,
        bool isChecked = false) :
        base(size, anchor, offset)
    {
        // update default style
        UpdateStyle(DefaultStyle);

        // create and set checkbox paragraph
        if (!UserInterface.Active._isDeserializing)
        {
            TextParagraph = UserInterface.DefaultParagraph(text, Anchor.CenterLeft);
            TextParagraph.UpdateStyle(DefaultParagraphStyle);
            TextParagraph.SetOffset(new Vector2(25, 0));
            TextParagraph._hiddenInternalEntity = true;
            TextParagraph.Identifier = "_checkbox_text";
            AddChild(TextParagraph, true);
        }

        // checkboxes are promiscuous by default.
        PromiscuousClicksMode = true;

        // set value
        Checked = isChecked;
    }

    /// <summary>
    ///     Create checkbox without text.
    /// </summary>
    public CheckBox() : this(string.Empty)
    {
    }

    /// <summary>
    ///     CheckBox current value, eg if its checked or unchecked.
    /// </summary>
    public bool Checked
    {
        get => _value;
        set
        {
            _value = value;
            DoOnValueChange();
        }
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();
        TextParagraph = Find("_checkbox_text") as Paragraph;
        TextParagraph._hiddenInternalEntity = true;
    }

    /// <summary>
    ///     Is the checkbox a natrually-interactable entity.
    /// </summary>
    /// <returns>True.</returns>
    public override bool IsNaturallyInteractable()
    {
        return true;
    }

    /// <summary>
    ///     Helper function to get checkbox texture based on state and current value.
    /// </summary>
    /// <returns>Which texture to use for the checkbox.</returns>
    protected virtual Texture2D GetTexture()
    {
        var state = _entityState;
        if (state != EntityState.MouseDown && Checked) state = EntityState.MouseDown;
        return Resources.CheckBoxTextures[state];
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // get texture based on checkbox / mouse state
        var texture = GetTexture();

        // calculate actual size
        var actualSize = CHECKBOX_SIZE * GlobalScale;

        // dest rect
        var dest = new Rectangle(_destRect.X,
            (int)(_destRect.Y + _destRect.Height / 2 - actualSize.Y / 2),
            (int)actualSize.X,
            (int)actualSize.Y);
        dest = UserInterface.Active.DrawUtils.ScaleRect(dest, Scale);

        // source rect
        var src = new Rectangle(0, 0, texture.Width, texture.Height);

        // draw checkbox
        spriteBatch.Draw(texture, dest, src, FillColor);

        // call base draw function
        base.DrawEntity(spriteBatch, phase);
    }

    /// <summary>
    ///     Handle mouse click event.
    ///     CheckBox entity override this function to handle value toggle.
    /// </summary>
    protected override void DoOnClick()
    {
        // toggle value
        Checked = !_value;

        // call base handler
        base.DoOnClick();
    }
}