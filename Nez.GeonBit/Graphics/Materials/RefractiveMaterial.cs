using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;

namespace Nez.GeonBit.Materials;

public class RefractiveMaterial : MaterialAPI
{
    [Flags]
    public enum FXModes
    {
        NormalMap = 0b100,
        VertexColors = 0b010,
        UVCoords = 0b001
    }

    [Flags]
    public enum FXTechniques
    {
        FlatVc = FXModes.VertexColors,
        FlatUv = FXModes.UVCoords,
        FlatVcUv = FXModes.VertexColors | FXModes.UVCoords,
        NormalUv = FXModes.NormalMap | FXModes.UVCoords,
        NormalVcUv = FXModes.NormalMap | FXModes.VertexColors | FXModes.UVCoords
    }

    // effect path
    private static readonly string _effectPath = EffectsPath + "lighting_refractive";

    // the effect instance of this material.
    protected Effect _effect;

    // effect parameters
    protected EffectParameterCollection _effectParams;


    private TextureCube _environmentMap;

    private Color _environmentSpecular;

    // caching of lights-related params from shader
    protected EffectParameter _fogColorParam;
    protected EffectParameter _fogVectorParam;

    private float _fresnelFactor;

    private Texture2D _normalTexture;
    private FXModes _oldShaderConfig;
    private float _oldWorldRefIndex = 1f;
    protected EffectParameter _paramDiffuseColor;
    protected EffectParameter _paramEmissiveColor;
    protected EffectParameter _paramEnvMap;
    protected EffectParameter _paramEnvMapAmount;
    protected EffectParameter _paramEnvMapSpecular;
    protected EffectParameter _paramEyePosition;
    protected EffectParameter _paramFresnel;
    protected EffectParameter _paramNormalMap;
    protected EffectParameter _paramRefractionIndex;
    protected EffectParameter _paramWorld;
    protected EffectParameter _paramWorldInverseTranspose;
    protected EffectParameter _paramWorldViewProjection;

    private float _refractionIndex = 1.000293f;

    private FXModes _shaderConfig = FXModes.UVCoords;

    /// <summary>
    ///     Create the lit material from an empty effect.
    /// </summary>
    public RefractiveMaterial()
    {
        _effect = CreateEffect();
        SetDefaults();
        InitLightParams();
    }

    /// <summary>
    ///     Create the material from another material instance.
    /// </summary>
    /// <param name="other">Other material to clone.</param>
    public RefractiveMaterial(RefractiveMaterial other)
    {
        // clone effect and set defaults
        _effect = other._effect.Clone();
        MaterialAPI asBase = this;
        other.CloneBasics(ref asBase);

        // init light params
        InitLightParams();
    }

    /// <summary>
    ///     Create the lit material.
    /// </summary>
    /// <param name="fromEffect">Effect to create material from.</param>
    public RefractiveMaterial(Effect fromEffect)
    {
        // clone effect and set defaults
        _effect = fromEffect.Clone();
        SetDefaults();

        // init light params
        InitLightParams();
    }

    /// <summary>
    ///     Create the lit material.
    /// </summary>
    /// <param name="fromEffect">Effect to create material from.</param>
    /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
    public RefractiveMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
    {
        // store effect and set default properties
        _effect = CreateEffect();
        SetDefaults();

        // copy properties from effect itself
        if (copyEffectProperties)
        {
            // set effect defaults
            Texture = fromEffect.Texture;
            TextureEnabled = fromEffect.TextureEnabled;
            Alpha = fromEffect.Alpha;
            DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);
            SpecularColor = new Color(fromEffect.SpecularColor.X, fromEffect.SpecularColor.Y,
                fromEffect.SpecularColor.Z);
            SpecularPower = fromEffect.SpecularPower;
        }

        // init light params
        InitLightParams();
    }

    /// <summary>
    ///     Get the effect instance.
    /// </summary>
    public override Effect Effect => _effect;

    /// <summary>
    ///     If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call
    ///     ApplyLights() with the lights from manager.
    /// </summary>
    protected override bool UseDefaultLightsManager => true;

    /// <summary>
    ///     Normal map texture.
    /// </summary>
    public virtual Texture2D NormalTexture
    {
        get => _normalTexture;
        set
        {
            _normalTexture = value;
            SetAsDirty(MaterialDirtyFlags.TextureParams);
        }
    }

    public override (float start, float end) FogRange
    {
        get => base.FogRange == default ? GeonDefaultRenderer.ActiveLightsManager.FogRange : base.FogRange;
        set => base.FogRange = value;
    }

    public override Color FogColor
    {
        get => base.FogColor == default ? GeonDefaultRenderer.ActiveLightsManager.FogColor : base.FogColor;
        set => base.FogColor = value;
    }

    public TextureCube EnvironmentMap
    {
        get => _environmentMap;
        set
        {
            _environmentMap = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public Color EnvironmentSpecular
    {
        get => _environmentSpecular;
        set
        {
            _environmentSpecular = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public float FresnelFactor
    {
        get => _fresnelFactor;
        set
        {
            _fresnelFactor = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public float RefractionIndex
    {
        get => _refractionIndex;
        set
        {
            _refractionIndex = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }


    /// <summary>
    ///     Get how many samplers this material uses.
    /// </summary>
    protected override int SamplersCount => _normalTexture == null ? 1 : 2;

    /// <summary>
    ///     Return if this material support dynamic lighting.
    /// </summary>
    public override bool LightingEnabled => true;

    /// <summary>
    ///     Create new lit effect instance.
    /// </summary>
    /// <returns>New lit effect instance.</returns>
    public virtual Effect CreateEffect()
    {
        return Core.Content.Load<Effect>(_effectPath.ToLower()).Clone();
    }

    /// <summary>
    ///     Init light-related params from shader.
    /// </summary>
    protected virtual void InitLightParams()
    {
        _effectParams = _effect.Parameters;
        _fogColorParam = _effectParams["FogColor"];
        _fogVectorParam = _effectParams["FogVector"];

        // effect parameters
        _paramWorld = _effectParams["World"];
        _paramWorldViewProjection = _effectParams["WorldViewProjection"];
        _paramWorldInverseTranspose = _effectParams["WorldInverseTranspose"];
        _paramEyePosition = _effectParams["EyePosition"];
        _paramDiffuseColor = _effectParams["DiffuseColor"];
        _paramEmissiveColor = _effectParams["EmissiveColor"];
        _paramNormalMap = _effectParams["NormalMap"];
        _paramEnvMap = _effectParams["ReflectionCubeMap"];
        _paramEnvMapAmount = _effectParams["EnvironmentMapAmount"];
        _paramEnvMapSpecular = _effectParams["EnvironmentMapSpecular"];
        _paramFresnel = _effectParams["FresnelFactor"];
        _paramRefractionIndex = _effectParams["RefractionIndex"];
    }

    /// <summary>
    ///     Apply this material.
    /// </summary>
    protected override void MaterialSpecificApply(bool wasLastMaterial)
    {
        // set world matrix
        _paramWorldViewProjection.SetValue(World * ViewProjection);
        _paramEyePosition.SetValue(EyePosition);

        // set world matrix
        if (IsDirty(MaterialDirtyFlags.World))
        {
            _paramWorld.SetValue(World);
            var eyePos = Matrix.Invert(Matrix.Transpose(World));
            _paramWorldInverseTranspose.SetValue(eyePos);
            SetAsDirty(MaterialDirtyFlags.Fog);
        }


        // set all effect params
        if (IsDirty(MaterialDirtyFlags.TextureParams))
        {
            // set normal texture
            if (NormalTexture != null) _shaderConfig |= FXModes.NormalMap;
            else _shaderConfig &= ~FXModes.NormalMap;
            _paramNormalMap?.SetValue(NormalTexture);
        }


        var nuRefIndex = GeonDefaultRenderer.ActiveLightsManager.RefractionIndex;
        if (nuRefIndex != _oldWorldRefIndex)
        {
            _oldWorldRefIndex = nuRefIndex;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }

        if (IsDirty(MaterialDirtyFlags.EnvironmentMap))
        {
            // set environment map
            _paramEnvMap.SetValue(EnvironmentMap);
            _paramEnvMapSpecular.SetValue(EnvironmentSpecular.ToVector3());
            _paramFresnel.SetValue(FresnelFactor);
            _paramRefractionIndex.SetValue(GeonDefaultRenderer.ActiveLightsManager.RefractionIndex / RefractionIndex);
        }

        if (IsDirty(MaterialDirtyFlags.MaterialColors) || IsDirty(MaterialDirtyFlags.Alpha))
        {
            _paramDiffuseColor.SetValue(DiffuseColor.ToVector4() * new Vector4(1f, 1f, 1f, Alpha));
            SetAsDirty(MaterialDirtyFlags.AmbientLight);
        }

        if (IsDirty(MaterialDirtyFlags.Fog))
        {
            _fogColorParam.SetValue(FogColor.ToVector3());
            BasicLitMaterial.SetFogVector(World * View, FogRange.start, FogRange.end, FogEnabled, _fogVectorParam);
        }

        //Set active technique
        if (_oldShaderConfig != _shaderConfig)
            _effect.CurrentTechnique = _effect.Techniques[((FXTechniques)_shaderConfig).ToString()];
        _oldShaderConfig = _shaderConfig;
    }

    /// <summary>
    ///     Update material view matrix.
    /// </summary>
    /// <param name="view">New view to set.</param>
    protected override void UpdateView(ref Matrix view)
    {
    }

    /// <summary>
    ///     Update material projection matrix.
    /// </summary>
    /// <param name="projection">New projection to set.</param>
    protected override void UpdateProjection(ref Matrix projection)
    {
    }

    /// <summary>
    ///     Apply light sources on this material.
    /// </summary>
    /// <param name="lights">Array of light sources to apply.</param>
    /// <param name="worldMatrix">World transforms of the rendering object.</param>
    /// <param name="boundingSphere">Bounding sphere (after world transformation applied) of the rendering object.</param>
    protected override void ApplyLights(ILightSource[] lights, ref Matrix worldMatrix,
        ref BoundingSphere boundingSphere)
    {
        // set global light params

        if (IsDirty(MaterialDirtyFlags.EmissiveLight) || IsDirty(MaterialDirtyFlags.AmbientLight))
            _paramEmissiveColor.SetValue(
                AmbientLight.ToVector3() * DiffuseColor.ToVector3() + EmissiveLight.ToVector3());
    }

    /// <summary>
    ///     Clone this material.
    /// </summary>
    /// <returns>Copy of this material.</returns>
    public override MaterialAPI Clone()
    {
        return new RefractiveMaterial(this);
    }
}