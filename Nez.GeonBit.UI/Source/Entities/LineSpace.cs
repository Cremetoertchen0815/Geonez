﻿#region File Description

//-----------------------------------------------------------------------------
// This entity just creates an empty space between entities which are auto
// aligned. In many ways, its very similar to the HTML <br> tag.
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
///     A line space is just a spacer for Auto-Anchored entities, eg a method to create artificial distance between rows.
/// </summary>
[Serializable]
public class LineSpace : Entity
{
	/// <summary>
	///     Single line space height.
	/// </summary>
	public static float SpaceSize = 8f;

    /// <summary>Default size this entity will have when no size is provided or when -1 is set for either width or height.</summary>
    public new static Vector2 DefaultSize = Vector2.Zero;

    /// <summary>
    ///     Static ctor.
    /// </summary>
    static LineSpace()
    {
        MakeSerializable(typeof(LineSpace));
    }

    /// <summary>
    ///     Create a new Line Space entity.
    /// </summary>
    /// <param name="spacesCount">How many spaces to create.</param>
    public LineSpace(int spacesCount) :
        base(Vector2.One, Anchor.Auto, Vector2.Zero)
    {
        // by default locked so it won't do events
        Locked = true;
        Enabled = false;
        ClickThrough = true;

        // set size based on space count
        _size.X = 0f;
        _size.Y = spacesCount != 0 ? SpaceSize * GlobalScale * Math.Max(spacesCount, 0) : -1;

        // default padding and spacing zero
        SpaceAfter = SpaceBefore = Padding = Vector2.Zero;
    }

    /// <summary>
    ///     Create default line space.
    /// </summary>
    public LineSpace() : this(1)
    {
    }

    /// <summary>
    ///     Draw the entity.
    /// </summary>
    /// <param name="spriteBatch">Sprite batch to draw on.</param>
    /// <param name="phase">The phase we are currently drawing.</param>
    protected override void DrawEntity(SpriteBatch spriteBatch, DrawPhase phase)
    {
    }
}