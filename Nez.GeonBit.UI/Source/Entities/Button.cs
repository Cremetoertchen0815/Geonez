﻿#region File Description

//-----------------------------------------------------------------------------
// Buttons are clickable entities you can put content inside and respond with 
// callback when clicked. By default, every button is created with an internal
// paragraph to describe it. If you don't want the internal text just set it to 
// empty string.
// 
// Buttons are very basic UI elements and often used for things like "Save Game", 
// "Load", "Exit", etc..
//
// Note that buttons can also behave like checkboxes with the 'ToggleMode' option.
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
///     Button skins.
/// </summary>
public enum ButtonSkin
{
    /// <summary>The default button skin.</summary>
    Default = 0,

    /// <summary>Alternative button skin.</summary>
    Alternative = 1,

    /// <summary>Fancy button skin.</summary>
    Fancy = 2
}

/// <summary>
///     A clickable button with label on it.
/// </summary>
[Serializable]
public class Button : Entity
{
    /// <summary>Default button size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(0f, 70f);

    /// <summary>Default styling for the button itself. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default styling for buttons label. Note: loaded from UI theme xml file.</summary>
    public static StyleSheet DefaultParagraphStyle = new();

    // button value when in toggle mode
    private bool _checked;

    /// <summary>Frame width for when using custom skin.</summary>
    private Vector2 _customFrame = Vector2.Zero;

    /// <summary>Optional custom skin that override's the default theme button textures.</summary>
    private Texture2D[] _customSkin;

    // button skin
    private ButtonSkin _skin;

    /// <summary>Button label. Use this if you want to change the button text or font style.</summary>
    public Paragraph ButtonParagraph;

    /// <summary>
    ///     If true, button will behave as a checkbox, meaning when you click on it it will stay pressed and have
    ///     'checked' value of true.
    /// </summary>
    public bool ToggleMode = false;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Button()
    {
        MakeSerializable(typeof(Button));
    }

    /// <summary>
    ///     Create the button.
    /// </summary>
    /// <param name="text">Text for the button label.</param>
    /// <param name="skin">Button skin (texture to use).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="size">Button size (if not defined will use default size).</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Button(string text, ButtonSkin skin = ButtonSkin.Default, Anchor anchor = Anchor.Auto, Vector2? size = null,
        Vector2? offset = null) :
        base(size, anchor, offset)
    {
        // store style
        _skin = skin;

        // update styles
        UpdateStyle(DefaultStyle);

        MilkFactor = 0.4f;

        if (!UserInterface.Active._isDeserializing)
        {
            // create and set button paragraph
            ButtonParagraph = UserInterface.DefaultParagraph(text, Anchor.Center);
            ButtonParagraph._hiddenInternalEntity = true;
            ButtonParagraph.UpdateStyle(DefaultParagraphStyle);
            ButtonParagraph.Identifier = "_button_caption";
            AddChild(ButtonParagraph, true);
        }
    }

    /// <summary>
    ///     Create button with default params and without text.
    /// </summary>
    public Button() : this(string.Empty)
    {
    }

    /// <summary>
    ///     Set / get current button skin.
    /// </summary>
    public ButtonSkin Skin
    {
        get => _skin;
        set
        {
            _skin = value;
            _customSkin = null;
        }
    }

    /// <summary>
    ///     When button is in Toggle mode, this is the current value (it button checked or not).
    /// </summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            DoOnValueChange();
        }
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();
        ButtonParagraph = Find("_button_caption") as Paragraph;
        ButtonParagraph._hiddenInternalEntity = true;
    }

    /// <summary>
    ///     Override the default theme textures and set a custom skin for this specific button.
    /// </summary>
    /// <remarks>You must provide all state textures when overriding button skin.</remarks>
    /// <param name="defaultTexture">Texture to use for default state.</param>
    /// <param name="mouseHoverTexture">Texture to use when mouse hover over the button.</param>
    /// <param name="mouseDownTexture">Texture to use when mouse button is down over this button.</param>
    /// <param name="frameWidth">The width of the custom texture's frame, in percents of texture size.</param>
    public void SetCustomSkin(Texture2D defaultTexture, Texture2D mouseHoverTexture, Texture2D mouseDownTexture,
        Vector2? frameWidth = null)
    {
        _customSkin = new Texture2D[3];
        _customSkin[(int)EntityState.Default] = defaultTexture;
        _customSkin[(int)EntityState.MouseHover] = mouseHoverTexture;
        _customSkin[(int)EntityState.MouseDown] = mouseDownTexture;
        _customFrame = frameWidth ?? _customFrame;
    }

    /// <summary>
    ///     Is the button a natrually-interactable entity.
    /// </summary>
    /// <returns>True.</returns>
    public override bool IsNaturallyInteractable()
    {
        return true;
    }

    /// <summary>
    ///     Handle click events. In button we override this so we can toggle button when in Toggle mode.
    /// </summary>
    protected override void DoOnClick()
    {
        // toggle value
        if (ToggleMode) Checked = !Checked;

        // call base handler
        base.DoOnClick();
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // get mouse state for graphics
        var state = _entityState;
        if (Checked) state = EntityState.MouseDown;

        // get texture based on skin and state
        var data = Resources.ButtonData[(int)_skin];

        if (data.StainedCanvasID < 0)
        {
            var texture = _customSkin == null ? Resources.ButtonTextures[_skin, state] : _customSkin[(int)state];

            // get frame width
            var frameSize = _customSkin == null ? new Vector2(data.FrameWidth, data.FrameHeight) : _customFrame;

            // draw the button background with frame
            if (frameSize.Length() > 0)
            {
                var scale = frameSize.Y > 0 ? Scale : 1f;
                UserInterface.Active.DrawUtils.DrawSurface(spriteBatch, texture, _destRect, frameSize, 1, FillColor,
                    scale);
            }
            // draw the button background without frame (just stretch texture)
            else
            {
                UserInterface.Active.DrawUtils.DrawImage(spriteBatch, texture, _destRect, FillColor);
            }
        }
        else
        {
            if (UserInterface.StainedCanvasEnabled)
            {
                var tex = UserInterface.Active.GetCanvasTexture(data.StainedCanvasID);
                var nuSize = new Vector2(data.FrameWidth, data.FrameHeight) * _destRect.Size.ToVector2();
                var srcRect = new Rectangle((_destRect.Center.ToVector2() - nuSize * 0.5f).ToPoint(),
                    nuSize.ToPoint());
                UserInterface.Active.DrawUtils.DrawImage(spriteBatch, tex, _destRect, FillColor, 1, srcRect);
                spriteBatch.DrawRect(_destRect, FillColor * MilkFactor);
            }
            else
            {
                spriteBatch.DrawRect(_destRect, FillColor with { A = 255 });
            }
        }

        // call base draw function
        base.DrawEntity(spriteBatch, phase);
    }
}