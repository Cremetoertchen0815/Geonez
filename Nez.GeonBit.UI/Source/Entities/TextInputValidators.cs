﻿#region File Description

//-----------------------------------------------------------------------------
// Validators you can attach to TextInput entities to manipulate and validate
// user input. These are used to create things like text input for numbers only,
// limit characters to english chars, etc.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Nez.GeonBit.UI.Entities.TextValidators;

/// <summary>
///     GeonBit.UI.Entities.TextValidators contains different text validators and processors you can attach to TextInput
///     entities.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     A class that validates text input to make sure its valid.
///     These classes can be added to any TextInput to limit the type of input the user can enter.
///     Note: this cannot be an interface due to serialization.
/// </summary>
public class ITextValidator
{
	/// <summary>
	///     Get the new text input value and return true if valid.
	///     This function can either return false to scrap input changes, or change the text and return true.
	/// </summary>
	/// <param name="text">New text input value.</param>
	/// <param name="oldText">Previous text input value.</param>
	/// <returns>If TextInput value is legal.</returns>
	public virtual bool ValidateText(ref string text, string oldText)
    {
        return true;
    }
}

/// <summary>
///     Make sure input is numeric and optionally validate min / max values.
/// </summary>
[Serializable]
public class TextValidatorNumbersOnly : ITextValidator
{
	/// <summary>
	///     Do we allow decimal point?
	/// </summary>
	public bool AllowDecimalPoint;

	/// <summary>
	///     Optional max value.
	/// </summary>
	public double? Max;

	/// <summary>
	///     Optional min value.
	/// </summary>
	public double? Min;

	/// <summary>
	///     Static ctor.
	/// </summary>
	static TextValidatorNumbersOnly()
    {
        Entity.MakeSerializable(typeof(TextValidatorNumbersOnly));
    }

	/// <summary>
	///     Create the number validator.
	/// </summary>
	/// <param name="allowDecimal">If true, will allow decimal point in input.</param>
	/// <param name="min">If provided, will force min value.</param>
	/// <param name="max">If provided, will force max value.</param>
	public TextValidatorNumbersOnly(bool allowDecimal, double? min = null, double? max = null)
    {
        AllowDecimalPoint = allowDecimal;
        Min = min;
        Max = max;
    }

	/// <summary>
	///     Create number validator with default params.
	/// </summary>
	public TextValidatorNumbersOnly() : this(false)
    {
    }

	/// <summary>
	///     Return true if text input is a valid number.
	/// </summary>
	/// <param name="text">New text input value.</param>
	/// <param name="oldText">Previous text input value.</param>
	/// <returns>If TextInput value is legal.</returns>
	public override bool ValidateText(ref string text, string oldText)
    {
        // if string empty return true
        if (text.Length == 0) return true;

        // will contain value as number
        double num;

        // try to parse as double
        if (AllowDecimalPoint)
        {
            if (!double.TryParse(text, out num)) return false;
        }
        // try to parse as int
        else
        {
            if (!int.TryParse(text, out var temp)) return false;
            num = temp;
        }

        // validate range
        if (Min != null && num < (double)Min) text = Min.ToString();
        if (Max != null && num > (double)Max) text = Max.ToString();

        // valid number input
        return true;
    }
}

/// <summary>
///     Make sure input contains only english characters.
/// </summary>
[Serializable]
public class TextValidatorEnglishCharsOnly : ITextValidator
{
    // regex for english only with spaces
    private static readonly Regex _slugNoSpaces = new(@"^[a-zA-Z|]+$");

    // regex for english only without spaces
    private static readonly Regex _slugWithSpaces = new(@"^[a-zA-Z|\ ]+$");

    // do we allow spaces in text?
    private bool _allowSpaces;

    // the regex to use
    private Regex _regex;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static TextValidatorEnglishCharsOnly()
    {
        Entity.MakeSerializable(typeof(TextValidatorEnglishCharsOnly));
    }

    /// <summary>
    ///     Create the validator.
    /// </summary>
    /// <param name="allowSpaces">If true, will allow spaces.</param>
    public TextValidatorEnglishCharsOnly(bool allowSpaces)
    {
        AllowSpaces = allowSpaces;
    }

    /// <summary>
    ///     Create the validator with default params.
    /// </summary>
    public TextValidatorEnglishCharsOnly() : this(false)
    {
    }

    /// <summary>
    ///     Set / get if we allow spaces in text.
    /// </summary>
    public bool AllowSpaces
    {
        get => _allowSpaces;
        set
        {
            _allowSpaces = value;
            _regex = _allowSpaces ? _slugWithSpaces : _slugNoSpaces;
        }
    }

    /// <summary>
    ///     Return true if text input is only english characters.
    /// </summary>
    /// <param name="text">New text input value.</param>
    /// <param name="oldText">Previous text input value.</param>
    /// <returns>If TextInput value is legal.</returns>
    public override bool ValidateText(ref string text, string oldText)
    {
        return (text.Length == 0 || _regex.IsMatch(text));
    }
}

/// <summary>
///     Make sure input contains only letters, numbers, underscores or hyphens (and optionally spaces).
/// </summary>
[Serializable]
public class SlugValidator : ITextValidator
{
    // regex for slug with spaces
    private static readonly Regex _slugNoSpaces = new(@"^[a-zA-Z\-_0-9]+$");

    // regex for slug without spaces
    private static readonly Regex _slugWithSpaces = new(@"^[a-zA-Z\-_\ 0-9]+$");

    // do we allow spaces in text?
    private bool _allowSpaces;

    // the regex to use
    private Regex _regex;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static SlugValidator()
    {
        Entity.MakeSerializable(typeof(SlugValidator));
    }

    /// <summary>
    ///     Create the slug validator.
    /// </summary>
    /// <param name="allowSpaces">If true, will allow spaces.</param>
    public SlugValidator(bool allowSpaces)
    {
        AllowSpaces = AllowSpaces;
    }

    /// <summary>
    ///     Create the validator with default params.
    /// </summary>
    public SlugValidator() : this(false)
    {
    }

    /// <summary>
    ///     Set / get if we allow spaces in text.
    /// </summary>
    public bool AllowSpaces
    {
        get => _allowSpaces;
        set
        {
            _allowSpaces = value;
            _regex = _allowSpaces ? _slugWithSpaces : _slugNoSpaces;
        }
    }

    /// <summary>
    ///     Return true if text input is slug.
    /// </summary>
    /// <param name="text">New text input value.</param>
    /// <param name="oldText">Previous text input value.</param>
    /// <returns>If TextInput value is legal.</returns>
    public override bool ValidateText(ref string text, string oldText)
    {
        return (text.Length == 0 || _regex.IsMatch(text));
    }
}

/// <summary>
///     Make sure input don't contain double spaces or tabs.
/// </summary>
[Serializable]
public class OnlySingleSpaces : ITextValidator
{
	/// <summary>
	///     Static ctor.
	/// </summary>
	static OnlySingleSpaces()
    {
        Entity.MakeSerializable(typeof(OnlySingleSpaces));
    }

	/// <summary>
	///     Return true if text input don't contain double spaces or tabs.
	/// </summary>
	/// <param name="text">New text input value.</param>
	/// <param name="oldText">Previous text input value.</param>
	/// <returns>If TextInput value is legal.</returns>
	public override bool ValidateText(ref string text, string oldText)
    {
        return !text.Contains("  ") && !text.Contains("\t");
    }
}

/// <summary>
///     Make sure input is always title, eg starts with a capital letter followed by lowercase.
/// </summary>
[Serializable]
public class TextValidatorMakeTitle : ITextValidator
{
	/// <summary>
	///     Static ctor.
	/// </summary>
	static TextValidatorMakeTitle()
    {
        Entity.MakeSerializable(typeof(TextValidatorMakeTitle));
    }

	/// <summary>
	///     Always return true, and make first character uppercase while all following
	///     chars lowercase.
	/// </summary>
	/// <param name="text">New text input value.</param>
	/// <param name="oldText">Previous text input value.</param>
	/// <returns>Always return true.</returns>
	public override bool ValidateText(ref string text, string oldText)
    {
        if (text.Length > 0)
        {
            text = text.ToLower();
            text = text[0].ToString().ToUpper() + text.Remove(0, 1);
        }

        return true;
    }
}