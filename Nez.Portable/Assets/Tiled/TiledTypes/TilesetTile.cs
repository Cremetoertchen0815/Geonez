using System.Collections.Generic;

namespace Nez.Tiled;

public class TmxTilesetTile
{
    private int _animationCurrentFrame;

    private float _animationElapsedTime;
    public List<TmxAnimationFrame> AnimationFrames;

    public int Id;
    public TmxImage Image;

    /// <summary>
    ///     returns the value of an "nez:isDestructable" property if present in the properties dictionary
    /// </summary>
    /// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
    public bool IsDestructable;

    /// <summary>
    ///     returns the value of a "nez:isOneWayPlatform" property if present in the properties dictionary
    /// </summary>
    public bool IsOneWayPlatform;

    /// <summary>
    ///     returns the value of a "nez:isSlope" property if present in the properties dictionary
    /// </summary>
    /// <value>The is slope.</value>
    public bool IsSlope;

    public TmxList<TmxObjectGroup> ObjectGroups;
    public double Probability;

    public PropertyDict Properties;

    /// <summary>
    ///     returns the value of a "nez:slopeTopLeft" property if present in the properties dictionary
    /// </summary>
    /// <value>The slope top left.</value>
    public int SlopeTopLeft;

    /// <summary>
    ///     returns the value of a "nez:slopeTopRight" property if present in the properties dictionary
    /// </summary>
    /// <value>The slope top right.</value>
    public int SlopeTopRight;

    public TmxTerrain[] TerrainEdges;
    public TmxTileset Tileset;
    public string Type;

    // HACK: why do animated tiles need to add the firstGid?
    public int currentAnimationFrameGid => AnimationFrames[_animationCurrentFrame].Gid + Tileset.FirstGid;

    public bool CanWallJump { get; set; }

    public void ProcessProperties()
    {
        if (Properties.TryGetValue("nez:isDestructable", out var value))
            IsDestructable = bool.Parse(value);

        if (Properties.TryGetValue("nez:isSlope", out value))
            IsSlope = bool.Parse(value);

        if (Properties.TryGetValue("nez:isOneWayPlatform", out value))
            IsOneWayPlatform = bool.Parse(value);

        if (Properties.TryGetValue("nez:slopeTopLeft", out value))
            SlopeTopLeft = int.Parse(value);

        if (Properties.TryGetValue("nez:slopeTopRight", out value))
            SlopeTopRight = int.Parse(value);

        if (Properties.TryGetValue("canWallJump", out value))
            CanWallJump = bool.Parse(value);
    }

    public void UpdateAnimatedTiles()
    {
        if (AnimationFrames.Count == 0)
            return;

        _animationElapsedTime += Time.DeltaTime;

        if (_animationElapsedTime > AnimationFrames[_animationCurrentFrame].Duration)
        {
            _animationCurrentFrame = Mathf.IncrementWithWrap(_animationCurrentFrame, AnimationFrames.Count);
            _animationElapsedTime = 0;
        }
    }
}

public class TmxAnimationFrame
{
    public float Duration;
    public int Gid;
}