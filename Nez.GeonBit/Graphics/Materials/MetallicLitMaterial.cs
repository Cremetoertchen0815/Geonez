using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Misc;
using Nez.GeonBit.Lights;

namespace Nez.GeonBit.Materials;

public class MetallicLitMaterial : BasicLitMaterial
{
    // effect path
    private static readonly string _effectPath = EffectsPath + "lighting_metal_";

    private float _environmentAmount;

    private TextureCube _environmentMap;

    private Color _environmentSpecular;

    private float _fresnelFactor;
    private FXModes _oldShaderConfig;
    private PcfQuality _oldShadowQuality;

    protected EffectParameter _paramEnvMap;
    protected EffectParameter _paramEnvMapAmount;
    protected EffectParameter _paramEnvMapSpecular;
    protected EffectParameter _paramFresnel;
    protected EffectParameter _paramRoughness;

    private float _roughness;

    private FXModes _shaderConfig = FXModes.UVCoords;


    /// <summary>
    ///     Create the lit material from an empty effect.
    /// </summary>
    public MetallicLitMaterial(PcfQuality? shadowQuality = null)
    {
        ShadowQuality = shadowQuality;
        _oldShadowQuality = ShadowQuality ?? LightsManager.ShadowQuality;
        _effect = CreateEffect();
        SetDefaults();
        InitLightParams();
    }

    /// <summary>
    ///     Create the material from another material instance.
    /// </summary>
    /// <param name="other">Other material to clone.</param>
    public MetallicLitMaterial(MetallicLitMaterial other)
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
    public MetallicLitMaterial(Effect fromEffect)
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
    public MetallicLitMaterial(BasicEffect fromEffect, PcfQuality? shadowQuality = null,
        bool copyEffectProperties = true)
    {
        // store effect and set default properties
        ShadowQuality = shadowQuality;
        _oldShadowQuality = ShadowQuality ?? LightsManager.ShadowQuality;
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

    public TextureCube EnvironmentMap
    {
        get => _environmentMap;
        set
        {
            _environmentMap = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }

    public float EnvironmentAmount
    {
        get => _environmentAmount;
        set
        {
            _environmentAmount = value;
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

    public float Roughness
    {
        get => _roughness;
        set
        {
            _roughness = value;
            SetAsDirty(MaterialDirtyFlags.EnvironmentMap);
        }
    }


    /// <summary>
    ///     Create new lit effect instance.
    /// </summary>
    /// <returns>New lit effect instance.</returns>
    public override Effect CreateEffect()
    {
        return Core.Content.Load<Effect>(_effectPath + _oldShadowQuality.ToString().ToLower()).Clone();
    }

    protected override void InitLightParams()
    {
        base.InitLightParams();

        _paramEnvMap = _effectParams["ReflectionCubeMap"];
        _paramEnvMapAmount = _effectParams["EnvironmentMapAmount"];
        _paramEnvMapSpecular = _effectParams["EnvironmentMapSpecular"];
        _paramFresnel = _effectParams["FresnelFactor"];
        _paramRoughness = _effectParams["ReflectionBlur"];
    }

    protected override void MaterialSpecificApply(bool wasLastMaterial)
    {
        // check for changing effect
        var currSQuality = ShadowQuality ?? LightsManager.ShadowQuality;
        if (currSQuality != _oldShadowQuality)
        {
            _oldShadowQuality = currSQuality;
            ReloadEffect();
        }

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
            if (_paramAlbedoMap != null)
            {
                _paramAlbedoEnabled.SetValue(TextureEnabled && Texture != null);
                _paramAlbedoMap.SetValue(Texture);
            }

            // set normal texture
            if (NormalTexture != null) _shaderConfig |= FXModes.NormalMap;
            else _shaderConfig &= ~FXModes.NormalMap;
            _paramNormalMap?.SetValue(NormalTexture);
        }

        if (IsDirty(MaterialDirtyFlags.EnvironmentMap))
        {
            // set environment map
            _paramEnvMap.SetValue(EnvironmentMap);
            _paramEnvMapAmount.SetValue(EnvironmentAmount);
            _paramEnvMapSpecular.SetValue(EnvironmentSpecular.ToVector3());
            _paramFresnel.SetValue(FresnelFactor);
            _paramRoughness.SetValue(Roughness);
        }

        if (IsDirty(MaterialDirtyFlags.MaterialColors) || IsDirty(MaterialDirtyFlags.Alpha))
        {
            _paramDiffuseColor.SetValue(DiffuseColor.ToVector4() * new Vector4(1f, 1f, 1f, Alpha));
            _paramSpecularColor.SetValue(SpecularColor.ToVector3());
            _paramSpecularPower.SetValue(SpecularPower);
            SetAsDirty(MaterialDirtyFlags.AmbientLight);
        }

        if (IsDirty(MaterialDirtyFlags.Fog))
        {
            _fogColorParam.SetValue(FogColor.ToVector3());
            SetFogVector(World * View, FogRange.start, FogRange.end, FogEnabled, _fogVectorParam);
        }

        if (IsDirty(MaterialDirtyFlags.ShadowMap)) _paramDepthBias.SetValue(ShadowBias);

        //Set active technique
        if (_oldShaderConfig != _shaderConfig)
            _effect.CurrentTechnique = _effect.Techniques[((FXTechniques)_shaderConfig).ToString()];
        _oldShaderConfig = _shaderConfig;
    }
}