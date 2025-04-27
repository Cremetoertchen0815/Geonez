using System;
using Microsoft.Xna.Framework;

namespace Nez.Tiled;

public partial class TmxMap : TmxDocument, IDisposable
{
    public Color BackgroundColor;
    public TmxList<TmxGroup> Groups;
    public int Height;
    public int? HexSideLength;
    public TmxList<TmxImageLayer> ImageLayers;

    /// <summary>
    ///     contains all of the ITmxLayers, regardless of their specific type. Note that layers in a TmxGroup will not
    ///     be in this list. TmxGroup manages its own layers list.
    /// </summary>
    public TmxList<ITmxLayer> Layers;

    /// <summary>
    ///     when we have an image tileset, tiles can be any size so we record the max size for culling
    /// </summary>
    public int MaxTileHeight;


    /// <summary>
    ///     when we have an image tileset, tiles can be any size so we record the max size for culling
    /// </summary>
    public int MaxTileWidth;

    public int? NextObjectID;
    public TmxList<TmxObjectGroup> ObjectGroups;
    public OrientationType Orientation;

    public PropertyDict Properties;
    public RenderOrderType RenderOrder;
    public StaggerAxisType StaggerAxis;
    public StaggerIndexType StaggerIndex;
    public string TiledVersion;
    public int TileHeight;
    public TmxList<TmxLayer> TileLayers;

    public TmxList<TmxTileset> Tilesets;
    public int TileWidth;
    public string Version;
    public int Width;
    public int WorldWidth => Width * TileWidth;
    public int WorldHeight => Height * TileHeight;

    /// <summary>
    ///     does this map have non-default tile sizes that would require special culling?
    /// </summary>
    public bool RequiresLargeTileCulling => MaxTileWidth > TileWidth || MaxTileHeight > TileHeight;

    /// <summary>
    ///     currently only used to tick all the Tilesets so they can update their animated tiles
    /// </summary>
    public void Update()
    {
        foreach (var tileset in Tilesets)
            tileset.Update();
    }

    #region IDisposable Support

    private bool _isDisposed;

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                foreach (var tileset in Tilesets)
                    tileset.Image?.Dispose();

                foreach (var layer in ImageLayers)
                    layer.Image?.Dispose();
            }

            _isDisposed = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
    }

    #endregion
}

public enum OrientationType
{
    Unknown,
    Orthogonal,
    Isometric,
    Staggered,
    Hexagonal
}

public enum StaggerAxisType
{
    X,
    Y
}

public enum StaggerIndexType
{
    Odd,
    Even
}

public enum RenderOrderType
{
    RightDown,
    RightUp,
    LeftDown,
    LeftUp
}