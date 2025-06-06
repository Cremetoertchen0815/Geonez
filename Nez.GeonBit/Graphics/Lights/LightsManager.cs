﻿#region LICENSE

//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------

#endregion

#region File Description

//-----------------------------------------------------------------------------
// Default basic lights manager.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Graphics.Misc;
using Nez.GeonBit.Materials;

namespace Nez.GeonBit.Lights;

/// <summary>
///     Implement a default, basic lights manager.
/// </summary>
public class LightsManager
{
    // to return empty lights array.
    private static readonly ILightSource[] EmptyLightsArray = new ILightSource[0];

    public static bool ShadowsEnabed = true;

    public static PCFQuality ShadowQuality = PCFQuality.MidPCF;

    public static Point DefaultShadowMapResolution = new(1024);

    internal static readonly Effect ShadowEffect = new DepthPlaneEffect(Core.GraphicsDevice);

    // list with all lights currently in manager.
    private readonly List<ILightSource> _allLights = new();

    // list of lights that are infinite, eg have no range limit.
    private readonly List<ILightSource> _infiniteLights = new();

    // list of lights that are infinite, eg have no range limit.
    private readonly List<IRangedLight> _rangedLights = new();

    // dictionary of regions and the lights they contain.
    private readonly Dictionary<Vector3, List<IRangedLight>> _regions = new();

    private readonly ResizableRentedArray<ILightSource> _retLights = new();

    // list of lights that throw shadows
    private readonly List<IShadowedLight> _shadowLights = new();


    // ambient light value
    private Color _ambient = Color.Gray;

    // the size of a batch / region containing lights.
    private Vector3 _regionSize = new(250, 250, 250);

    /// <summary>
    ///     Ambient light.
    /// </summary>
    public Color AmbientLight
    {
        get => Enabled ? _ambient : Color.Black;
        set => _ambient = value;
    }

    public Color FogColor { get; set; } = Color.Black;
    public (float start, float end) FogRange { get; set; } = (0f, 100f);
    public float RefractionIndex { get; set; } = 1.000293f;

    /// <summary>
    ///     Enable / disable all lights.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Lights manager divide the world into segments, or regions, that contain lights.
    ///     When drawing entities we get the entity bounding sphere and and all the
    /// </summary>
    public Vector3 LightsRegionSize
    {
        get => _regionSize;
        set
        {
            _regionSize = value;
            UpdateLightsRegionSize();
        }
    }

    /// <summary>
    ///     Add a light source to lights manager.
    /// </summary>
    /// <param name="light">Light to add.</param>
    internal void AddLight(ILightSource light)
    {
        // add to list of lights
        _allLights.Add(light);

        // add light to lights map


        // if its infinite light add to infinite list
        if (light is IRangedLight rl)
        {
            rl.RecalcBoundingSphere(false);
            UpdateLightTransform(rl);
        }
        else if (light is IShadowedLight sl)
        {
            _shadowLights.Add(sl);
        }
        else
        {
            _infiniteLights.Add(light);
        }
    }

    /// <summary>
    ///     Remove a light source from lights manager.
    /// </summary>
    /// <param name="light">Light to remove.</param>
    internal void RemoveLight(ILightSource light)
    {
        // remove from list of lights
        if (light is IRangedLight rl)
        {
            RemoveLightFromItsRegions(rl);
            _rangedLights.Remove(rl);
        }
        else if (light is IShadowedLight sl)
        {
            _shadowLights.Remove(sl);
        }
        else
        {
            _allLights.Remove(light);
        }
    }

    internal IEnumerable<IShadowedLight> GetShadowedLights()
    {
        return _shadowLights.Where(x => x.Enabled);
    }

    /// <summary>
    ///     Get min region index for a given bounding sphere.
    /// </summary>
    /// <param name="boundingSphere">Bounding sphere to get min region for.</param>
    /// <returns>Min lights region index.</returns>
    private Vector3 GetMinRegionIndex(ref BoundingSphere boundingSphere)
    {
        var ret = boundingSphere.Center - Vector3.One * boundingSphere.Radius;
        ret /= LightsRegionSize;
        ret.X = (float)Math.Floor(ret.X);
        ret.Y = (float)Math.Floor(ret.Y);
        ret.Z = (float)Math.Floor(ret.Z);
        return ret;
    }

    /// <summary>
    ///     Get max region index for a given bounding sphere.
    /// </summary>
    /// <param name="boundingSphere">Bounding sphere to get min region for.</param>
    /// <returns>Min lights region index.</returns>
    private Vector3 GetMaxRegionIndex(ref BoundingSphere boundingSphere)
    {
        var ret = boundingSphere.Center + Vector3.One * boundingSphere.Radius;
        ret /= LightsRegionSize;
        ret.X = (float)Math.Floor(ret.X);
        ret.Y = (float)Math.Floor(ret.Y);
        ret.Z = (float)Math.Floor(ret.Z);
        return ret;
    }

    /// <summary>
    ///     Get all lights for a given bounding sphere.
    /// </summary>
    /// <param name="material">Material to get lights for.</param>
    /// <param name="boundingSphere">Rendering bounding sphere.</param>
    /// <param name="maxLights">Maximum lights count to return.</param>
    /// <returns>Array of lights to apply on this material and drawing. Note: directional lights must always come first!</returns>
    internal ILightSource[] GetLights(MaterialAPI material, int ShadowID, ref BoundingSphere boundingSphere,
        int maxLights)
    {
        // if disabled return empty lights array
        if (!Enabled) return EmptyLightsArray;

        _retLights.Clear();

        // if no lights at all, skip
        if (_regions.Count == 0 && _infiniteLights.Count == 0 && _shadowLights.Count == 0) return EmptyLightsArray;

        // get min and max points of this bounding sphere
        var min = GetMinRegionIndex(ref boundingSphere);
        var max = GetMaxRegionIndex(ref boundingSphere);

        // build array to return

        // Note: For now only one shadowable light is supported anyway, so any other shadow-projecting lights get ignored.
        //		 Should be changed later, once I actually get how to combine/efficiently store multiple shadowed lights
        var firstEnabledShadowLight = _shadowLights.FirstOrDefault(x => x.Enabled && x.ShadowSourceID == ShadowID);
        if (firstEnabledShadowLight is not null) _retLights.Add(firstEnabledShadowLight);

        // add all infinite lights first (directional lights etc)
        foreach (var light in _infiniteLights)
            if (light.Enabled)
                _retLights.Add(light);

        // iterate regions and add lights
        var isFirstRegionWeCheck = true;
        var index = new Vector3();
        for (var x = (int)min.X; x <= max.X; ++x)
        {
            index.X = x;
            for (var y = (int)min.Y; y <= max.Y; ++y)
            {
                index.Y = y;
                for (var z = (int)min.Z; z <= max.Z; ++z)
                {
                    index.Z = z;

                    // try to fetch region lights
                    if (_regions.TryGetValue(index, out var regionLights))
                    {
                        // iterate lights in region
                        foreach (var light in _regions[index])
                        {
                            // if light not visible, skip
                            if (!light.Enabled) continue;

                            // if its not first region we fetch, test against duplications
                            if (!isFirstRegionWeCheck && Array.IndexOf(_retLights.InternalArray, light) != -1) continue;

                            // make sure light really touch object
                            if (!boundingSphere.Intersects(light.BoundingSphere)) continue;

                            // if light is out of camera, skip it
                            if (!GeonDefaultRenderer.ActiveCamera.ViewFrustum.Intersects(light.BoundingSphere))
                                continue;

                            // add light to return array
                            _retLights.Add(light);

                            // if exceeded max lights stop here
                            if (_retLights.Count >= maxLights) break;
                        }

                        // no longer first region we test
                        isFirstRegionWeCheck = false;
                    }
                }
            }
        }

        // return the results array
        return _retLights.InternalArray;
    }

    /// <summary>
    ///     Called after user changed the lights region size.
    /// </summary>
    private void UpdateLightsRegionSize()
    {
        // clear regions dictionary
        _regions.Clear();

        // re-add all lights
        foreach (var light in _rangedLights) UpdateLightTransform(light);
    }

    /// <summary>
    ///     Remove a light from all the regions its in (assuming light is inside this lights manager).
    /// </summary>
    /// <param name="light">Light to remove from regions.</param>
    protected void RemoveLightFromItsRegions(IRangedLight light)
    {
        // get light metadata
        var min = light.MinRegionIndex;
        var max = light.MaxRegionIndex;

        // remove light from previous regions
        var index = new Vector3();
        for (var x = (int)min.X; x <= max.X; ++x)
        {
            index.X = x;
            for (var y = (int)min.Y; y <= max.Y; ++y)
            {
                index.Y = y;
                for (var z = (int)min.Z; z <= max.Z; ++z)
                {
                    index.Z = z;

                    // remove light from region
                    if (_regions.TryGetValue(index, out var region))
                    {
                        region.Remove(light);

                        // if region is now empty, remove it
                        if (region.Count == 0) _regions.Remove(index);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Update the transformations of a light inside this manager.
    /// </summary>
    /// <param name="light">Light to update.</param>
    internal void UpdateLightTransform(IRangedLight light)
    {
        // remove light from previous regions
        RemoveLightFromItsRegions(light);

        // calc new min and max for the light
        var boundingSphere = light.BoundingSphere;
        var min = GetMinRegionIndex(ref boundingSphere);
        var max = GetMaxRegionIndex(ref boundingSphere);

        // add light to new regions
        var index = new Vector3();
        for (var x = (int)min.X; x <= max.X; ++x)
        {
            index.X = x;
            for (var y = (int)min.Y; y <= max.Y; ++y)
            {
                index.Y = y;
                for (var z = (int)min.Z; z <= max.Z; ++z)
                {
                    index.Z = z;

                    // if region don't exist, create it
                    if (!_regions.ContainsKey(index)) _regions[index] = new List<IRangedLight>();

                    // add light to region
                    _regions[index].Add(light);
                }
            }
        }

        // update light's metadata
        light.MinRegionIndex = min;
        light.MaxRegionIndex = max;
    }
}