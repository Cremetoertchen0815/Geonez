﻿#region File Description

//-----------------------------------------------------------------------------
// TextInput are entities that allow the user to type in free text using the keyboard.
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
using Nez.GeonBit.UI.Entities.TextValidators;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     A textbox that allow users to put in free text.
/// </summary>
[Serializable]
public class TextInput : PanelBase
{
    /// <summary>Default styling for the text input itself. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>Default style for paragraph that show current value.</summary>
    public static StyleSheet DefaultParagraphStyle = new();

    /// <summary>Default style for the placeholder paragraph.</summary>
    public static StyleSheet DefaultPlaceholderStyle = new();

    /// <summary>How fast to blink caret when text input is selected.</summary>
    public static float CaretBlinkingSpeed = 2f;

    /// <summary>Default text-input size for when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = new(0f, 65f);

    /// <summary>
    ///     The actual displayed text, after wordwrap and other processing.
    ///     note: only the text currently visible by scrollbar.
    /// </summary>
    private string _actualDisplayText = string.Empty;

    // current caret position (-1 is last character).
    private int _caret = -1;

    // current caret animation step
    private float _caretAnim;

    /// <summary>If false, it will only allow one line input.</summary>
    protected bool _multiLine;

    /// <summary>
    ///     Text to show when there's no input. Note that this text will be drawn with PlaceholderParagraph, and not
    ///     TextParagraph.
    /// </summary>
    private string _placeholderText = string.Empty;

    // scrollbar to use if text height exceed the input box size
    private VerticalScrollbar _scrollbar;

    // current text value
    private string _value = string.Empty;

    /// <summary>
    ///     Set to any number to limit input by characters count.
    ///     Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.
    /// </summary>
    public int CharactersLimit = 0;

    /// <summary>
    ///     If provided, hide input and replace it with the given character.
    ///     This is useful for stuff like password input field.
    /// </summary>
    public char? HideInputWithChar;

    /// <summary>
    ///     If true, will limit max input length to fit textbox size.
    ///     Note: this will only take effect when user insert input, if you set value programmatically it will be ignored.
    /// </summary>
    public bool LimitBySize;

    public int MaxCharsPerLine = -1;

    /// <summary>A placeholder paragraph to show when text input is empty.</summary>
    public Paragraph PlaceholderParagraph;

    /// <summary>The Paragraph object showing current text value.</summary>
    public Paragraph TextParagraph;

    /// <summary>List of validators to apply on text input.</summary>
    public List<ITextValidator> Validators = new();

    /// <summary>
    ///     If provided, will automatically put this value whenever the user leave the input box and its empty.
    /// </summary>
    public string ValueWhenEmpty = null;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static TextInput()
    {
        MakeSerializable(typeof(TextInput));
    }

    /// <summary>
    ///     Create the text input.
    /// </summary>
    /// <param name="multiline">If true, text input will accept multiple lines.</param>
    /// <param name="size">Input box size.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    /// <param name="skin">TextInput skin, eg which texture to use.</param>
    public TextInput(bool multiline, Vector2 size, Anchor anchor = Anchor.Auto, Vector2? offset = null,
        PanelSkin skin = PanelSkin.ListBackground) :
        base(size, skin, anchor, offset)
    {
        // set multiline mode
        _multiLine = multiline;

        // special case - if multiline and asked for default height, make it heigher
        if (multiline && size.Y == -1) _size.Y *= 4;

        // update default style
        UpdateStyle(DefaultStyle);

        // set limit by size - default true in single-line, default false in multi-line
        LimitBySize = !_multiLine;

        if (!UserInterface.Active._isDeserializing)
        {
            // create paragraph to show current value
            TextParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
            TextParagraph.UpdateStyle(DefaultParagraphStyle);
            TextParagraph._hiddenInternalEntity = true;
            TextParagraph.Identifier = "_TextParagraph";
            TextParagraph.TextModifier = x =>
                MaxCharsPerLine < 0 || x.Length < MaxCharsPerLine
                    ? x
                    : "..." + x.Substring(x.Length - MaxCharsPerLine + 3, MaxCharsPerLine - 3);
            (TextParagraph as MulticolorParagraph).EnableColorInstructions = false;
            AddChild(TextParagraph, true);

            // create the placeholder paragraph
            PlaceholderParagraph = UserInterface.DefaultParagraph(string.Empty, Anchor.TopLeft);
            PlaceholderParagraph.UpdateStyle(DefaultPlaceholderStyle);
            PlaceholderParagraph._hiddenInternalEntity = true;
            PlaceholderParagraph.Identifier = "_PlaceholderParagraph";
            AddChild(PlaceholderParagraph, true);

            // update multiline related stuff
            UpdateMultilineState();

            // if the default paragraph type is multicolor, disable it for input
            var colorTextParagraph = TextParagraph as MulticolorParagraph;
            if (colorTextParagraph != null) colorTextParagraph.EnableColorInstructions = false;
        }
    }

    /// <summary>
    ///     Create the text input with default size.
    /// </summary>
    /// <param name="multiline">If true, text input will accept multiple lines.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public TextInput(bool multiline, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
        this(multiline, USE_DEFAULT_SIZE, anchor, offset)
    {
    }

    /// <summary>
    ///     Create default single-line text input.
    /// </summary>
    public TextInput() : this(false)
    {
    }

    /// <summary>
    ///     Set / get multiline mode.
    /// </summary>
    public bool Multiline
    {
        get => _multiLine;
        set
        {
            if (_multiLine != value)
            {
                _multiLine = value;
                UpdateMultilineState();
            }
        }
    }

    /// <summary>
    ///     Text to show when there's no input using the placeholder style.
    /// </summary>
    public string PlaceholderText
    {
        get => _placeholderText;
        set => _placeholderText = _multiLine ? value : value.Replace("\n", string.Empty);
    }

    /// <summary>
    ///     Current input text value.
    /// </summary>
    public string Value
    {
        get => _value;
        set
        {
            _value = _multiLine ? value : value.Replace("\n", string.Empty);
            FixCaretPosition();
        }
    }

    /// <summary>
    ///     Current cursor, eg where we are about to put next character.
    ///     Set to -1 to jump to end.
    /// </summary>
    public int Caret
    {
        get => _caret;
        set => _caret = value;
    }

    /// <summary>
    ///     Current scrollbar position.
    /// </summary>
    [XmlIgnore]
    public int ScrollPosition
    {
        get => _scrollbar != null ? _scrollbar.Value : 0;
        set
        {
            if (_scrollbar != null) _scrollbar.Value = value;
        }
    }

    /// <summary>
    ///     Update after multiline state was changed.
    /// </summary>
    private void UpdateMultilineState()
    {
        // we are now multiline
        if (_multiLine)
        {
            _scrollbar = new VerticalScrollbar(0, 0, Anchor.CenterRight, new Vector2(-8, 0))
            {
                Value = 0,
                Visible = false,
                _hiddenInternalEntity = true,
                Identifier = "__inputScrollbar"
            };
            AddChild(_scrollbar);
        }
        // we are not multiline
        else
        {
            if (_scrollbar != null)
            {
                _scrollbar.RemoveFromParent();
                _scrollbar = null;
            }
        }

        // set default wrap words state
        TextParagraph.WrapWords = _multiLine;
        PlaceholderParagraph.WrapWords = _multiLine;
        TextParagraph.Anchor = PlaceholderParagraph.Anchor = _multiLine ? Anchor.TopLeft : Anchor.CenterLeft;
    }

    /// <summary>
    ///     Special init after deserializing entity from file.
    /// </summary>
    protected internal override void InitAfterDeserialize()
    {
        base.InitAfterDeserialize();

        // set main text paragraph
        TextParagraph = Find("_TextParagraph") as Paragraph;
        TextParagraph._hiddenInternalEntity = true;

        // set scrollbar
        _scrollbar = Find<VerticalScrollbar>("__inputScrollbar");
        if (_scrollbar != null)
            _scrollbar._hiddenInternalEntity = true;

        // set placeholder paragraph
        PlaceholderParagraph = Find("_PlaceholderParagraph") as Paragraph;
        PlaceholderParagraph._hiddenInternalEntity = true;

        // recalc dest rects
        UpdateMultilineState();
    }

    /// <summary>
    ///     Is the text input a natrually-interactable entity.
    /// </summary>
    /// <returns>True.</returns>
    public override bool IsNaturallyInteractable()
    {
        return true;
    }

    /// <summary>
    ///     Move scrollbar to show caret position.
    /// </summary>
    public void ScrollToCaret()
    {
        // skip if no scrollbar
        if (_scrollbar == null) return;

        // make sure caret position is legal
        if (_caret >= _value.Length) _caret = -1;

        // if caret is at end of text jump to it
        if (_caret == -1)
        {
            _scrollbar.Value = (int)_scrollbar.Max;
        }
        // if not try to find the right pos
        else
        {
            TextParagraph.Text = _value;
            TextParagraph.CalcTextActualRectWithWrap();
            var processedValueText = TextParagraph.GetProcessedText();
            var currLine = processedValueText.Substring(0, _caret).Split('\n').Length;
            _scrollbar.Value = currLine - 1;
        }
    }

    /// <summary>
    ///     Move caret to the end of text.
    /// </summary>
    /// <param name="scrollToCaret">If true, will also scroll to show caret position.</param>
    public void ResetCaret(bool scrollToCaret)
    {
        Caret = -1;
        if (scrollToCaret) ScrollToCaret();
    }

    /// <summary>
    ///     Prepare the input paragraph for display.
    /// </summary>
    /// <param name="usePlaceholder">If true, will use the placeholder text. Else, will use the real input text.</param>
    /// <param name="showCaret">If true, will also add the caret text when needed. If false, will not show caret.</param>
    /// <returns>Processed text that will actually be displayed on screen.</returns>
    protected string PrepareInputTextForDisplay(bool usePlaceholder, bool showCaret)
    {
        // set caret char
        var caretShow = showCaret ? (int)_caretAnim % 2 == 0 ? "|" : " " : string.Empty;

        // set main text when hidden with password char
        if (HideInputWithChar != null)
        {
            var hiddenVal = new string(HideInputWithChar.Value, _value.Length);
            TextParagraph.Text = hiddenVal.Insert(_caret >= 0 ? _caret : hiddenVal.Length, caretShow);
        }
        // set main text for regular text input
        else
        {
            TextParagraph.Text = _value.Insert(_caret >= 0 ? _caret : _value.Length, caretShow);
        }

        // update placeholder text
        PlaceholderParagraph.Text = _placeholderText;

        // get current paragraph and prepare to draw
        var currParagraph = usePlaceholder ? PlaceholderParagraph : TextParagraph;
        TextParagraph.UpdateDestinationRectsIfDirty();

        // get text to display
        return currParagraph.GetProcessedText();
    }

    /// <summary>
    ///     Handle mouse click event.
    ///     TextInput override this function to handle picking caret position.
    /// </summary>
    protected override void DoOnClick()
    {
        // first call base DoOnClick
        base.DoOnClick();

        // check if hit paragraph
        if (_value.Length > 0)
        {
            // get relative position
            var actualParagraphPos = new Vector2(_destRectInternal.Location.X, _destRectInternal.Location.Y);
            var relativeOffset = GetMousePos(-actualParagraphPos);

            // calc caret position
            var charSize = TextParagraph.GetCharacterActualSize();
            var x = (int)(relativeOffset.X / charSize.X);
            _caret = x + (MaxCharsPerLine < 0 || _value.Length < MaxCharsPerLine
                ? 0
                : _value.Length - MaxCharsPerLine + 3);

            // if multiline, take line into the formula
            if (_multiLine)
            {
                // get the whole processed text
                TextParagraph.Text = _value;
                TextParagraph.CalcTextActualRectWithWrap();
                var processedValueText = TextParagraph.GetProcessedText();

                // calc y position and add scrollbar value to it
                var y = (int)(relativeOffset.Y / charSize.Y) + _scrollbar.Value;

                // break actual text into lines
                var lines = new List<string>(processedValueText.Split('\n'));
                for (var i = 0; i < y && i < lines.Count; ++i) _caret += lines[i].Length + 1;
            }

            // if override text length reset caret
            if (_caret >= _value.Length) _caret = -1;
        }
        // if don't click on the paragraph itself, reset caret position.
        else
        {
            _caret = -1;
        }
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
        // call base draw function to draw the panel part
        base.DrawEntity(spriteBatch, phase);

        // get which paragraph we currently show - real or placeholder
        var showPlaceholder = !(IsFocused || _value.Length > 0);
        var currParagraph = showPlaceholder ? PlaceholderParagraph : TextParagraph;

        // get actual processed string
        _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder, IsFocused);

        // for multiline only - handle scrollbar visibility and max
        if (_multiLine && _actualDisplayText != null)
        {
            // get how many lines can fit in the textbox and how many lines display text actually have
            var linesFit = _destRectInternal.Height / (int)Math.Max(currParagraph.GetCharacterActualSize().Y, 1);
            var linesInText = _actualDisplayText.Split('\n').Length;

            // if there are more lines than can fit, show scrollbar and manage scrolling:
            if (linesInText > linesFit)
            {
                // fix paragraph width to leave room for the scrollbar
                var prevWidth = currParagraph.Size.X;
                currParagraph.Size = new Vector2(_destRectInternal.Width / GlobalScale - 20, 0);
                if (currParagraph.Size.X != prevWidth)
                {
                    // update size and re-calculate lines in text
                    _actualDisplayText = PrepareInputTextForDisplay(showPlaceholder, IsFocused);
                    linesInText = _actualDisplayText.Split('\n').Length;
                }

                // set scrollbar max and steps
                _scrollbar.Max = (uint)Math.Max(linesInText - linesFit, 2);
                _scrollbar.StepsCount = _scrollbar.Max;
                _scrollbar.Visible = true;

                // update text to fit scrollbar. first, rebuild the text with just the visible segment
                var lines = new List<string>(_actualDisplayText.Split('\n'));
                var from = Math.Min(_scrollbar.Value, lines.Count - 1);
                var size = Math.Min(linesFit, lines.Count - from);
                lines = lines.GetRange(from, size);
                _actualDisplayText = string.Join("\n", lines);
                currParagraph.Text = _actualDisplayText;
            }
            // if no need for scrollbar make it invisible
            else
            {
                currParagraph.Size = Vector2.Zero;
                _scrollbar.Visible = false;
            }
        }

        // set placeholder and main text visibility based on current value
        TextParagraph.Visible = !showPlaceholder;
        PlaceholderParagraph.Visible = showPlaceholder;
    }

    /// <summary>
    ///     Validate current text input after change (usually addition of text).
    /// </summary>
    /// <param name="newVal">New text value, to check validity.</param>
    /// <param name="oldVal">Previous text value, before the change.</param>
    /// <returns>True if new input is valid, false otherwise.</returns>
    private bool ValidateInput(ref string newVal, string oldVal)
    {
        // if new characters were added, and we now exceed characters limit, revet to previous value.
        if (CharactersLimit != 0 &&
            newVal.Length > CharactersLimit)
        {
            newVal = oldVal;
            return false;
        }

        // if not multiline and got line break, revet to previous value
        if (!_multiLine && newVal.Contains("\n"))
        {
            newVal = oldVal;
            return false;
        }

        // get main paragraph actual size
        TextParagraph.Text = newVal;
        TextParagraph.CalcTextActualRectWithWrap();
        var textSize = TextParagraph.GetActualDestRect();

        // if set to limit by size make sure we don't exceed it
        if (LimitBySize)
        {
            // prepare display
            PrepareInputTextForDisplay(false, false);

            // if multiline, compare heights
            if (_multiLine && textSize.Height >= _destRectInternal.Height)
            {
                newVal = oldVal;
                return false;
            }
            // if single line, compare widths

            if (textSize.Width >= _destRectInternal.Width)
            {
                newVal = oldVal;
                return false;
            }
        }

        if (MaxCharsPerLine == -1 && textSize.Width >= _destRectInternal.Width) MaxCharsPerLine = oldVal.Length;

        // if got here we iterate over additional validators
        foreach (var validator in Validators)
            if (!validator.ValidateText(ref newVal, oldVal))
            {
                newVal = oldVal;
                return false;
            }

        // looks good!
        return true;
    }

    /// <summary>
    ///     Make sure caret position is currently valid and in range.
    /// </summary>
    private void FixCaretPosition()
    {
        if (_caret < -1) _caret = 0;
        if (_caret >= _value.Length || _value.Length == 0) _caret = -1;
    }

    /// <summary>
    ///     Called every frame before update.
    ///     TextInput implement this function to get keyboard input and also to animate caret timer.
    /// </summary>
    protected override void DoBeforeUpdate()
    {
        // animate caret
        _caretAnim += (float)Input.CurrGameTime.ElapsedGameTime.TotalSeconds * CaretBlinkingSpeed;

        // if focused, and got character input in this frame..
        if (IsFocused)
        {
            // validate caret position
            FixCaretPosition();

            // get caret position
            var pos = _caret;

            // store old string and update based on user input
            var oldVal = _value;
            _value = Input.GetTextInput(_value, ref pos);

            // update caret position
            _caret = pos;

            // if value changed:
            if (_value != oldVal)
            {
                // if new characters were added and input is now illegal, revert to previous value
                if (!ValidateInput(ref _value, oldVal)) _value = oldVal;

                // call change event
                if (_value != oldVal) DoOnValueChange();

                // after change, scroll to caret
                ScrollToCaret();

                // fix caret position
                FixCaretPosition();
            }
        }

        // call base do-before-update
        base.DoBeforeUpdate();
    }

    /// <summary>
    ///     Called every time this entity is focused / unfocused.
    /// </summary>
    protected override void DoOnFocusChange()
    {
        // call base on focus change
        base.DoOnFocusChange();

        // check if need to set default value
        if (ValueWhenEmpty != null && Value.Length == 0) Value = ValueWhenEmpty;
    }
}