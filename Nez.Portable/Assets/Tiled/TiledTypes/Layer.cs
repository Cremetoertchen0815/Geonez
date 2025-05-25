using Microsoft.Xna.Framework;

namespace Nez.Tiled;

public partial class TmxLayer : ITmxLayer
{
    /// <summary>
    ///     height in tiles for this layer. Always the same as the map height for fixed-size maps.
    /// </summary>
    public int Height;

    public TmxMap Map;
    public TmxLayerTile[] Tiles;

    /// <summary>
    ///     width in tiles for this layer. Always the same as the map width for fixed-size maps.
    /// </summary>
    public int Width;

    public Vector2 Offset => new(OffsetX, OffsetY);
    public Vector2 ParallaxFactor => new(ParallaxFactorX, ParallaxFactorY);
    public string Name { get; set; }
    public float Opacity { get; set; }
    public bool Visible { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public float ParallaxFactorX { get; set; }
    public float ParallaxFactorY { get; set; }

    public PropertyDict Properties { get; set; }

    /// <summary>
    ///     returns the TmxLayerTile with gid. This is a slow lookup so cache it!
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public TmxLayerTile GetTileWithGid(int gid)
    {
        for (var i = 0; i < Tiles.Length; i++)
            if (Tiles[i] != null && Tiles[i].Gid == gid)
                return Tiles[i];
        return null;
    }
}

public class TmxLayerTile
{
    private const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
    private const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
    private const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;
    private int? _tilesetTileIndex;
    public bool DiagonalFlip;
    public int Gid;
    public bool HorizontalFlip;

    public TmxTileset Tileset;
    public bool VerticalFlip;
    public int X;
    public int Y;

    public TmxLayerTile(TmxMap map, uint id, int x, int y)
    {
        X = x;
        Y = y;
        var rawGid = id;

        // Scan for tile flip bit flags
        var flip = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
        HorizontalFlip = flip;

        flip = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
        VerticalFlip = flip;

        flip = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
        DiagonalFlip = flip;

        // Zero the bit flags
        rawGid &= ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

        // Save GID remainder to int
        Gid = (int)rawGid;
        Tileset = map.GetTilesetForTileGid(Gid);
    }

    public Vector2 Position => new(X, Y);

    /// <summary>
    ///     gets the TmxTilesetTile for this TmxLayerTile if it exists. TmxTilesetTile only exist for animated tiles and tiles
    ///     with attached
    ///     properties.
    /// </summary>
    public TmxTilesetTile TilesetTile
    {
        get
        {
            if (!_tilesetTileIndex.HasValue)
            {
                _tilesetTileIndex = -1;
                if (Tileset.FirstGid <= Gid)
                    if (Tileset.Tiles.TryGetValue(Gid - Tileset.FirstGid, out _))
                        _tilesetTileIndex = Gid - Tileset.FirstGid;
            }

            if (_tilesetTileIndex.Value < 0)
                return null;

            return Tileset.Tiles[_tilesetTileIndex.Value];
        }
    }
}