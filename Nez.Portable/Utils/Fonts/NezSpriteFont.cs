﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;
#if !FNA
public class NezSpriteFont : IFont
{
    private readonly SpriteFont _font;
    private readonly Dictionary<char, SpriteFont.Glyph> _glyphs;

    /// <summary>
    ///     this sucker gets used a lot so we cache it to avoid having to create it every frame
    /// </summary>
    private Matrix2D _transformationMatrix = Matrix2D.Identity;


    public NezSpriteFont(SpriteFont font)
    {
        _font = font;
        _glyphs = font.GetGlyphs();
    }

    public float LineSpacing => _font.LineSpacing;


    /// <summary>
    ///     Returns the size of a string when rendered in this font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>
    ///     The size, in pixels, of 'text' when rendered in
    ///     this font.
    /// </returns>
    public Vector2 MeasureString(string text)
    {
        var source = new FontCharacterSource(text);
        MeasureString(ref source, out var size);
        return size;
    }


    /// <summary>
    ///     Returns the size of the contents of a StringBuilder when
    ///     rendered in this font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>
    ///     The size, in pixels, of 'text' when rendered in
    ///     this font.
    /// </returns>
    public Vector2 MeasureString(StringBuilder text)
    {
        var source = new FontCharacterSource(text);
        MeasureString(ref source, out var size);
        return size;
    }


    /// <summary>
    ///     checks to see if a BitmapFontRegion exists for the char
    /// </summary>
    /// <returns><c>true</c>, if region exists for char was fonted, <c>false</c> otherwise.</returns>
    /// <param name="c">C.</param>
    public bool HasCharacter(char c)
    {
        return TryGetFontRegionForChar(c, out var fontGlyph);
    }

    private void MeasureString(ref FontCharacterSource text, out Vector2 size)
    {
        if (text.Length == 0)
        {
            size = Vector2.Zero;
            return;
        }

        // Get the default glyph here once.
        SpriteFont.Glyph? defaultGlyph = null;
        if (_font.DefaultCharacter.HasValue)
            defaultGlyph = _glyphs[_font.DefaultCharacter.Value];

        var width = 0.0f;
        float finalLineHeight = _font.LineSpacing;

        var currentGlyph = SpriteFont.Glyph.Empty;
        var offset = Vector2.Zero;
        var firstGlyphOfLine = true;

        for (var i = 0; i < text.Length; ++i)
        {
            var c = text[i];

            if (c == '\r' || c == '\0')
                continue;

            if (c == '\n')
            {
                finalLineHeight = _font.LineSpacing;

                offset.X = 0;
                offset.Y += _font.LineSpacing;
                firstGlyphOfLine = true;
                continue;
            }

            if (!_glyphs.TryGetValue(c, out currentGlyph))
            {
                if (!defaultGlyph.HasValue)
                    throw new ArgumentException("Errors.TextContainsUnresolvableCharacters", "text");

                currentGlyph = defaultGlyph.Value;
            }

            // The first character on a line might have a negative left side bearing.
            // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
            //  so that text does not hang off the left side of its rectangle.
            if (firstGlyphOfLine)
            {
                offset.X = Math.Max(currentGlyph.LeftSideBearing, 0);
                firstGlyphOfLine = false;
            }
            else
            {
                offset.X += _font.Spacing + currentGlyph.LeftSideBearing;
            }

            offset.X += currentGlyph.Width;

            var proposedWidth = offset.X + Math.Max(currentGlyph.RightSideBearing, 0);
            if (proposedWidth > width)
                width = proposedWidth;

            offset.X += currentGlyph.RightSideBearing;

            if (currentGlyph.Cropping.Height > finalLineHeight)
                finalLineHeight = currentGlyph.Cropping.Height;
        }

        size.X = width;
        size.Y = offset.Y + finalLineHeight;
    }


    /// <summary>
    ///     gets the BitmapFontRegion for the given char optionally substituting the default region if it isnt present.
    /// </summary>
    /// <returns><c>true</c>, if get font region for char was tryed, <c>false</c> otherwise.</returns>
    /// <param name="c">C.</param>
    /// <param name="fontRegion">Font region.</param>
    /// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
    public bool TryGetFontRegionForChar(char c, out SpriteFont.Glyph fontGlyph,
        bool useDefaultRegionIfNotPresent = false)
    {
        if (!_glyphs.TryGetValue(c, out fontGlyph))
        {
            if (useDefaultRegionIfNotPresent)
            {
                fontGlyph = _glyphs[_font.DefaultCharacter.Value];
                return true;
            }

            return false;
        }

        return true;
    }


    #region drawing

    void IFont.DrawInto(Batcher batcher, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
    {
        var source = new FontCharacterSource(text);
        DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
    }


    void IFont.DrawInto(Batcher batcher, StringBuilder text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
    {
        var source = new FontCharacterSource(text);
        DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
    }


    public void DrawInto(Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
    {
        var flipAdjustment = Vector2.Zero;

        var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
        var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

        if (flippedVert || flippedHorz)
        {
            MeasureString(ref text, out var size);

            if (flippedHorz)
            {
                origin.X *= -1;
                flipAdjustment.X = -size.X;
            }

            if (flippedVert)
            {
                origin.Y *= -1;
                flipAdjustment.Y = _font.LineSpacing - size.Y;
            }
        }

        // TODO: This looks excessive... i suspect we could do most of this with simple vector math and avoid this much matrix work.
        var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != Vector2.One;
        if (requiresTransformation)
        {
            Matrix2D.CreateTranslation(-origin.X, -origin.Y, out _transformationMatrix);
            Matrix2D.CreateScale(flippedHorz ? -scale.X : scale.X, flippedVert ? -scale.Y : scale.Y, out var temp);
            Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
            Matrix2D.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
            Matrix2D.Multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
            Matrix2D.CreateRotation(rotation, out temp);
            Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
            Matrix2D.CreateTranslation(position.X, position.Y, out temp);
            Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
        }

        // Get the default glyph here once.
        SpriteFont.Glyph? defaultGlyph = null;
        if (_font.DefaultCharacter.HasValue)
            defaultGlyph = _glyphs[_font.DefaultCharacter.Value];

        var currentGlyph = SpriteFont.Glyph.Empty;
        var offset = requiresTransformation ? Vector2.Zero : position - origin;
        var firstGlyphOfLine = true;

        for (var i = 0; i < text.Length; ++i)
        {
            var c = text[i];

            if (c == '\r' || c == '\0')
                continue;

            if (c == '\n')
            {
                offset.X = requiresTransformation ? 0f : position.X - origin.X;
                offset.Y += _font.LineSpacing;
                firstGlyphOfLine = true;
                continue;
            }

            if (!_glyphs.TryGetValue(c, out currentGlyph))
            {
                if (!defaultGlyph.HasValue)
                    throw new ArgumentException("Errors.TextContainsUnresolvableCharacters", "text");

                currentGlyph = defaultGlyph.Value;
            }

            // The first character on a line might have a negative left side bearing.
            // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
            // so that text does not hang off the left side of its rectangle.
            if (firstGlyphOfLine)
            {
                offset.X += Math.Max(currentGlyph.LeftSideBearing, 0);
                firstGlyphOfLine = false;
            }
            else
            {
                offset.X += _font.Spacing + currentGlyph.LeftSideBearing;
            }

            var p = offset;

            if (flippedHorz)
                p.X += currentGlyph.BoundsInTexture.Width;
            p.X += currentGlyph.Cropping.X;

            if (flippedVert)
                p.Y += currentGlyph.BoundsInTexture.Height - _font.LineSpacing;
            p.Y += currentGlyph.Cropping.Y;

            // transform our point if we need to
            if (requiresTransformation)
                Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

            var destRect = RectangleExt.FromFloats(p.X, p.Y,
                currentGlyph.BoundsInTexture.Width * scale.X,
                currentGlyph.BoundsInTexture.Height * scale.Y);

            batcher.Draw(_font.Texture, destRect, currentGlyph.BoundsInTexture, color, rotation, Vector2.Zero,
                effect, depth);

            offset.X += currentGlyph.Width + currentGlyph.RightSideBearing;
        }
    }

    #endregion
}

#else
	public class NezSpriteFont : IFont
	{
		public float LineSpacing { get { return _font.LineSpacing; } }

		SpriteFont _font;

		/// <summary>
		/// this sucker gets used a lot so we cache it to avoid having to create it every frame
		/// </summary>
#pragma warning disable 0414
		Matrix _transformationMatrix = Matrix.Identity;
#pragma warning restore 0414


		public NezSpriteFont( SpriteFont font )
		{
			_font = font;
		}


		public void DrawInto( Batcher batcher, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			var source = new FontCharacterSource( text );
			DrawInto( batcher, ref source, position, color, rotation, origin, scale, effect, depth );
		}


		public void DrawInto( Batcher batcher, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			var source = new FontCharacterSource( text );
			DrawInto( batcher, ref source, position, color, rotation, origin, scale, effect, depth );
		}


		public void DrawInto( Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			throw new NotImplementedException();
		}


		bool IFont.HasCharacter( char c )
		{
			throw new NotImplementedException();
		}


		public Vector2 MeasureString( StringBuilder text )
		{
			return _font.MeasureString( text );
		}


		public Vector2 MeasureString( string text )
		{
			return _font.MeasureString( text );
		}

	}
#endif