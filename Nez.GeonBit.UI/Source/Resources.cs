﻿#region File Description

//-----------------------------------------------------------------------------
// This file pre-load and hold all the resources (textures, fonts, etc..) that
// GeonBit.UI needs. If you edit and add new files to content, you probably
// need to update this file as well.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.ExtendedContent.DataTypes;
using Nez.GeonBit.UI.Entities;

namespace Nez.GeonBit.UI;

/// <summary>
///     A class to get texture with index and constant path part.
///     Used internally.
/// </summary>
public class TexturesGetter<TEnum> where TEnum : IConvertible
{
    // base path of textures to load (index will be appended to them).
    private readonly string _basepath;

    // textures we already loaded
    private readonly Texture2D[] _loadedTextures;

    // suffix to add to the end of texture path
    private readonly string _suffix;

    // textures types count
    private readonly int _typesCount;

    // do we use states like down / hover / default for these textures?
    private bool _usesStates;

    /// <summary>
    ///     Create the texture getter with base path.
    /// </summary>
    /// <param name="path">Resource path, under geonbit.ui content.</param>
    /// <param name="suffix">Suffix to add to the texture path after the enum part.</param>
    /// <param name="usesStates">If true, it means these textures may also use entit states, eg mouse hover / down / default.</param>
    public TexturesGetter(string path, string suffix = null, bool usesStates = true)
    {
        _basepath = path;
        _suffix = suffix ?? string.Empty;
        _usesStates = usesStates;
        _typesCount = Enum.GetValues(typeof(TEnum)).Length;
        _loadedTextures = new Texture2D[usesStates ? _typesCount * 3 : _typesCount];
    }

    /// <summary>
    ///     Get texture for enum state.
    ///     This is for textures that don't have different states, like mouse hover, down, or default.
    /// </summary>
    /// <param name="i">Texture enum identifier.</param>
    /// <returns>Loaded texture.</returns>
    public Texture2D this[TEnum i]
    {
        // get texture for a given type
        get
        {
            var indx = GetIndex(i);
            if (_loadedTextures[indx] == null)
            {
                var path = Resources._root + _basepath + EnumToString(i) + _suffix;
                _loadedTextures[indx] = Resources._content.Load<Texture2D>(path);
            }

            return _loadedTextures[indx];
        }

        // force-override texture for a given type
        set
        {
            var indx = GetIndex(i);
            _loadedTextures[indx] = value;
        }
    }

    /// <summary>
    ///     Get texture for enum state and entity state.
    ///     This is for textures that don't have different states, like mouse hover, down, or default.
    /// </summary>
    /// <param name="i">Texture enum identifier.</param>
    /// <param name="s">Entity state to get texture for.</param>
    /// <returns>Loaded texture.</returns>
    public Texture2D this[TEnum i, EntityState s]
    {
        // get texture for a given type and state
        get
        {
            var indx = GetIndex(i, s);
            if (_loadedTextures[indx] == null)
            {
                var path = Resources._root + _basepath + EnumToString(i) + _suffix + StateEnumToString(s);
                _loadedTextures[indx] = Resources._content.Load<Texture2D>(path);
            }

            return _loadedTextures[indx];
        }

        // force-override texture for a given type and state
        set
        {
            var indx = GetIndex(i, s);
            _loadedTextures[indx] = value;
        }
    }

    /// <summary>
    ///     Get index from enum type with optional state.
    /// </summary>
    private int GetIndex(TEnum i, EntityState? s = null)
    {
        if (s != null)
            return (int)(object)i + _typesCount * (int)s;
        return (int)(object)i;
    }

    /// <summary>
    ///     Convert enum to its string for filename.
    /// </summary>
    private string EnumToString(TEnum e)
    {
        // entity state enum
        if (typeof(TEnum) == typeof(EntityState)) return StateEnumToString((EntityState)(object)e);

        // icon type enum
        if (typeof(TEnum) == typeof(IconType)) return e.ToString();

        // all other type of enums
        return e.ToString().ToLower();
    }

    /// <summary>
    ///     Convert entity state enum to string.
    /// </summary>
    private string StateEnumToString(EntityState e)
    {
        switch (e)
        {
            case EntityState.MouseDown:
                return "_down";
            case EntityState.MouseHover:
                return "_hover";
            case EntityState.Default:
                return string.Empty;
        }

        return null;
    }
}

/// <summary>
///     A static class to init and store all UI resources (textures, effects, fonts, etc.)
/// </summary>
public static class Resources
{
    /// <summary>Just a plain white texture, used internally.</summary>
    public static Texture2D WhiteTexture;

    /// <summary>Cursor textures.</summary>
    public static TexturesGetter<CursorType> Cursors;

    /// <summary>Metadata about cursor textures.</summary>
    public static CursorTextureData[] CursorsData;

    /// <summary>All panel skin textures.</summary>
    public static TexturesGetter<PanelSkin> PanelTextures;

    /// <summary>Metadata about panel textures.</summary>
    public static TextureData[] PanelData;

    /// <summary>Button textures (accessed as [skin, state]).</summary>
    public static TexturesGetter<ButtonSkin> ButtonTextures;

    /// <summary>CheckBox textures.</summary>
    public static TexturesGetter<EntityState> CheckBoxTextures;

    /// <summary>Radio button textures.</summary>
    public static TexturesGetter<EntityState> RadioTextures;

    /// <summary>ProgressBar texture.</summary>
    public static Texture2D ProgressBarTexture;

    /// <summary>Metadata about progressbar texture.</summary>
    public static TextureData ProgressBarData;

    /// <summary>ProgressBar fill texture.</summary>
    public static Texture2D ProgressBarFillTexture;

    /// <summary>HorizontalLine texture.</summary>
    public static Texture2D HorizontalLineTexture;

    /// <summary>Sliders base textures.</summary>
    public static TexturesGetter<SliderSkin> SliderTextures;

    /// <summary>Sliders mark textures (the sliding piece that shows current value).</summary>
    public static TexturesGetter<SliderSkin> SliderMarkTextures;

    /// <summary>Metadata about slider textures.</summary>
    public static TextureData[] SliderData;

    /// <summary>All icon textures.</summary>
    public static TexturesGetter<IconType> IconTextures;

    /// <summary>Icons inventory background texture.</summary>
    public static Texture2D IconBackgroundTexture;

    /// <summary>Vertical scrollbar base texture.</summary>
    public static Texture2D VerticalScrollbarTexture;

    /// <summary>Vertical scrollbar mark texture.</summary>
    public static Texture2D VerticalScrollbarMarkTexture;

    /// <summary>Arrow-down texture (used in dropdown).</summary>
    public static Texture2D ArrowDown;

    /// <summary>Arrow-up texture (used in dropdown).</summary>
    public static Texture2D ArrowUp;

    /// <summary>Default font types.</summary>
    public static SpriteFont[] Fonts;

    /// <summary>Effect for disabled entities (greyscale).</summary>
    public static Effect DisabledEffect;

    /// <summary>An effect to draw just a silhouette of the texture.</summary>
    public static Effect SilhouetteEffect;

    /// <summary>Store the content manager instance</summary>
    internal static ContentManager _content;

    /// <summary>Root for geonbit.ui content</summary>
    internal static string _root;

    /// <summary>Metadata about button textures.</summary>
    public static TextureData[] ButtonData { get; set; }

    /// <summary>Metadata about scrollbar texture.</summary>
    public static TextureData VerticalScrollbarData { get; set; }

    /// <summary>
    ///     Load all GeonBit.UI resources.
    /// </summary>
    /// <param name="content">Content manager to use.</param>
    /// <param name="theme">Which theme to load resources from.</param>
    public static void LoadContent(ContentManager content, string theme = "default")
    {
        // set resources root path and store content manager
        _content = content;
        _root = "engine/ui/" + theme + "/";

        WhiteTexture = _content.Load<Texture2D>(_root + "textures/white_texture");
        Cursors = new TexturesGetter<CursorType>("textures/cursor_");
        PanelTextures = new TexturesGetter<PanelSkin>("textures/panel_");
        ButtonTextures = new TexturesGetter<ButtonSkin>("textures/button_");
        CheckBoxTextures = new TexturesGetter<EntityState>("textures/checkbox");
        RadioTextures = new TexturesGetter<EntityState>("textures/radio");
        ProgressBarTexture = _content.Load<Texture2D>(_root + "textures/progressbar");
        ProgressBarFillTexture = _content.Load<Texture2D>(_root + "textures/progressbar_fill");
        HorizontalLineTexture = _content.Load<Texture2D>(_root + "textures/horizontal_line");
        SliderTextures = new TexturesGetter<SliderSkin>("textures/slider_");
        SliderMarkTextures = new TexturesGetter<SliderSkin>("textures/slider_", "_mark");
        IconTextures = new TexturesGetter<IconType>("textures/icons/");
        //IconBackgroundTexture = _content.Load<Texture2D>(_root + "textures/icons/background");
        VerticalScrollbarTexture = _content.Load<Texture2D>(_root + "textures/scrollbar");
        VerticalScrollbarMarkTexture = _content.Load<Texture2D>(_root + "textures/scrollbar_mark");
        ArrowDown = _content.Load<Texture2D>(_root + "textures/arrow_down");
        ArrowUp = _content.Load<Texture2D>(_root + "textures/arrow_up");

        // load cursors metadata
        CursorsData = new CursorTextureData[Enum.GetValues(typeof(CursorType)).Length];
        foreach (CursorType cursor in Enum.GetValues(typeof(CursorType)))
        {
            var cursorName = cursor.ToString().ToLower();
            CursorsData[(int)cursor] = content.Load<CursorTextureData>(_root + "textures/cursor_" + cursorName + "_md");
        }

        // load panels
        PanelData = new TextureData[Enum.GetValues(typeof(PanelSkin)).Length];
        foreach (PanelSkin skin in Enum.GetValues(typeof(PanelSkin)))
        {
            // skip none panel skin
            if (skin == PanelSkin.None) continue;

            // load panels metadata
            var skinName = skin.ToString().ToLower();
            PanelData[(int)skin] = content.Load<TextureData>(_root + "textures/panel_" + skinName + "_md");
        }

        // load scrollbar metadata
        VerticalScrollbarData = content.Load<TextureData>(_root + "textures/scrollbar_md");

        // load slider metadata
        SliderData = new TextureData[Enum.GetValues(typeof(SliderSkin)).Length];
        foreach (SliderSkin skin in Enum.GetValues(typeof(SliderSkin)))
        {
            var skinName = skin.ToString().ToLower();
            SliderData[(int)skin] = content.Load<TextureData>(_root + "textures/slider_" + skinName + "_md");
        }

        // load fonts
        Fonts = new SpriteFont[Enum.GetValues(typeof(FontStyle)).Length];
        foreach (FontStyle style in Enum.GetValues(typeof(FontStyle)))
        {
            Fonts[(int)style] = content.Load<SpriteFont>(_root + "fonts/" + style);
            Fonts[(int)style].LineSpacing += 2;
        }

        // load buttons metadata
        ButtonData = new TextureData[Enum.GetValues(typeof(ButtonSkin)).Length];
        foreach (ButtonSkin skin in Enum.GetValues(typeof(ButtonSkin)))
        {
            var skinName = skin.ToString().ToLower();
            ButtonData[(int)skin] = content.Load<TextureData>(_root + "textures/button_" + skinName + "_md");
        }

        // load progress bar metadata
        ProgressBarData = content.Load<TextureData>(_root + "textures/progressbar_md");

        // load effects
        DisabledEffect = content.Load<Effect>(_root + "effects/disabled");
        SilhouetteEffect = content.Load<Effect>(_root + "effects/silhouette");

        // load default styleSheets
        LoadDefaultStyles(ref Entities.Entity.DefaultStyle, "Entity", _root, content);
        LoadDefaultStyles(ref Paragraph.DefaultStyle, "Paragraph", _root, content);
        LoadDefaultStyles(ref Button.DefaultStyle, "Button", _root, content);
        LoadDefaultStyles(ref Button.DefaultParagraphStyle, "ButtonParagraph", _root, content);
        LoadDefaultStyles(ref CheckBox.DefaultStyle, "CheckBox", _root, content);
        LoadDefaultStyles(ref CheckBox.DefaultParagraphStyle, "CheckBoxParagraph", _root, content);
        LoadDefaultStyles(ref ColoredRectangle.DefaultStyle, "ColoredRectangle", _root, content);
        LoadDefaultStyles(ref DropDown.DefaultStyle, "DropDown", _root, content);
        LoadDefaultStyles(ref DropDown.DefaultParagraphStyle, "DropDownParagraph", _root, content);
        LoadDefaultStyles(ref DropDown.DefaultSelectedParagraphStyle, "DropDownSelectedParagraph", _root, content);
        LoadDefaultStyles(ref Header.DefaultStyle, "Header", _root, content);
        LoadDefaultStyles(ref HorizontalLine.DefaultStyle, "HorizontalLine", _root, content);
        LoadDefaultStyles(ref Icon.DefaultStyle, "Icon", _root, content);
        LoadDefaultStyles(ref Image.DefaultStyle, "Image", _root, content);
        LoadDefaultStyles(ref Label.DefaultStyle, "Label", _root, content);
        LoadDefaultStyles(ref Panel.DefaultStyle, "Panel", _root, content);
        LoadDefaultStyles(ref ProgressBar.DefaultStyle, "ProgressBar", _root, content);
        LoadDefaultStyles(ref ProgressBar.DefaultFillStyle, "ProgressBarFill", _root, content);
        LoadDefaultStyles(ref RadioButton.DefaultStyle, "RadioButton", _root, content);
        LoadDefaultStyles(ref RadioButton.DefaultParagraphStyle, "RadioButtonParagraph", _root, content);
        LoadDefaultStyles(ref SelectList.DefaultStyle, "SelectList", _root, content);
        LoadDefaultStyles(ref SelectList.DefaultParagraphStyle, "SelectListParagraph", _root, content);
        LoadDefaultStyles(ref Slider.DefaultStyle, "Slider", _root, content);
        LoadDefaultStyles(ref TextInput.DefaultStyle, "TextInput", _root, content);
        LoadDefaultStyles(ref TextInput.DefaultParagraphStyle, "TextInputParagraph", _root, content);
        LoadDefaultStyles(ref TextInput.DefaultPlaceholderStyle, "TextInputPlaceholder", _root, content);
        LoadDefaultStyles(ref VerticalScrollbar.DefaultStyle, "VerticalScrollbar", _root, content);
        LoadDefaultStyles(ref PanelTabs.DefaultButtonStyle, "PanelTabsButton", _root, content);
        LoadDefaultStyles(ref PanelTabs.DefaultButtonParagraphStyle, "PanelTabsButtonParagraph", _root, content);
    }

    /// <summary>
    ///     Load default stylesheets for a given entity name and put values inside the sheet.
    /// </summary>
    /// <param name="sheet">StyleSheet to load.</param>
    /// <param name="entityName">Entity unique identifier for file names.</param>
    /// <param name="themeRoot">Path of the current theme root directory.</param>
    /// <param name="content">Content manager to allow us to load xmls.</param>
    private static void LoadDefaultStyles(ref StyleSheet sheet, string entityName, string themeRoot,
        ContentManager content)
    {
        // get stylesheet root path (eg everything before the state part)
        var stylesheetBase = themeRoot + "styles/" + entityName;

        // load default styles
        FillDefaultStyles(ref sheet, EntityState.Default, content.Load<DefaultStyles>(stylesheetBase + "-Default"));

        // load mouse-hover styles
        FillDefaultStyles(ref sheet, EntityState.MouseHover,
            content.Load<DefaultStyles>(stylesheetBase + "-MouseHover"));

        // load mouse-down styles
        FillDefaultStyles(ref sheet, EntityState.MouseDown, content.Load<DefaultStyles>(stylesheetBase + "-MouseDown"));
    }

    /// <summary>
    ///     Fill a set of default styles into a given stylesheet.
    /// </summary>
    /// <param name="sheet">StyleSheet to fill.</param>
    /// <param name="state">State to fill values for.</param>
    /// <param name="styles">Default styles, as loaded from xml file.</param>
    private static void FillDefaultStyles(ref StyleSheet sheet, EntityState state, DefaultStyles styles)
    {
        if (styles.FillColor != null) sheet[state + "." + "FillColor"] = new StyleProperty((Color)styles.FillColor);
        if (styles.FontStyle != null) sheet[state + "." + "FontStyle"] = new StyleProperty((int)styles.FontStyle);
        if (styles.ForceAlignCenter != null)
            sheet[state + "." + "ForceAlignCenter"] = new StyleProperty((bool)styles.ForceAlignCenter);
        if (styles.OutlineColor != null)
            sheet[state + "." + "OutlineColor"] = new StyleProperty((Color)styles.OutlineColor);
        if (styles.OutlineWidth != null)
            sheet[state + "." + "OutlineWidth"] = new StyleProperty((int)styles.OutlineWidth);
        if (styles.Scale != null) sheet[state + "." + "Scale"] = new StyleProperty((float)styles.Scale);
        if (styles.SelectedHighlightColor != null)
            sheet[state + "." + "SelectedHighlightColor"] = new StyleProperty((Color)styles.SelectedHighlightColor);
        if (styles.ShadowColor != null)
            sheet[state + "." + "ShadowColor"] = new StyleProperty((Color)styles.ShadowColor);
        if (styles.ShadowOffset != null)
            sheet[state + "." + "ShadowOffset"] = new StyleProperty((Vector2)styles.ShadowOffset);
        if (styles.Padding != null) sheet[state + "." + "Padding"] = new StyleProperty((Vector2)styles.Padding);
        if (styles.SpaceBefore != null)
            sheet[state + "." + "SpaceBefore"] = new StyleProperty((Vector2)styles.SpaceBefore);
        if (styles.SpaceAfter != null)
            sheet[state + "." + "SpaceAfter"] = new StyleProperty((Vector2)styles.SpaceAfter);
        if (styles.ShadowScale != null)
            sheet[state + "." + "ShadowScale"] = new StyleProperty((float)styles.ShadowScale);
    }
}