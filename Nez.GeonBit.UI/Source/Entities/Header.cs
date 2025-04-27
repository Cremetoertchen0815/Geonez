﻿#region File Description

//-----------------------------------------------------------------------------
// Header is just like Paragraph, but with different default color, size and align.
// Its used as the top title for panels and menus.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;

namespace Nez.GeonBit.UI.Entities;

/// <summary>
///     Header entity is a subclass of Paragraph. Basically its the same, but with a different
///     default styling, and serves as a sugarcoat to quickly create headers for menues.
/// </summary>
[Serializable]
public class Header : Paragraph
{
	/// <summary>
	///     Default styling for headers. Remember that header is a subclass of Paragraph and has its basic styline.
	///     Note: loaded from UI theme xml file.
	/// </summary>
	public new static StyleSheet DefaultStyle = new();

	/// <summary>
	///     Static ctor.
	/// </summary>
	static Header()
    {
        MakeSerializable(typeof(Header));
    }

	/// <summary>
	///     Create the header.
	/// </summary>
	/// <param name="text">Header text.</param>
	/// <param name="anchor">Position anchor.</param>
	/// <param name="offset">Offset from anchor position.</param>
	public Header(string text, Anchor anchor = Anchor.Auto, Vector2? offset = null) :
        base(text, anchor, offset: offset)
    {
        UpdateStyle(DefaultStyle);
    }

	/// <summary>
	///     Create default header without text.
	/// </summary>
	public Header() : this(string.Empty)
    {
    }
}