﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Sprites;

/// <summary>
///     Scrolling sprite. Note that ScrollingSprite overrides the Material so that it can wrap the UVs. This class requires
///     the texture
///     to not be part of an atlas so that wrapping can work.
/// </summary>
public class ScrollingSpriteRenderer : TiledSpriteRenderer, IUpdatable
{
    // accumulate scroll in a separate float so that we can round it without losing precision for small scroll speeds
    private float _scrollX, _scrollY;

    /// <summary>
    ///     x speed of automatic scrolling in pixels/s
    /// </summary>
    public float ScrollSpeedX = 15;

    /// <summary>
    ///     y speed of automatic scrolling in pixels/s
    /// </summary>
    public float ScrollSpeedY = 0;


    public ScrollingSpriteRenderer()
    {
    }

    public ScrollingSpriteRenderer(Sprite sprite) : base(sprite)
    {
    }

    public ScrollingSpriteRenderer(Texture2D texture) : this(new Sprite(texture))
    {
    }

    /// <summary>
    ///     scale of the texture
    /// </summary>
    /// <value>The texture scale.</value>
    public override Vector2 TextureScale
    {
        get => _textureScale;
        set
        {
            _textureScale = value;

            // recalulcate our inverseTextureScale and the source rect size
            _inverseTexScale = new Vector2(1f / _textureScale.X, 1f / _textureScale.Y);
        }
    }

    public virtual void Update()
    {
        _scrollX += ScrollSpeedX * Time.DeltaTime;
        _scrollY += ScrollSpeedY * Time.DeltaTime;
        _sourceRect.X = (int)_scrollX;
        _sourceRect.Y = (int)_scrollY;
    }
}