﻿#region File Description

//-----------------------------------------------------------------------------
// Paragraph is a simple text to display.
// It support multilines, outline color, different colors for when mouse hover
// or click, auto word wrap, and align to center.
//
// Note that by default paragraph align based on its anchor, eg. anchoring right
// will align right, left will align left, and anything with center will align
// to center. This behavior can be overrided with the AlignCenter property.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ExtendedContent.DataTypes;
using Nez.GeonBit.UI.Exceptions;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     Font styles.
/// </summary>
public enum FontStyle
{
    /// <summary>Regular font.</summary>
    Regular,

    /// <summary>Bold font.</summary>
    Bold,

    /// <summary>Italic font.</summary>
    Italic
}

/// <summary>
///     Paragraph is a renderable text. It can be multiline, wrap words, have outline, etc.
/// </summary>
[Serializable]
public class Paragraph : Entity
{
    /// <summary>Default styling for paragraphs. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>
    ///     If the outline width is less than this value, the outline will be optimized but will appear slightly less
    ///     sharp on corners.
    /// </summary>
    protected static int MaxOutlineWidthToOptimize = 1;

    /// <summary>Base font size. Change this property to affect the size of all paragraphs and other text entities.</summary>
    public static float BaseSize = 1f;

    // text actual destination rect
    private Rectangle _actualDestRect;

    /// <summary>
    ///     Calculated, final text scale.
    /// </summary>
    protected float _actualScale;

    // should we add a hyphen whenever we break words?
    private bool _addHyphenWhenBreakWord = true;

    // should we break words too long if in wrap mode?
    private bool _breakWordsIfMust = true;

    /// <summary>
    ///     Current font used.
    /// </summary>
    protected SpriteFont _currFont;

    /// <summary>
    ///     Calculated font draw origin.
    /// </summary>
    protected Vector2 _fontOrigin;

    /// <summary>
    ///     Calculated text position.
    /// </summary>
    protected Vector2 _position;

    /// <summary>
    ///     Actual processed text to display (after word-wrap etc).
    /// </summary>
    protected string _processedText;

    /// <summary>
    ///     Paragraph's current text.
    /// </summary>
    protected string _text = string.Empty;

    // is wrap-words enabled?
    private bool _wrapWords = true;

    /// <summary>
    ///     Optional background color for text.
    /// </summary>
    public Color BackgroundColor = Color.Transparent;

    /// <summary>
    ///     Extra padding for background color.
    /// </summary>
    public Point BackgroundColorPadding = new(10, 10);

    /// <summary>
    ///     If true and have background color, will use the paragraph box size for it instead of the text actual size.
    /// </summary>
    public bool BackgroundColorUseBoxSize = false;

    /// <summary>
    ///     An optional font you can set to override the default fonts.
    ///     NOTE! Only monospace fonts are supported!
    /// </summary>
    [XmlIgnore] public SpriteFont FontOverride = null;

    // the size of a single space character with current font.
    private Vector2 SingleCharacterSize;

    public Func<string, string> TextModifier = x => x;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Paragraph()
    {
        MakeSerializable(typeof(Paragraph));
    }

    /// <summary>
    ///     Create the paragraph.
    /// </summary>
    /// <param name="text">Paragraph text (accept new line characters).</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="size">Paragraph size (note: not font size, but the region that will contain the paragraph).</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="scale">Optional font size.</param>
    public Paragraph(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null,
        float? scale = null) :
        base(size, anchor, offset)
    {
        Text = text;
        UpdateStyle(DefaultStyle);
        if (scale != null) SetStyleProperty(StylePropertyIds.Scale, new StyleProperty((float)scale));
        UpdateFontPropertiesIfNeeded();
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
    public Paragraph(string text, Anchor anchor, Color color, float? scale = null, Vector2? size = null,
        Vector2? offset = null) :
        this(text, anchor, size, offset)
    {
        SetStyleProperty(StylePropertyIds.FillColor, new StyleProperty(color));
        if (scale != null) SetStyleProperty(StylePropertyIds.Scale, new StyleProperty((float)scale));
        UpdateFontPropertiesIfNeeded();
    }

    /// <summary>
    ///     Create default paragraph without text.
    /// </summary>
    public Paragraph() : this(string.Empty)
    {
    }

    /// <summary>Get / Set the paragraph text.</summary>
    public virtual string Text
    {
        get => _text;
        set
        {
            var txt = TextModifier(value);
            if (_text != txt)
            {
                _text = txt;
                MarkAsDirty();
            }
        }
    }

    /// <summary>
    ///     Get / Set word wrap mode.
    ///     If true, and text exceeded destination width, the paragraph will wrap words by adding line breaks where needed.
    /// </summary>
    public bool WrapWords
    {
        get => _wrapWords;
        set
        {
            _wrapWords = value;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     If WrapWords is true and there's a word that's too long (eg longer than max width), will break the word in the
    ///     middle.
    ///     If false, word wrap will only break lines in between words (eg spaces) and never break words.
    /// </summary>
    public bool BreakWordsIfMust
    {
        get => _breakWordsIfMust;
        set
        {
            _breakWordsIfMust = value;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     If true and a long word is broken due to word wrap, will add hyphen at the breaking point.
    /// </summary>
    public bool AddHyphenWhenBreakWord
    {
        get => _addHyphenWhenBreakWord;
        set
        {
            _addHyphenWhenBreakWord = value;
            MarkAsDirty();
        }
    }

    /// <summary>
    ///     Current font style - this is just a sugarcoat to access the default font style property.
    /// </summary>
    [XmlIgnore]
    public FontStyle TextStyle
    {
        set => SetStyleProperty("FontStyle", new StyleProperty((int)value));
        get => (FontStyle)GetActiveStyle("FontStyle").asInt;
    }

    /// <summary>
    ///     Should we align text to center - this is just a sugarcoat to access the default force-align-to-center style
    ///     property.
    /// </summary>
    [XmlIgnore]
    public bool AlignToCenter
    {
        set => SetStyleProperty("ForceAlignCenter", new StyleProperty(value));
        get => GetActiveStyle("ForceAlignCenter").asBool;
    }

    /// <summary>
    ///     Get the actual destination rect that this paragraph takes (based on text content, font size, and word wrap).
    /// </summary>
    /// <returns>Actual paragraph destination rect.</returns>
    public override Rectangle GetActualDestRect()
    {
        return _actualDestRect;
    }

    /// <summary>
    ///     Get the size, in pixels, of a single character in paragraph.
    /// </summary>
    /// <returns>Actual size, in pixels, of a single character.</returns>
    public Vector2 GetCharacterActualSize()
    {
        GetCurrFont();
        var scale = Scale * BaseSize * GlobalScale;
        return SingleCharacterSize * scale;
    }

    /// <summary>
    ///     Wrap text to fit destination rect.
    ///     Most if this code is coming from: http://stackoverflow.com/questions/15986473/how-do-i-implement-word-wrap
    /// </summary>
    /// <param name="font">Font of the text to wrap.</param>
    /// <param name="text">Text content.</param>
    /// <param name="maxLineWidth">Max line width to wrap.</param>
    /// <param name="fontSize">Font scale (scale you are about to use when drawing the text).</param>
    /// <returns>Text that is wrapped to fit the given length (by adding line breaks at the right places).</returns>
    public string WrapText(SpriteFont font, string text, float maxLineWidth, float fontSize)
    {
        // invalid width (can happen during init steps - skip
        if (maxLineWidth <= 0) return text;

        // create string to return as result
        var ret = new StringBuilder(string.Empty);

        // if text got line breaks, break into lines and process them seperately
        if (text?.Contains("\n") ?? false)
        {
            // break into lines
            var lines = text.Split('\n');

            // iterate lines and wrap them
            foreach (var line in lines) ret.AppendLine(WrapText(font, line, maxLineWidth, fontSize));

            // remove the last extra linebreak that was added in this process and return.
            ret = ret.Remove(ret.Length - 1, 1);
            return ret.ToString();
        }

        // if got here it means we are processing a single line. break it into words.
        // note: we use a list so we can push words in the middle while iterating (to handle words too long).
        var words = new List<string>(text?.Split(' ') ?? []);

        // iterate words
        var currWidth = 0;
        for (var i = 0; i < words.Count; ++i)
        {
            // is it last word?
            var lastWord = i == words.Count - 1;

            // get current word and its width
            var word = words[i];
            var wordWidth = (int)((font.MeasureString(word).X + SingleCharacterSize.X) * fontSize);

            // special case: word itself is longer than line width
            if (BreakWordsIfMust && wordWidth >= maxLineWidth && word.Length >= 4)
            {
                // find breaking position
                var breakPos = 0;
                var currWordWidth = (int)(SingleCharacterSize.X * fontSize);
                foreach (var c in word)
                {
                    currWordWidth += (int)(font.MeasureString(c.ToString()).X * fontSize);
                    if (currWordWidth >= maxLineWidth) break;
                    breakPos++;
                }

                breakPos -= 3;
                if (breakPos >= word.Length - 1) breakPos -= 2;
                if (breakPos <= 0) breakPos = 1;

                // break the word into two and add to the list of words after this position.
                // we will process them in next loop iterations.
                var firstHalf = word.Substring(0, breakPos);
                var secondHalf = word.Substring(breakPos, word.Length - breakPos);
                if (AddHyphenWhenBreakWord) firstHalf += '-';
                words.Insert(i + 1, firstHalf);
                words.Insert(i + 2, secondHalf);

                // continue to skip current word (it will be added later, with its broken parts)
                continue;
            }

            // add to total width
            currWidth += wordWidth;

            // did overflow max width? add line break and reset current width.
            if (currWidth >= maxLineWidth)
            {
                ret.Append('\n');
                ret.Append(word);
                if (!lastWord) ret.Append(' ');
                currWidth = wordWidth;
            }
            // if didn't overflow just add the word as-is
            else
            {
                ret.Append(word);
                if (!lastWord) ret.Append(' ');
            }
        }

        // remove the extra space that was appended to the end during the process and return wrapped text.
        //ret = ret.Remove(ret.Length - 1, 1);

        // special case - if last word was just the size of the line, it will add a useless trailing \n and create double line breaks.
        // remove that extra line break.
        if (ret.Length > 0 && ret[ret.Length - 1] == '\n') ret = ret.Remove(ret.Length - 1, 1);

        // return the final wrapped text
        return ret.ToString();
    }

    /// <summary>
    ///     Return the processed text that is actually displayed on screen, after word-wrap etc.
    /// </summary>
    /// <returns>Actual displayed text with word-wrap and other runtime processing.</returns>
    public string GetProcessedText()
    {
        return _processedText;
    }

    /// <summary>
    ///     Get the currently active font for this paragraph.
    /// </summary>
    /// <returns>Current font.</returns>
    public SpriteFont GetCurrFont()
    {
        return FontOverride ?? Resources.Fonts[(int)TextStyle];
    }

    /// <summary>
    ///     Update dest rect and internal dest rect.
    ///     This is called internally whenever a change is made to the entity or its parent.
    /// </summary>
    public override void UpdateDestinationRects()
    {
        // call base function
        base.UpdateDestinationRects();

        // do extra preperation for text entities
        CalcTextActualRectWithWrap();
    }

    /// <summary>
    ///     Update font-related properties, if needed.
    /// </summary>
    private void UpdateFontPropertiesIfNeeded()
    {
        var font = GetCurrFont();
        if (font != _currFont)
        {
            // mark as dirty so we'll recalculate positions and line breaks
            MarkAsDirty();

            // set font and get single character size
            _currFont = font;
            SingleCharacterSize = _currFont.MeasureString(" ");

            // sanity test
            if (SingleCharacterSize.X * 2 != _currFont.MeasureString("!.").X)
                throw new InvalidValueException("Cannot use non-monospace fonts!");
        }
    }

    /// <summary>
    ///     Calculate the paragraph actual destination rect with word-wrap and other factors taken into consideration.
    /// </summary>
    public void CalcTextActualRectWithWrap()
    {
        // update font properties
        UpdateFontPropertiesIfNeeded();

        // calc actual scale
        var actualScale = Scale * BaseSize * GlobalScale;
        if (actualScale != _actualScale)
        {
            _actualScale = actualScale;
            MarkAsDirty();
        }

        // get text and add things like line-breaks to wrap words etc.
        var newProcessedText = Text;
        if (WrapWords) newProcessedText = WrapText(_currFont, newProcessedText, _destRect.Width, _actualScale);

        // if processed text changed
        if (newProcessedText != _processedText)
        {
            _processedText = newProcessedText;
            MarkAsDirty();
        }

        // due to the mechanism of calculating destination rect etc based on parent and anchor,
        // to set text alignment all we need to do is keep the size the actual text size.
        // so we just update _size every frame and the text alignemtn (left, right, center..) fix itself by the destination rect.
        _fontOrigin = Vector2.Zero;
        _position = new Vector2(_destRect.X, _destRect.Y);
        var size = _currFont.MeasureString(_processedText);

        // set position and origin based on anchor.
        // note: no top-left here because thats the default set above.
        var alreadyCentered = false;
        switch (_anchor)
        {
            case Anchor.Center:
                _fontOrigin = size / 2;
                _position += new Vector2(_destRect.Width / 2, _destRect.Height / 2);
                alreadyCentered = true;
                break;
            case Anchor.AutoCenter:
            case Anchor.TopCenter:
                _fontOrigin = new Vector2(size.X / 2, 0);
                _position += new Vector2(_destRect.Width / 2, 0f);
                alreadyCentered = true;
                break;
            case Anchor.TopRight:
                _fontOrigin = new Vector2(size.X, 0);
                _position += new Vector2(_destRect.Width, 0f);
                break;
            case Anchor.BottomCenter:
                _fontOrigin = new Vector2(size.X / 2, size.Y);
                _position += new Vector2(_destRect.Width / 2, _destRect.Height);
                alreadyCentered = true;
                break;
            case Anchor.BottomRight:
                _fontOrigin = new Vector2(size.X, size.Y);
                _position += new Vector2(_destRect.Width, _destRect.Height);
                break;
            case Anchor.BottomLeft:
                _fontOrigin = new Vector2(0f, size.Y);
                _position += new Vector2(0f, _destRect.Height);
                break;
            case Anchor.CenterLeft:
                _fontOrigin = new Vector2(0f, size.Y / 2);
                _position += new Vector2(0f, _destRect.Height / 2);
                break;
            case Anchor.CenterRight:
                _fontOrigin = new Vector2(size.X, size.Y / 2);
                _position += new Vector2(_destRect.Width, _destRect.Height / 2);
                break;
        }

        // force center align
        if (AlignToCenter && !alreadyCentered)
        {
            _fontOrigin.X = size.X / 2;
            _position.X = _destRect.X + _destRect.Width / 2;
        }

        // set actual height
        _actualDestRect.X = (int)_position.X - (int)(_fontOrigin.X * _actualScale);
        _actualDestRect.Y = (int)_position.Y - (int)(_fontOrigin.Y * _actualScale);
        _actualDestRect.Width = (int)(size.X * _actualScale);
        _actualDestRect.Height = (int)(size.Y * _actualScale);

        // apply min size
        if (MinSize != null)
        {
            var minInPixels = CalcActualSizeInPixels(MinSize.Value);
            _actualDestRect.Width = Math.Max(minInPixels.X, _actualDestRect.Width);
            _actualDestRect.Height = Math.Max(minInPixels.Y, _actualDestRect.Height);
        }

        // apply max size
        if (MaxSize != null)
        {
            var maxInPixels = CalcActualSizeInPixels(MaxSize.Value);
            _actualDestRect.Width = Math.Min(maxInPixels.X, _actualDestRect.Width);
            _actualDestRect.Height = Math.Min(maxInPixels.Y, _actualDestRect.Height);
        }
    }

    /// <summary>
    ///     Draw entity outline. Note: in paragraph its a special case and we implement it inside the DrawEntity function.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    protected override void DrawEntityOutline(SpriteBatch spriteBatch)
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
        if (_processedText == null)
            UpdateDestinationRects();

        // draw background color
        if (BackgroundColor.A > 0)
        {
            // get background color
            var backColor = UserInterface.Active.DrawUtils.FixColorOpacity(BackgroundColor);

            // get destination rect to draw it
            var rect = BackgroundColorUseBoxSize ? _destRect : _actualDestRect;

            // fix height for box background and scaling
            if (BackgroundColorUseBoxSize)
                rect.Height = (int)(rect.Height / GlobalScale);

            // add padding
            var padding = new Point(
                (int)(BackgroundColorPadding.X * GlobalScale),
                (int)(BackgroundColorPadding.Y * GlobalScale));
            rect.Location -= padding;
            rect.Size += padding + padding;

            // draw background color
            spriteBatch.Draw(Resources.WhiteTexture, rect, backColor);
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

        // get fill color
        var fillCol = UserInterface.Active.DrawUtils.FixColorOpacity(FillColor);

        // draw text itself
        spriteBatch.DrawString(_currFont, _processedText, _position, fillCol,
            0, _fontOrigin, _actualScale, SpriteEffects.None, 0.5f);

        // call base draw function
        base.DrawEntity(spriteBatch, phase);
    }
}