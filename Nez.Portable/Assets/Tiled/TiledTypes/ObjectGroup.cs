using Microsoft.Xna.Framework;

namespace Nez.Tiled;

public class TmxObjectGroup : ITmxLayer
{
    public Color Color;
    public DrawOrderType DrawOrder;
    public TmxMap Map;

    public TmxList<TmxObject> Objects;
    public string Name { get; set; }
    public float Opacity { get; set; }
    public bool Visible { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public float ParallaxFactorX { get; set; }
    public float ParallaxFactorY { get; set; }
    public PropertyDict Properties { get; set; }
}

public class TmxObject : ITmxElement
{
    public float Height;
    public int Id;
    public TmxObjectType ObjectType;

    public Vector2[] Points;
    public PropertyDict Properties;
    public float Rotation;
    public TmxText Text;
    public TmxLayerTile Tile;
    public string Type;
    public bool Visible;
    public float Width;
    public float X;
    public float Y;
    public string Name { get; set; }
}

public class TmxText
{
    public TmxAlignment Alignment;
    public bool Bold;
    public Color Color;
    public string FontFamily;
    public bool Italic;
    public bool Kerning;
    public int PixelSize;
    public bool Strikeout;
    public bool Underline;
    public string Value;
    public bool Wrap;
}

public class TmxAlignment
{
    public TmxHorizontalAlignment Horizontal;
    public TmxVerticalAlignment Vertical;
}

public enum TmxObjectType
{
    Basic,
    Point,
    Tile,
    Ellipse,
    Polygon,
    Polyline,
    Text
}

public enum DrawOrderType
{
    UnknownOrder = -1,
    TopDown,
    IndexOrder
}

public enum TmxHorizontalAlignment
{
    Left,
    Center,
    Right,
    Justify
}

public enum TmxVerticalAlignment
{
    Top,
    Center,
    Bottom
}