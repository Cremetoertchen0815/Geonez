#region LICENSE
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
// A basic one-pass lit material.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using System;

namespace Nez.GeonBit.Materials
{
    /// <summary>
    /// A material that support ambient + several point / directional lights.
    /// </summary>
    public class LitMaterial : MaterialAPI
    {
        // effect path
        private static readonly string _effectPath = EffectsPath + "lighting_regular";

        // the effect instance of this material.
        private readonly Effect _effect;

        /// <summary>
        /// Get the effect instance.
        /// </summary>
        public override Effect Effect => _effect;

        /// <summary>
        /// If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call ApplyLights() with the lights from manager.
        /// </summary>
        protected override bool UseDefaultLightsManager => true;

        // caching of lights-related params from shader
        private EffectParameter[] _lightsDiffuse = new EffectParameter[MaxLightsCount];
        private EffectParameter[] _lightsDirections = new EffectParameter[MaxLightsCount];
        private EffectParameter[] _lightsSpecular = new EffectParameter[MaxLightsCount];
        private EffectParameter _fogColorParam;
        private EffectParameter _fogVectorParam;
        private EffectParameter _paramActiveLights;

        // effect parameters
        private EffectParameterCollection _effectParams;
        private EffectParameter _paramWorld;
        private EffectParameter _paramWorldViewProjection;
        private EffectParameter _paramWorldInverseTranspose;
        private EffectParameter _paramEyePosition;
        private EffectParameter _paramDiffuseColor;
        private EffectParameter _paramEmissiveColor;
        private EffectParameter _paramSpecularPower;
        private EffectParameter _paramAlbedoMap;
        private EffectParameter _paramAlbedoEnabled;
        private EffectParameter _paramNormalMap;
        private EffectParameter _paramShadowViewProjection;
        private EffectParameter _paramDepthBias;
        private EffectParameter _paramShadowMap;

        // current active lights counter
        private int _activeLightsCount = 0;
        private LitFXModes _shaderConfig = 0;
        private LitFXModes _oldShaderConfig = 0;

        /// <summary>
        /// Shadow depth bias to prevent shadow acne.
        /// </summary>
        public virtual float ShadowBias
        {
            get => _shadowBias;
            set { _shadowBias = value; SetAsDirty(MaterialDirtyFlags.ShadowMap); }
        }

        private float _shadowBias = 0f;

        /// <summary>
        /// Shadow depth bias to prevent shadow acne.
        /// </summary>
        public virtual bool ShadowsEnabled
        {
            get => _shadowEnabled;
            set { _shadowEnabled = value; SetAsDirty(MaterialDirtyFlags.ShadowMap); }
        }

        private bool _shadowEnabled = true;

        /// <summary>
        /// Normal map texture.
        /// </summary>
        public virtual Texture2D NormalTexture
        {
            get => _normalTexture;
            set { _normalTexture = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
        }

        private Texture2D _normalTexture;

        private bool _fogEnabled = false;

        /// <summary>
        /// Get how many samplers this material uses.
        /// </summary>
        protected override int SamplersCount => _normalTexture == null ? 1 : 2;

        // How many lights we can support at the same time. based on effect definition.
        private static readonly int MaxLightsCount = 3;

        // cache of lights we applied
        private readonly ILightSource[] _lastLights = new ILightSource[MaxLightsCount];

        // cache of lights last known params version
        private readonly uint[] _lastLightVersions = new uint[MaxLightsCount];
        private uint _lastShadowVersion = 0;

        /// <summary>
        /// Return if this material support dynamic lighting.
        /// </summary>
        public override bool LightingEnabled => true;

        /// <summary>
        /// Create new lit effect instance.
        /// </summary>
        /// <returns>New lit effect instance.</returns>
        public virtual Effect CreateEffect() => Core.Content.Load<Effect>(_effectPath).Clone();

        /// <summary>
        /// Create the lit material from an empty effect.
        /// </summary>
        public LitMaterial()
        {
            _effect = CreateEffect();
            SetDefaults();
            InitLightParams();
        }

        /// <summary>
        /// Create the material from another material instance.
        /// </summary>
        /// <param name="other">Other material to clone.</param>
        public LitMaterial(LitMaterial other)
        {
            // clone effect and set defaults
            _effect = other._effect.Clone();
            MaterialAPI asBase = this;
            other.CloneBasics(ref asBase);

            // init light params
            InitLightParams();
        }

        /// <summary>
        /// Create the lit material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        public LitMaterial(Effect fromEffect)
        {
            // clone effect and set defaults
            _effect = fromEffect.Clone();
            SetDefaults();

            // init light params
            InitLightParams();
        }

        /// <summary>
        /// Create the lit material.
        /// </summary>
        /// <param name="fromEffect">Effect to create material from.</param>
        /// <param name="copyEffectProperties">If true, will copy initial properties from effect.</param>
        public LitMaterial(BasicEffect fromEffect, bool copyEffectProperties = true)
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
                AmbientLight = new Color(fromEffect.AmbientLightColor.X, fromEffect.AmbientLightColor.Y, fromEffect.AmbientLightColor.Z);
                DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);
                SpecularColor = new Color(fromEffect.SpecularColor.X, fromEffect.SpecularColor.Y, fromEffect.SpecularColor.Z);
                SpecularPower = fromEffect.SpecularPower;
            }

            // init light params
            InitLightParams();
        }

        /// <summary>
        /// Init light-related params from shader.
        /// </summary>
        private void InitLightParams()
        {
            _effectParams = _effect.Parameters;
            _lightsDiffuse[0] = _effectParams["LightDiffuseA"]; //Implemented
            _lightsDiffuse[1] = _effectParams["LightDiffuseB"]; //Implemented
            _lightsDiffuse[2] = _effectParams["LightDiffuseC"]; //Implemented
            _lightsDirections[0] = _effectParams["LightDirectionA"]; //Implemented
            _lightsDirections[1] = _effectParams["LightDirectionB"]; //Implemented
            _lightsDirections[2] = _effectParams["LightDirectionC"]; //Implemented
            _lightsSpecular[0] = _effectParams["LightSpecularA"]; //Implemented
            _lightsSpecular[1] = _effectParams["LightSpecularB"]; //Implemented
            _lightsSpecular[2] = _effectParams["LightSpecularC"]; //Implemented
            _fogColorParam = _effectParams["FogColor"]; //Implemented
            _fogVectorParam = _effectParams["FogVector"]; //Implemented
            _paramActiveLights = _effectParams["ActiveLightsCount"]; //Implemented

            // effect parameters
            _paramWorld = _effectParams["World"]; //Implemented
            _paramWorldViewProjection = _effectParams["WorldViewProjection"]; //Implemented
            _paramWorldInverseTranspose = _effectParams["WorldInverseTranspose"]; //Implemented
            _paramEyePosition = _effectParams["EyePosition"]; //Implemented
            _paramDiffuseColor = _effectParams["DiffuseColor"]; //Implemented
            _paramEmissiveColor = _effectParams["EmissiveColor"]; //Implemented
            _paramSpecularPower = _effectParams["SpecularPower"]; //Implemented
            _paramAlbedoMap = _effectParams["AlbedoMap"]; //Implemented
            _paramAlbedoEnabled = _effectParams["AlbedoEnabled"]; //Implemented
            _paramNormalMap = _effectParams["NormalMap"]; //Implemented
            _paramShadowViewProjection = _effectParams["ShadowViewProjection"]; //Implemented
            _paramDepthBias = _effectParams["DepthBias"]; //Implemented
            _paramShadowMap = _effectParams["ShadowMap"]; //Implemented
        }

        /// <summary>
        /// Apply this material.
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
                _paramWorldInverseTranspose.SetValue(Matrix.Invert(Matrix.Transpose(World)));
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
                if (TextureEnabled && NormalTexture != null) _shaderConfig |= LitFXModes.NormalMap; else _shaderConfig &= ~LitFXModes.NormalMap;
                _paramNormalMap?.SetValue(NormalTexture);
            }

            if (IsDirty(MaterialDirtyFlags.MaterialColors) || IsDirty(MaterialDirtyFlags.Alpha))
            {
                _paramDiffuseColor.SetValue(DiffuseColor.ToVector4() * new Vector4(1f, 1f, 1f, Alpha));
                _paramSpecularPower.SetValue(SpecularPower);
            }

            if (IsDirty(MaterialDirtyFlags.Fog))
            {
                _fogColorParam.SetValue(FogColor.ToVector3());
                SetFogVector(World, FogRange.start, FogRange.end, FogEnabled, _fogVectorParam);
            }

            if (IsDirty(MaterialDirtyFlags.ShadowMap))
            {
                _paramDepthBias.SetValue(ShadowBias);
            }

            //Set active technique
            if (_oldShaderConfig != _shaderConfig) _effect.CurrentTechnique = _effect.Techniques[((LitFXTechniques)_shaderConfig).ToString()];
            _oldShaderConfig = _shaderConfig;
        }

        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        static void SetFogVector(Matrix worldView, float fogStart, float fogEnd, bool enabled, EffectParameter fogVectorParam)
        {
            if (!enabled)
            {
                // Degenerate case: force everything to 0% fogged if fog is not enabled.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 0));
            }
            if (fogStart == fogEnd)
            {
                // Degenerate case: force everything to 100% fogged if start and end are the same.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 1));
            }
            else
            {
                // We want to transform vertex positions into view space, take the resulting
                // Z value, then scale and offset according to the fog start/end distances.
                // Because we only care about the Z component, the shader can do all this
                // with a single dot product, using only the Z row of the world+view matrix.

                float scale = 1f / (fogStart - fogEnd);

                Vector4 fogVector = new Vector4();

                fogVector.X = worldView.M13 * scale;
                fogVector.Y = worldView.M23 * scale;
                fogVector.Z = worldView.M33 * scale;
                fogVector.W = (worldView.M43 + fogStart) * scale;

                fogVectorParam.SetValue(fogVector);
            }
        }

        /// <summary>
        /// Update material view matrix.
        /// </summary>
        /// <param name="view">New view to set.</param>
        protected override void UpdateView(ref Matrix view)
        {
        }

        /// <summary>
        /// Update material projection matrix.
        /// </summary>
        /// <param name="projection">New projection to set.</param>
        protected override void UpdateProjection(ref Matrix projection)
        {
        }

        /// <summary>
        /// Apply light sources on this material.
        /// </summary>
        /// <param name="lights">Array of light sources to apply.</param>
        /// <param name="worldMatrix">World transforms of the rendering object.</param>
        /// <param name="boundingSphere">Bounding sphere (after world transformation applied) of the rendering object.</param>
        protected override void ApplyLights(ILightSource[] lights, ref Matrix worldMatrix, ref BoundingSphere boundingSphere)
        {
            // set global light params
            var nuAmbient = GeonDefaultRenderer.ActiveLightsManager.AmbientLight;
            if (AmbientLight != nuAmbient) AmbientLight = nuAmbient;

            if (IsDirty(MaterialDirtyFlags.EmissiveLight) || IsDirty(MaterialDirtyFlags.AmbientLight))
            {
                _paramEmissiveColor.SetValue(AmbientLight.ToVector3() + EmissiveLight.ToVector3());
            }

            // iterate on lights and apply only the changed ones
            int lightsCount = Math.Min(MaxLightsCount, lights.Length);
            IShadowedLight shadowedLight = null;
            for (int i = 0; i < lightsCount; ++i)
            {
                // get current light
                var light = lights[i];

                if (light is IShadowedLight sl && shadowedLight is null) shadowedLight = sl;

                // only if light changed
                if (_lastLights[i] == light && _lastLightVersions[i] == light.ParamsVersion) continue;

                // set lights data
                _lightsDiffuse[i].SetValue(light.Diffuse.ToVector3());
                _lightsDirections[i].SetValue(light.IsDirectionalLight ? Vector3.Normalize(light.Direction.Value) : Vector3.Zero); //Non-directional lights currently not supported by shader
                _lightsSpecular[i].SetValue(light.Specular.ToVector3());

                // store light in cache so we won't copy it next time if it haven't changed
                _lastLights[i] = lights[i];
                _lastLightVersions[i] = lights[i].ParamsVersion;
            }

            if (shadowedLight is not null && shadowedLight.ParamsVersion != _lastShadowVersion)
            {
                _paramShadowViewProjection.SetValue(shadowedLight.ShadowViewMatrix * shadowedLight.ShadowProjectionMatrix);
                _paramShadowMap.SetValue(shadowedLight.ShadowMap);
                _lastShadowVersion = shadowedLight.ParamsVersion;
            }
            if (shadowedLight is not null && ShadowsEnabled) _shaderConfig |= LitFXModes.ShadowMap; else _shaderConfig &= ~LitFXModes.ShadowMap;

            // update active lights count
            if (_activeLightsCount != lightsCount)
            {
                _activeLightsCount = lightsCount;
                _paramActiveLights.SetValue(_activeLightsCount);
            }
        }

        /// <summary>
        /// Clone this material.
        /// </summary>
        /// <returns>Copy of this material.</returns>
        public override MaterialAPI Clone() => new LitMaterial(this);
    }


    [Flags]
    public enum LitFXModes
    {
        NormalMap = 0b1000,
        ShadowMap = 0b0100,
        VertexColors = 0b0010,
        UVCoords = 0b0001
    }

    [Flags]
    public enum LitFXTechniques
    {
        FlatNoShadowVc = LitFXModes.VertexColors,
        FlatNoShadowUv = LitFXModes.UVCoords,
        FlatNoShadowVcUv = LitFXModes.VertexColors | LitFXModes.UVCoords,
        FlatShadowVc = LitFXModes.ShadowMap | LitFXModes.VertexColors,
        FlatShadowUv = LitFXModes.ShadowMap | LitFXModes.UVCoords,
        FlatShadowVcUv = LitFXModes.ShadowMap | LitFXModes.VertexColors | LitFXModes.UVCoords,
        NormalNoShadowUv = LitFXModes.NormalMap | LitFXModes.UVCoords,
        NormalNoShadowVcUv = LitFXModes.NormalMap | LitFXModes.VertexColors | LitFXModes.UVCoords,
        NormalShadowUv = LitFXModes.NormalMap | LitFXModes.ShadowMap | LitFXModes.UVCoords,
        NormalShadowVcUv = LitFXModes.NormalMap | LitFXModes.ShadowMap | LitFXModes.VertexColors | LitFXModes.UVCoords
    }

}
