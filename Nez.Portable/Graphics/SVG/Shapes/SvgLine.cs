﻿using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Nez.Svg;

public class SvgLine : SvgElement
{
    [XmlAttribute("x1")] public float X1;

    [XmlAttribute("x2")] public float X2;

    [XmlAttribute("y1")] public float Y1;

    [XmlAttribute("y2")] public float Y2;

    public Vector2 Start => new(X1, Y1);

    public Vector2 End => new(X2, Y2);


    public Vector2[] GetTransformedPoints()
    {
        var pts = new[] { Start, End };
        var mat = GetCombinedMatrix();
        Vector2Ext.Transform(pts, ref mat, pts);

        return pts;
    }
}