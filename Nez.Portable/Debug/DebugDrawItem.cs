﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;

namespace Nez;

internal class DebugDrawItem
{
    public BitmapFont BitmapFont;

    // shared by multiple items
    public Color Color;

    internal DebugDrawType drawType;
    public float Duration;
    public Vector2 End;
    public Vector2 Position;
    public Rectangle Rectangle;
    public float Scale;
    public int Size;
    public NezSpriteFont SpriteFont;

    // used for Line items
    public Vector2 Start;

    // used for Text items
    public string Text;

    // used for Pixel items
    public float X, Y;


    public DebugDrawItem(Vector2 start, Vector2 end, Color color, float duration)
    {
        Start = start;
        End = end;
        Color = color;
        Duration = duration;
        drawType = DebugDrawType.Line;
    }


    public DebugDrawItem(Rectangle rectangle, Color color, float duration)
    {
        Rectangle = rectangle;
        Color = color;
        Duration = duration;
        drawType = DebugDrawType.HollowRectangle;
    }


    public DebugDrawItem(float x, float y, int size, Color color, float duration)
    {
        X = x;
        Y = y;
        Size = size;
        Color = color;
        Duration = duration;
        drawType = DebugDrawType.Pixel;
    }


    public DebugDrawItem(BitmapFont bitmapFont, string text, Vector2 position, Color color, float duration,
        float scale)
    {
        BitmapFont = bitmapFont;
        Text = text;
        Position = position;
        Color = color;
        Scale = scale;
        Duration = duration;
        drawType = DebugDrawType.BitmapFontText;
    }


    public DebugDrawItem(NezSpriteFont spriteFont, string text, Vector2 position, Color color, float duration,
        float scale)
    {
        SpriteFont = spriteFont;
        Text = text;
        Position = position;
        Color = color;
        Scale = scale;
        Duration = duration;
        drawType = DebugDrawType.SpriteFontText;
    }


    public DebugDrawItem(string text, Color color, float duration, float scale)
    {
        BitmapFont = Graphics.Instance.BitmapFont;
        Text = text;
        Color = color;
        Scale = scale;
        Duration = duration;
        drawType = DebugDrawType.ConsoleText;
    }


    /// <summary>
    ///     returns true if we are done with this debug draw item
    /// </summary>
    public bool Draw(Batcher batcher)
    {
        switch (drawType)
        {
            case DebugDrawType.Line:
                batcher.DrawLine(Start, End, Color);
                break;
            case DebugDrawType.HollowRectangle:
                batcher.DrawHollowRect(Rectangle, Color);
                break;
            case DebugDrawType.Pixel:
                batcher.DrawPixel(X, Y, Color, Size);
                break;
            case DebugDrawType.BitmapFontText:
                batcher.DrawString(BitmapFont, Text, Position, Color, 0f, Vector2.Zero, Scale,
                    SpriteEffects.None, 0f);
                break;
            case DebugDrawType.SpriteFontText:
                batcher.DrawString(SpriteFont, Text, Position, Color, 0f, Vector2.Zero, new Vector2(Scale),
                    SpriteEffects.None, 0f);
                break;
            case DebugDrawType.ConsoleText:
                batcher.DrawString(BitmapFont, Text, Position, Color, 0f, Vector2.Zero, Scale,
                    SpriteEffects.None, 0f);
                break;
        }

        Duration -= Time.DeltaTime;

        return Duration < 0f;
    }


    public float GetHeight()
    {
        switch (drawType)
        {
            case DebugDrawType.Line:
                return (End - Start).Y;
            case DebugDrawType.HollowRectangle:
                return Rectangle.Height;
            case DebugDrawType.Pixel:
                return Size;
            case DebugDrawType.BitmapFontText:
            case DebugDrawType.ConsoleText:
                return BitmapFont.MeasureString(Text).Y * Scale;
            case DebugDrawType.SpriteFontText:
                return SpriteFont.MeasureString(Text).Y * Scale;
        }

        return 0;
    }

    internal enum DebugDrawType
    {
        Line,
        HollowRectangle,
        Pixel,
        BitmapFontText,
        SpriteFontText,
        ConsoleText
    }
}