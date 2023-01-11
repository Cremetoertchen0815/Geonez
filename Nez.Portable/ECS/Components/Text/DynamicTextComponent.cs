﻿using Microsoft.Xna.Framework;
using Nez.Sprites;
using System;

namespace Nez
{
	/// <summary>
	/// Renders a dynamic text in form of a delegate.
	/// </summary>
	public class DynamicTextComponent : SpriteRenderer
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin, Entity.Transform.Scale,
						Entity.Transform.Rotation, _size.X, _size.Y);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
		public Func<string> Text
		{
			get => _text;
			set => SetText(value);
		}

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign HorizontalOrigin
		{
			get => _horizontalAlign;
			set => SetHorizontalAlign(value);
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign VerticalOrigin
		{
			get => _verticalAlign;
			set => SetVerticalAlign(value);
		}


		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		protected IFont _font;
		protected Func<string> _text;
		private Vector2 _size;


		public DynamicTextComponent() : this(Graphics.Instance.BitmapFont, () => "", Vector2.Zero, Color.White)
		{
		}

		public DynamicTextComponent(IFont font, Func<string> text, Vector2 localOffset, Color color)
		{
			_font = font;
			_text = text;
			_localOffset = localOffset;
			Color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			UpdateSize();
		}


		#region Fluent setters

		public DynamicTextComponent SetFont(IFont font)
		{
			_font = font;
			UpdateSize();

			return this;
		}

		public DynamicTextComponent SetText(Func<string> text)
		{
			_text = text;
			UpdateSize();
			UpdateCentering();

			return this;
		}

		public DynamicTextComponent SetHorizontalAlign(HorizontalAlign hAlign)
		{
			_horizontalAlign = hAlign;
			UpdateCentering();

			return this;
		}

		public DynamicTextComponent SetVerticalAlign(VerticalAlign vAlign)
		{
			_verticalAlign = vAlign;
			UpdateCentering();

			return this;
		}

		#endregion


		private void UpdateSize()
		{
			_size = _font.MeasureString(_text());
			UpdateCentering();
		}

		private void UpdateCentering()
		{
			var oldOrigin = _origin;

			if (_horizontalAlign == HorizontalAlign.Left)
				oldOrigin.X = 0;
			else if (_horizontalAlign == HorizontalAlign.Center)
				oldOrigin.X = _size.X / 2;
			else
				oldOrigin.X = _size.X;

			if (_verticalAlign == VerticalAlign.Top)
				oldOrigin.Y = 0;
			else if (_verticalAlign == VerticalAlign.Center)
				oldOrigin.Y = _size.Y / 2;
			else
				oldOrigin.Y = _size.Y;

			Origin = new Vector2((int)oldOrigin.X, (int)oldOrigin.Y);
		}

		public override void Render(Batcher batcher, Camera camera) => batcher.DrawString(_font, _text(), Entity.Transform.Position + _localOffset, Color,
				Entity.Transform.Rotation, Origin, Entity.Transform.Scale, SpriteEffects, LayerDepth);
	}
}