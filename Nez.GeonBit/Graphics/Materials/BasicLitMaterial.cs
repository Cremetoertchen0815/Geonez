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
// A basic one-pass lit material.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Graphics.Misc;
using Nez.GeonBit.Lights;
using System;

namespace Nez.GeonBit.Materials
{
    /// <summary>
    /// A material that supports ambient + several directional lights, with shadows and normal mapping.
    /// </summary>
    public class BasicLitMaterial : MaterialAPI
    {
        // effect path
        private static readonly string _effectPath = EffectsPath + "lighting_regular_";

        // the effect instance of this material.
        protected Effect _effect;

        /// <summary>
        /// Get the effect instance.
        /// </summary>
        public override Effect Effect => _effect;

        /// <summary>
        /// If true, will use the currently set lights manager in `Graphics.GeonBitRenderer.LightsManager` and call ApplyLights() with the lights from manager.
        /// </summary>
        protected override bool UseDefaultLightsManager => true;

        // caching of lights-related params from shader
        protected EffectParameter[] _lightsDiffuse = new EffectParameter[MaxLightsCount];
        protected EffectParameter[] _lightsDirections = new EffectParameter[MaxLightsCount];
        protected EffectParameter[] _lightsSpecular = new EffectParameter[MaxLightsCount];
        protected EffectParameter _fogColorParam;
        protected EffectParameter _fogVectorParam;
        protected EffectParameter _paramActiveLights;

        // effect parameters
        protected EffectParameterCollection _effectParams;
        protected EffectParameter _paramWorld;
        protected EffectParameter _paramWorldViewProjection;
        protected EffectParameter _paramWorldInverseTranspose;
        protected EffectParameter _paramEyePosition;
        protected EffectParameter _paramDiffuseColor;
        protected EffectParameter _paramEmissiveColor;
        protected EffectParameter _paramSpecularPower;
        protected EffectParameter _paramAlbedoMap;
        protected EffectParameter _paramAlbedoEnabled;
        protected EffectParameter _paramNormalMap;
        protected EffectParameter _paramShadowViewProjection;
        protected EffectParameter _paramDepthBias;
        protected EffectParameter _paramShadowMap;

        private int _activeLightsCount = 0;
        private FXModes _shaderConfig = FXModes.UVCoords;
        private FXModes _oldShaderConfig;
        private PCFQuality _oldShadowQuality;

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


        public virtual PCFQuality? ShadowQuality { get; set; }

        /// <summary>
        /// Normal map texture.
        /// </summary>
        public virtual Texture2D NormalTexture
        {
            get => _normalTexture;
            set { _normalTexture = value; SetAsDirty(MaterialDirtyFlags.TextureParams); }
        }

        private Texture2D _normalTexture;

        public override (float start, float end) FogRange { get => base.FogRange == default ? GeonDefaultRenderer.ActiveLightsManager.FogRange : base.FogRange; set => base.FogRange = value; }
        public override Color FogColor { get => base.FogColor == default ? GeonDefaultRenderer.ActiveLightsManager.FogColor : base.FogColor; set => base.FogColor = value; }

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
        public virtual Effect CreateEffect() => Core.Content.Load<Effect>(_effectPath + _oldShadowQuality.ToString().ToLower()).Clone();

        /// <summary>
        /// Create the lit material from an empty effect.
        /// </summary>
        public BasicLitMaterial(PCFQuality? shadowQuality = null)
        {
            ShadowQuality = shadowQuality;
            _oldShadowQuality = ShadowQuality ?? LightsManager.ShadowQuality;
            _effect = CreateEffect();
            SetDefaults();
            InitLightParams();
        }

        /// <summary>
        /// Create the material from another material instance.
        /// </summary>
        /// <param name="other">Other material to clone.</param>
        public BasicLitMaterial(BasicLitMaterial other)
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
        public BasicLitMaterial(Effect fromEffect)
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
        public BasicLitMaterial(BasicEffect fromEffect, PCFQuality? shadowQuality = null, bool copyEffectProperties = true)
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
                AmbientLight = new Color(fromEffect.AmbientLightColor.X, fromEffect.AmbientLightColor.Y, fromEffect.AmbientLightColor.Z);
                DiffuseColor = new Color(fromEffect.DiffuseColor.X, fromEffect.DiffuseColor.Y, fromEffect.DiffuseColor.Z);
                SpecularColor = new Color(fromEffect.SpecularColor.X, fromEffect.SpecularColor.Y, fromEffect.SpecularColor.Z);
                SpecularPower = fromEffect.SpecularPower;
            }

            // init light params
            InitLightParams();
        }

        protected void ReloadEffect()
        {
            //Load new effect
            _effect = CreateEffect();
            InitLightParams();

            //Mark every parameter as dirty
            SetAsDirty(MaterialDirtyFlags.All);
            for (int i = 0; i < MaxLights; i++) _lastLightVersions[i] = 0;
            _activeLightsCount = 0;
            _lastShadowVersion = 0;
            _oldShaderConfig = 0;
        }

        /// <summary>
        /// Init light-related params from shader.
        /// </summary>
        protected virtual void InitLightParams()
        {
            _effectParams = _effect.Parameters;
            _lightsDiffuse[0] = _effectParams["LightDiffuseA"];
            _lightsDiffuse[1] = _effectParams["LightDiffuseB"];
            _lightsDiffuse[2] = _effectParams["LightDiffuseC"];
            _lightsDirections[0] = _effectParams["LightDirectionA"];
            _lightsDirections[1] = _effectParams["LightDirectionB"];
            _lightsDirections[2] = _effectParams["LightDirectionC"];
            _lightsSpecular[0] = _effectParams["LightSpecularA"];
            _lightsSpecular[1] = _effectParams["LightSpecularB"];
            _lightsSpecular[2] = _effectParams["LightSpecularC"];
            _fogColorParam = _effectParams["FogColor"];
            _fogVectorParam = _effectParams["FogVector"];
            _paramActiveLights = _effectParams["ActiveLightsCount"];

            // effect parameters
            _paramWorld = _effectParams["World"];
            _paramWorldViewProjection = _effectParams["WorldViewProjection"];
            _paramWorldInverseTranspose = _effectParams["WorldInverseTranspose"];
            _paramEyePosition = _effectParams["EyePosition"];
            _paramDiffuseColor = _effectParams["DiffuseColor"];
            _paramEmissiveColor = _effectParams["EmissiveColor"];
            _paramSpecularPower = _effectParams["SpecularPower"];
            _paramAlbedoMap = _effectParams["AlbedoMap"];
            _paramAlbedoEnabled = _effectParams["AlbedoEnabled"];
            _paramNormalMap = _effectParams["NormalMap"];
            _paramShadowViewProjection = _effectParams["ShadowViewProjection"];
            _paramDepthBias = _effectParams["DepthBias"];
            _paramShadowMap = _effectParams["ShadowMap"];
        }

        /// <summary>
        /// Apply this material.
        /// </summary>
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
                if (NormalTexture != null) _shaderConfig |= FXModes.NormalMap; else _shaderConfig &= ~FXModes.NormalMap;
                _paramNormalMap?.SetValue(NormalTexture);
            }

            if (IsDirty(MaterialDirtyFlags.MaterialColors) || IsDirty(MaterialDirtyFlags.Alpha))
            {
                _paramDiffuseColor.SetValue(DiffuseColor.ToVector4() * new Vector4(1f, 1f, 1f, Alpha));
                _paramSpecularPower.SetValue(SpecularPower);
                SetAsDirty(MaterialDirtyFlags.AmbientLight);
            }

            if (IsDirty(MaterialDirtyFlags.Fog))
            {
                _fogColorParam.SetValue(FogColor.ToVector3());
                SetFogVector(World * View, FogRange.start, FogRange.end, FogEnabled, _fogVectorParam);
            }

            if (IsDirty(MaterialDirtyFlags.ShadowMap))
            {
                _paramDepthBias.SetValue(ShadowBias);
            }

            //Set active technique
            if (_oldShaderConfig != _shaderConfig) _effect.CurrentTechnique = _effect.Techniques[((FXTechniques)_shaderConfig).ToString()];
            _oldShaderConfig = _shaderConfig;
        }

        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        internal static void SetFogVector(Matrix worldView, float fogStart, float fogEnd, bool enabled, EffectParameter fogVectorParam)
        {
            if (!enabled)
            {
                // Degenerate case: force everything to 0% fogged if fog is not enabled.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 0));
            }
            else if (fogStart == fogEnd)
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
                _paramEmissiveColor.SetValue(AmbientLight.ToVector3() * DiffuseColor.ToVector3() + EmissiveLight.ToVector3());
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

            if (shadowedLight is not null)
            {
                _paramShadowViewProjection.SetValue(shadowedLight.ShadowViewMatrix * shadowedLight.ShadowProjectionMatrix);
                if (shadowedLight.ParamsVersion != _lastShadowVersion) _paramShadowMap.SetValue(shadowedLight.ShadowMap);
                _lastShadowVersion = shadowedLight.ParamsVersion;
            }
            if (shadowedLight is not null && ShadowsEnabled) _shaderConfig |= FXModes.ShadowMap; else _shaderConfig &= ~FXModes.ShadowMap;

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
        public override MaterialAPI Clone() => new BasicLitMaterial(this);

        [Flags]
        public enum FXModes
        {
            NormalMap = 0b1000,
            ShadowMap = 0b0100,
            VertexColors = 0b0010,
            UVCoords = 0b0001
        }

        [Flags]
        public enum FXTechniques
        {
            FlatNoShadowVc = FXModes.VertexColors,
            FlatNoShadowUv = FXModes.UVCoords,
            FlatNoShadowVcUv = FXModes.VertexColors | FXModes.UVCoords,
            FlatShadowVc = FXModes.ShadowMap | FXModes.VertexColors,
            FlatShadowUv = FXModes.ShadowMap | FXModes.UVCoords,
            FlatShadowVcUv = FXModes.ShadowMap | FXModes.VertexColors | FXModes.UVCoords,
            NormalNoShadowUv = FXModes.NormalMap | FXModes.UVCoords,
            NormalNoShadowVcUv = FXModes.NormalMap | FXModes.VertexColors | FXModes.UVCoords,
            NormalShadowUv = FXModes.NormalMap | FXModes.ShadowMap | FXModes.UVCoords,
            NormalShadowVcUv = FXModes.NormalMap | FXModes.ShadowMap | FXModes.VertexColors | FXModes.UVCoords
        }
    }


}
