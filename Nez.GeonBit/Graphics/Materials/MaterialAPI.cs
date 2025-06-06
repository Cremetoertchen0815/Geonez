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
// Material base class.
// A material is a MonoGame effect wrapper + per-instance settings, such as 
// diffuse color, lightings, etc.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;

namespace Nez.GeonBit.Materials;

/// <summary>
///     GeonBit.Materials contain all the built-in materials.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     Track which material parameters need to be recomputed during the next OnApply.
/// </summary>
public enum MaterialDirtyFlags
{
    /// <summary>
    ///     Change in world matrix.
    /// </summary>
    World = 1 << 0,

    /// <summary>
    ///     Change in light sources, not including ambient or emissive.
    /// </summary>
    LightSources = 1 << 1,

    /// <summary>
    ///     Change in material color params (can be diffuse, specular, etc. This includes specular power as well.)
    /// </summary>
    MaterialColors = 1 << 2,

    /// <summary>
    ///     Change in material alpha.
    /// </summary>
    Alpha = 1 << 3,

    /// <summary>
    ///     Change in texture, texture enabled, or other texture-related params.
    /// </summary>
    TextureParams = 1 << 4,

    /// <summary>
    ///     Lighting params changed (enabled disabled / smooth lighting mode).
    /// </summary>
    LightingParams = 1 << 5,

    /// <summary>
    ///     Change in fog-related params.
    /// </summary>
    Fog = 1 << 6,

    /// <summary>
    ///     Chage in alpha-test related params.
    /// </summary>
    AlphaTest = 1 << 7,

    /// <summary>
    ///     Change in skinned mesh bone transformations.
    /// </summary>
    Bones = 1 << 8,

    /// <summary>
    ///     Change in ambient light color.
    /// </summary>
    AmbientLight = 1 << 9,

    /// <summary>
    ///     Change in emissive light color.
    /// </summary>
    EmissiveLight = 1 << 10,

    /// <summary>
    ///     Change in the normal map (enabled/disabled, map)
    /// </summary>
    NormalMap = 1 << 11,

    /// <summary>
    ///     Change in the shadow map (enabled/disabled, depth bias)
    /// </summary>
    ShadowMap = 1 << 12,

    /// <summary>
    ///     Change in the  environment map settings.
    /// </summary>
    EnvironmentMap = 1 << 13,

    /// <summary>
    ///     All dirty flags.
    /// </summary>
    All = int.MaxValue
}

/// <summary>
///     A callback to call per technique pass when using material iterate.
/// </summary>
/// <param name="pass">Current pass.</param>
public delegate void EffectPassCallback(EffectPass pass);

/// <summary>
///     Sampler states we can use for materials textures.
/// </summary>
public static class SamplerStates
{
    /// <summary>
    ///     AnisotropicClamp sampler state.
    /// </summary>
    public static SamplerState AnisotropicClamp => SamplerState.AnisotropicClamp;

    /// <summary>
    ///     AnisotropicWrap sampler state.
    /// </summary>
    public static SamplerState AnisotropicWrap => SamplerState.AnisotropicWrap;

    /// <summary>
    ///     LinearClamp sampler state.
    /// </summary>
    public static SamplerState LinearClamp => SamplerState.LinearClamp;

    /// <summary>
    ///     PointClamp sampler state.
    /// </summary>
    public static SamplerState PointClamp => SamplerState.PointClamp;

    /// <summary>
    ///     PointWrap sampler state.
    /// </summary>
    public static SamplerState PointWrap => SamplerState.PointWrap;
}

/// <summary>
///     The base class for a material.
///     Note: for some material types one or more of the properties below may be ignored.
///     For example, we might have a material that doesn't support lighting at all, and will ignore lighting-related
///     properties.
/// </summary>
public abstract class MaterialAPI
{
    // last material used
    internal static MaterialAPI _lastMaterialApplied;

    /// <summary>
    ///     Path of GeonBit built-in effects.
    /// </summary>
    public static readonly string EffectsPath = "engine/fx/geon/";

    // current view matrix (shared by all materials)
    private static Matrix _view;

    // current camera position (shared by all materials)
    private static Vector3 _eyePosition;

    // current projection matrix (shared by all materials)
    private static Matrix _projection;

    // view-projection matrix (multiply of view and projection)
    private static Matrix _viewProjection;

    // current view matrix version, used so we'll only update materials view when needed.
    private static uint _globalViewMatrixVersion = 1;

    // current projection matrix version, used so we'll only update materials projection when needed.
    private static uint _globalProjectionMatrixVersion = 1;

    /// <summary>
    ///     Default sampler state.
    /// </summary>
    public static SamplerState DefaultSamplerState = SamplerState.LinearWrap;

    private float _alpha;

    private Color _ambientLight;

    private Color _diffuseColor;

    /// <summary>
    ///     Params dirty flags.
    /// </summary>
    private int _dirtyFlags = (int)MaterialDirtyFlags.All;

    private Color _emissiveLight;

    private Color _fogColor;

    private bool _fogEnabled;

    private (float start, float end) _fogRange;

    // local projection matrix version
    private uint _projectionMatrixVersion;

    private Color _specularColor;

    private float _specularPower;

    private Texture2D _texture;

    private bool _textureEnabled;

    // local view matrix version
    private uint _viewMatrixVersion;

    private Matrix _world;

    /// <summary>
    ///     Create the material object.
    /// </summary>
    public MaterialAPI()
    {
        // count the object creation
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.AddedOrCreated);
    }

    /// <summary>
    ///     Get the effect instance.
    /// </summary>
    public abstract Effect Effect { get; }

    /// <summary>
    ///     Get how many samplers this material uses.
    /// </summary>
    protected virtual int SamplersCount => 1;

    /// <summary>
    ///     Return if this material support dynamic lighting.
    /// </summary>
    public virtual bool LightingEnabled => false;

    /// <summary>
    ///     Get how many lights this material support on the same render pass.
    /// </summary>
    protected virtual int MaxLights => 3;

    /// <summary>
    ///     Diffuse color.
    /// </summary>
    public virtual Color DiffuseColor
    {
        get => _diffuseColor;
        set
        {
            _diffuseColor = value;
            SetAsDirty(MaterialDirtyFlags.MaterialColors);
        }
    }

    /// <summary>
    ///     Specular color.
    /// </summary>
    public virtual Color SpecularColor
    {
        get => _specularColor;
        set
        {
            _specularColor = value;
            SetAsDirty(MaterialDirtyFlags.MaterialColors);
        }
    }

    /// <summary>
    ///     Ambient light color.
    /// </summary>
    internal virtual Color AmbientLight => _ambientLight;

    /// <summary>
    ///     Emissive light color.
    /// </summary>
    public virtual Color EmissiveLight
    {
        get => _emissiveLight;
        set
        {
            _emissiveLight = value;
            SetAsDirty(MaterialDirtyFlags.EmissiveLight);
        }
    }

    /// <summary>
    ///     Specular power.
    /// </summary>
    public virtual float SpecularPower
    {
        get => _specularPower;
        set
        {
            _specularPower = value;
            SetAsDirty(MaterialDirtyFlags.MaterialColors);
        }
    }

    /// <summary>
    ///     Opacity levels (multiplied with color opacity).
    /// </summary>
    public virtual float Alpha
    {
        get => _alpha;
        set
        {
            _alpha = value;
            SetAsDirty(MaterialDirtyFlags.Alpha);
        }
    }

    /// <summary>
    ///     Texture to draw.
    /// </summary>
    public virtual Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            SetAsDirty(MaterialDirtyFlags.TextureParams);
        }
    }

    /// <summary>
    ///     Is texture currently enabled.
    /// </summary>
    public virtual bool TextureEnabled
    {
        get => _textureEnabled;
        set
        {
            _textureEnabled = value;
            SetAsDirty(MaterialDirtyFlags.TextureParams);
        }
    }


    /// <summary>
    ///     Specular color.
    /// </summary>
    public virtual (float start, float end) FogRange
    {
        get => _fogRange;
        set
        {
            _fogRange = value;
            SetAsDirty(MaterialDirtyFlags.Fog);
        }
    }

    public virtual Color FogColor
    {
        get => _fogColor;
        set
        {
            _fogColor = value;
            SetAsDirty(MaterialDirtyFlags.Fog);
        }
    }

    public virtual bool FogEnabled
    {
        get => _fogEnabled;
        set
        {
            _fogEnabled = value;
            SetAsDirty(MaterialDirtyFlags.Fog);
        }
    }

    /// <summary>
    ///     Current world transformations.
    /// </summary>
    public virtual Matrix World
    {
        get => _world;
        set
        {
            _world = value;
            SetAsDirty(MaterialDirtyFlags.World);
        }
    }

    /// <summary>
    ///     Current view matrix.
    /// </summary>
    public virtual Matrix View => _view;

    /// <summary>
    ///     Current projection matrix.
    /// </summary>
    public virtual Matrix Projection => _projection;

    /// <summary>
    ///     Current view-projection matrix.
    /// </summary>
    public virtual Matrix ViewProjection => _viewProjection;

    public virtual Vector3 EyePosition => _eyePosition;

    /// <summary>
    ///     Sampler state.
    /// </summary>
    public SamplerState SamplerState { get; set; } = DefaultSamplerState;

    /// <summary>
    ///     If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call
    ///     ApplyLights() with the lights from manager.
    /// </summary>
    protected virtual bool UseDefaultLightsManager => false;

    /// <summary>
    ///     Add to dirty flags.
    /// </summary>
    /// <param name="val">Value to add to dirty flags using the or operator.</param>
    protected void SetAsDirty(int val)
    {
        _dirtyFlags |= val;
    }

    /// <summary>
    ///     Add to dirty flags.
    /// </summary>
    /// <param name="val">Value to add to dirty flags using the or operator.</param>
    protected void SetAsDirty(MaterialDirtyFlags val)
    {
        _dirtyFlags |= (int)val;
    }

    /// <summary>
    ///     Check dirty flags.
    /// </summary>
    /// <param name="val">Value to test if dirty.</param>
    protected bool IsDirty(int val)
    {
        return (_dirtyFlags & val) != 0;
    }

    /// <summary>
    ///     Add to dirty flags.
    /// </summary>
    /// <param name="val">Value to add to dirty flags using the or operator.</param>
    protected bool IsDirty(MaterialDirtyFlags val)
    {
        return (_dirtyFlags & (int)val) != 0;
    }

    /// <summary>
    ///     Set materials view and projection matrixes (shared by all materials).
    /// </summary>
    /// <param name="view">Current view matrix.</param>
    /// <param name="projection">Current projection matrix.</param>
    public static void SetViewProjection(Matrix view, Matrix projection)
    {
        // update view
        if (_view != view)
        {
            _view = view;
            _globalViewMatrixVersion++;
            _eyePosition = Matrix.Invert(_view).Translation;
        }

        // update projection
        if (_projection != projection)
        {
            _projection = projection;
            _globalProjectionMatrixVersion++;
        }

        // update view/projection matrix
        _viewProjection = _view * _projection;
    }

    /// <summary>
    ///     Clone this material.
    /// </summary>
    /// <returns>Copy of this material.</returns>
    public abstract MaterialAPI Clone();

    /// <summary>
    ///     Apply sampler state of this material.
    /// </summary>
    protected virtual void ApplySamplerState()
    {
        var states = Core.GraphicsDevice.SamplerStates;
        for (var i = 0; i < SamplersCount; ++i)
            if (states[i] != SamplerState)
                states[i] = SamplerState;
    }

    /// <summary>
    ///     Apply all new properties on the material effect.
    ///     Call this whenever you want to draw using this material.
    /// </summary>
    /// <param name="worldMatrix">The world transformations of the currently rendered entity.</param>
    /// <param name="boundingSphere">The bounding sphere (should be already transformed) of the rendered entity.</param>
    public void Apply(ref Matrix worldMatrix, ref BoundingSphere boundingSphere, int ShadowID)
    {
        // set world matrix
        World = worldMatrix;

        // apply sampler state
        ApplySamplerState();

        // update view if needed
        if (_viewMatrixVersion != _globalViewMatrixVersion)
        {
            UpdateView(ref _view);
            _viewMatrixVersion = _globalViewMatrixVersion;
        }

        // update projection if needed
        if (_projectionMatrixVersion != _globalProjectionMatrixVersion)
        {
            UpdateProjection(ref _projection);
            _projectionMatrixVersion = _globalProjectionMatrixVersion;
        }

        // if support light get lights and set them
        if (LightingEnabled && UseDefaultLightsManager)
        {
            // get lights in rendering range
            var lightsManager = GeonDefaultRenderer.ActiveLightsManager;
            var lights = lightsManager.GetLights(this, ShadowID, ref boundingSphere, MaxLights);

            // set ambient light
            if (AmbientLight != lightsManager.AmbientLight)
            {
                _ambientLight = lightsManager.AmbientLight;
                SetAsDirty(MaterialDirtyFlags.AmbientLight);
            }

            ApplyLights(lights, ref worldMatrix, ref boundingSphere);
            ArrayPool<ILightSource>.Shared.Return(lights, true);
        }

        // set effect tag to point on self, and call the per-effect specific apply
        if (Effect.Tag == null) Effect.Tag = this;
        MaterialSpecificApply(_lastMaterialApplied == this);

        // set last material applied to self
        _lastMaterialApplied = this;

        // clear flags
        _dirtyFlags = 0;
    }

    /// <summary>
    ///     Apply light sources on this material.
    /// </summary>
    /// <param name="lights">Array of light sources to apply.</param>
    /// <param name="worldMatrix">World transforms of the rendering object.</param>
    /// <param name="boundingSphere">Bounding sphere (after world transformation applied) of the rendering object.</param>
    protected virtual void ApplyLights(ILightSource[] lights, ref Matrix worldMatrix, ref BoundingSphere boundingSphere)
    {
    }

    /// <summary>
    ///     Apply all new properties on the material effect, implemented per material type.
    /// </summary>
    /// <param name="wasLastMaterial">
    ///     If true, it means this material was the last material applied. Useful for internal
    ///     optimizations.
    /// </param>
    protected abstract void MaterialSpecificApply(bool wasLastMaterial);

    /// <summary>
    ///     Update material view matrix.
    /// </summary>
    /// <param name="view">New view to set.</param>
    protected abstract void UpdateView(ref Matrix view);

    /// <summary>
    ///     Update material projection matrix.
    /// </summary>
    /// <param name="projection">New projection to set.</param>
    protected abstract void UpdateProjection(ref Matrix projection);

    /// <summary>
    ///     Set bone transforms for an animated material.
    ///     Useable only for materials that implement skinned animation in shader.
    /// </summary>
    /// <param name="bones">Bones to set.</param>
    public virtual void SetBoneTransforms(Matrix[] bones)
    {
        throw new InvalidOperationException("Material does not support bone transformations in GPU.");
    }

    /// <summary>
    ///     Iterate over all passes in current technique and call the provided callback for each pass.
    ///     You can use this function to draw stuff manually.
    /// </summary>
    /// <param name="callback">The callback to call for every pass.</param>
    public void IterateEffectPasses(EffectPassCallback callback)
    {
        // render the buffer with effect
        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            // draw current pass
            pass.Apply();

            // call the draw callback
            callback(pass);
        }
    }

    /// <summary>
    ///     Clone all the basic properties of a material.
    /// </summary>
    /// <param name="cloned">Cloned material to copy properties into.</param>
    protected void CloneBasics(ref MaterialAPI cloned)
    {
        cloned.World = World;
        cloned.TextureEnabled = TextureEnabled;
        cloned.Texture = Texture;
        cloned.Alpha = Alpha;
        cloned.DiffuseColor = DiffuseColor;
        cloned.SpecularColor = SpecularColor;
        cloned.SpecularPower = SpecularPower;
        cloned.EmissiveLight = EmissiveLight;
        cloned.SamplerState = SamplerState;
    }

    /// <summary>
    ///     Set default value for all the basic properties.
    /// </summary>
    public void SetDefaults()
    {
        World = Matrix.Identity;
        TextureEnabled = false;
        Texture = null;
        Alpha = 1f;
        DiffuseColor = Color.White;
        SpecularColor = Color.White;
        EmissiveLight = Color.Black;
        SpecularPower = 1f;
        SamplerState = DefaultSamplerState;
    }
}