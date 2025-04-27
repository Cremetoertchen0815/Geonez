using System.Collections.Generic;

namespace Nez.Tiled;

public class TmxTileset : TmxDocument, ITmxElement
{
    public int? Columns;
    public int FirstGid;
    public TmxImage Image;
    public TmxMap Map;
    public int Margin;
    public PropertyDict Properties;
    public int Spacing;
    public TmxList<TmxTerrain> Terrains;
    public int? TileCount;
    public int TileHeight;
    public TmxTileOffset TileOffset;

    /// <summary>
    ///     cache of the source rectangles for each tile
    /// </summary>
    public Dictionary<int, RectangleF> TileRegions;

    public Dictionary<int, TmxTilesetTile> Tiles;
    public int TileWidth;
    public string Name { get; set; }

    public void Update()
    {
        foreach (var kvPair in Tiles)
            kvPair.Value.UpdateAnimatedTiles();
    }
}

public class TmxTileOffset
{
    public int X;
    public int Y;
}

public class TmxTerrain : ITmxElement
{
    public PropertyDict Properties;
    public int Tile;
    public string Name { get; set; }
}