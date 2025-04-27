﻿#region File Description

//-----------------------------------------------------------------------------
// Label is just like Paragraph, but with different default color, size and align.
// Its used as small text above entities, for example if you want to add a slider 
// with a short sentence that explains what it does.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     Label entity is a subclass of Paragraph. Basically its the same, but with a different
///     default styling, and serves as a sugarcoat to quickly create labels for widgets.
/// </summary>
[Serializable]
public class Label : Paragraph
{
    /// <summary>Default styling for labels. Note: loaded from UI theme xml file.</summary>
    public new static StyleSheet DefaultStyle = new();

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static Label()
    {
        MakeSerializable(typeof(Label));
    }

    /// <summary>
    ///     Create the label.
    /// </summary>
    /// <param name="text">Label text.</param>
    /// <param name="anchor">Position anchor.</param>
    /// <param name="size">Optional label size.</param>
    /// <param name="offset">Offset from anchor position.</param>
    public Label(string text, Anchor anchor = Anchor.Auto, Vector2? size = null, Vector2? offset = null) :
        base(text, anchor, size, offset)
    {
        UpdateStyle(DefaultStyle);
    }

    /// <summary>
    ///     Create label with default params and empty text.
    /// </summary>
    public Label() : this(string.Empty)
    {
    }
}