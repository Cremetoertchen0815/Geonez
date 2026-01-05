#region File Description

//-----------------------------------------------------------------------------
// A paragraph extension that support multiple fill colors (change colors via
// special color commands).
//
// Author: Justin Gattuso, Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.UI.Exceptions;

namespace Nez.GeonBit.UI.Entities;

/// <summary>Hold color instructions for MultiColor paragraphs.</summary>
[Serializable]
public class ColorInstruction
{
    // dictionary with available colors and the code to use them
    internal static Dictionary<string, Color> _colors = new()
    {
        { "RED", Color.Red },
        { "BLUE", Color.Blue },
        { "GREEN", Color.Green },
        { "YELLOW", Color.Yellow },
        { "BROWN", Color.Brown },
        { "BLACK", Color.Black },
        { "WHITE", Color.White },
        { "CYAN", Color.Cyan },
        { "PINK", Color.Pink },
        { "GRAY", Color.Gray },
        { "MAGENTA", Color.Magenta },
        { "ORANGE", Color.Orange },
        { "PURPLE", Color.Purple },
        { "SILVER", Color.Silver },
        { "GOLD", Color.Gold },
        { "TEAL", Color.Teal },
        { "NAVY", Color.Navy },
        { "LIME", Color.Lime }
    };

    // color for this instruction

    // should we use the paragraph original color?

    /// <summary>Constructor to use when creating a color instruction.</summary>
    /// <param name="sColor">The string representation of the color to use for rendering.</param>
    public ColorInstruction(string sColor)
    {
        // use default paragraph fill color
        if (sColor == "DEFAULT")
            UseFillColor = true;
        // change to custom color
        else
            Color = StringToColor(sColor);
    }

    /// <summary>Flag represents whether or not to use the default FillColor for this instruction.</summary>
    public bool UseFillColor { get; }

    /// <summary>If UseFillColor is false the color here is used for the instruction.</summary>
    public Color Color { get; } = Color.White;

    /// <summary>
    ///     Add a custom color we can use in the multicolor paragraph.
    ///     You can also override one of the built-in colors.
    /// </summary>
    /// <param name="key">Color key to use this color.</param>
    /// <param name="color">Color to set.</param>
    public static void AddCustomColor(string key, Color color)
    {
        _colors[key] = color;
    }

    /// <summary>
    ///     Converts a supported color string to its respective color.
    ///     Supported colors are as follows:
    ///     red, blue, green, yellow, brown, black, white, cyan, pink, gray, magenta, orange, purple, silver, gold, teal or
    ///     default (White)
    ///     Also supports custom colors added via AddCustomColor() and any valid XNA hex color string(eg #ffffffff).
    /// </summary>
    /// <param name="sColor">The color represented as a string value; any invalid value will default to White.</param>
    /// <returns>The actual color object or White as the fallback default.</returns>
    public Color StringToColor(string sColor)
    {
        if (sColor.StartsWith("#")) return sColor.FromRGBAHex();


        if (!_colors.TryGetValue(sColor, out var outColor))
        {
            if (UserInterface.Active.SilentSoftErrors) return Color.White;
            throw new InvalidValueException("Unknown color code '" + sColor + "'.");
        }

        return outColor;
    }
}

/// <summary>
///     Multicolor Paragraph is a paragraph that supports in-text color tags that changes the fill color of the text.
/// </summary>
[Serializable]
public class MulticolorParagraph : Paragraph
{
    /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    // regex to find color instructions
    private static Regex colorInstructionsRegex = new("{{[^{}]*}}");

    // color-changing instructions in current paragraph. key is char index, value is color change command.
    private Dictionary<int, ColorInstruction> _colorInstructions = new();

    // are color instructions currently enabled
    private bool _enableColorInstructions = true;

    // do we need to update color-related stuff?
    private bool _needUpdateColors = true;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static MulticolorParagraph()
    {
        MakeSerializable(typeof(MulticolorParagraph));
    }

    /// <summary>
    ///     Create the paragraph.
    /// </summary>
    /// <param name="text">Paragraph text (accept new line characters).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="scale">Optional font size.</param>
    public MulticolorParagraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null,
        float? scale = null) :
        base(text, anchor, size, offset, scale)
    {
    }

    /// <summary>
    ///     Create default multicolor paragraph with empty text.
    /// </summary>
    public MulticolorParagraph() : this(string.Empty)
    {
    }

    /// <summary>
    ///     Create the paragraph with optional fill color and font size.
    /// </summary>
    /// <param name="text">Paragraph text (accept new line characters).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="color">Text fill color.</param>
    /// <param name="scale">Optional font size.</param>
    /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
    /// <param name="offset">Offset from anchor position.</param>
    public MulticolorParagraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null,
        Vector2? offset = null) :
        base(text, anchor, color, scale, size, offset)
    {
    }

    /// <summary>
    ///     If true, will enable color instructions (special codes that change fill color inside the text).
    /// </summary>
    public bool EnableColorInstructions
    {
        // get if color instructions are enabled
        get => _enableColorInstructions;

        // set if color instructions are enabled and parse instructions if needed
        set
        {
            _enableColorInstructions = value;
            _needUpdateColors = true;
        }
    }

    /// <summary>Get / Set the paragraph text.</summary>
    public override string Text
    {
        get => _text;
        set
        {
            var txt = _enableColorInstructions ? value : TextModifier(value);
            if (_text != txt)
            {
                _text = txt;
                MarkAsDirty();
                if (EnableColorInstructions) _needUpdateColors = true;
            }
        }
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();
        _needUpdateColors = true;
    }

    /// <summary>
    ///     Parse special color-changing instructions inside the text.
    /// </summary>
    private void ParseColorInstructions()
    {
        // clear previous color instructions
        _colorInstructions.Clear();

        // if color instructions are disabled, stop here
        if (!EnableColorInstructions) return;

        // find and parse color instructions
        if (_text.Contains("{{"))
        {
            var iLastLength = 0;

            var oMatches = colorInstructionsRegex.Matches(_text);
            foreach (Match oMatch in oMatches)
            {
                var sColor = oMatch.Value.Substring(2, oMatch.Value.Length - 4);
                var key = oMatch.Index - iLastLength;
                if (_colorInstructions.ContainsKey(key))
                    _colorInstructions[key] = new ColorInstruction(sColor);
                else
                    _colorInstructions.Add(key, new ColorInstruction(sColor));
                iLastLength += oMatch.Value.Length;
            }

            // Strip out the color instructions from the text to allow the rest of processing to process the actual text content
            _text = TextModifier(colorInstructionsRegex.Replace(_text, string.Empty));
        }

        // no longer need to update colors
        _needUpdateColors = false;
    }

    /// <summary>
    ///     Draw entity outline. Note: in paragraph its a special case and we implement it inside the DrawEntity function.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="screenMatrix">The screen matrix.</param>
    protected override void DrawEntityOutline(SpriteBatch spriteBatch, Matrix screenMatrix)
    {
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // update processed text if needed
        if (_needUpdateColors)
        {
            ParseColorInstructions();
            UpdateDestinationRects();
        }

        // get outline width
        var outlineWidth = OutlineWidth;

        // if we got outline draw it
        if (outlineWidth > 0)
        {
            // get outline color
            var outlineColor = UserInterface.Active.DrawUtils.FixColorOpacity(OutlineColor);

            // for not-too-thick outline we render just two corners
            if (outlineWidth <= MaxOutlineWidthToOptimize)
            {
                spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.One * outlineWidth, outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.One * outlineWidth, outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
            }
            // for really thick outline we need to cover the other corners as well
            else
            {
                spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.UnitX * outlineWidth,
                    outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.UnitX * outlineWidth,
                    outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(_currFont, _processedText, _position + Vector2.UnitY * outlineWidth,
                    outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(_currFont, _processedText, _position - Vector2.UnitY * outlineWidth,
                    outlineColor,
                    0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);
            }
        }

        // if there are color changing instructions in paragraph, draw with color changes
        if (_colorInstructions.Count > 0)
        {
            var iTextIndex = 0;
            var oColor = FillColor;
            var oCharacterSize = GetCharacterActualSize();
            var oCurrentPosition = new Vector2(_position.X - oCharacterSize.X, _position.Y);
            foreach (var cCharacter in _processedText)
            {
                if (_colorInstructions.TryGetValue(iTextIndex, out var colorInstruction))
                {
                    if (colorInstruction.UseFillColor)
                        oColor = FillColor;
                    else
                        oColor = colorInstruction.Color;
                }

                if (cCharacter == '\n')
                {
                    oCurrentPosition.X = _position.X - oCharacterSize.X;
                    oCurrentPosition.Y += _currFont.LineSpacing * _actualScale;
                }
                else
                {
                    iTextIndex++;
                    oCurrentPosition.X += oCharacterSize.X;
                }

                // fix color opacity and draw
                var fillCol = UserInterface.Active.DrawUtils.FixColorOpacity(oColor);
                spriteBatch.DrawString(_currFont, cCharacter.ToString(), oCurrentPosition, fillCol, 0, _fontOrigin,
                    _actualScale, SpriteEffects.None, 0.5f);
            }
        }
        // if there are no color-changing instructions, just draw the paragraph as-is
        else
        {
            base.DrawEntity(spriteBatch, phase);
        }
    }
}