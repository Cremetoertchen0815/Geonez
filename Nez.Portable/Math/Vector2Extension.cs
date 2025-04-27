using System;
using Microsoft.Xna.Framework;

namespace Nez;

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        var sin = (float)Math.Sin(degrees);
        var cos = (float)Math.Cos(degrees);

        var tx = v.X;
        var ty = v.Y;
        v.X = cos * tx - sin * ty;
        v.Y = sin * tx + cos * ty;
        return v;
    }
}