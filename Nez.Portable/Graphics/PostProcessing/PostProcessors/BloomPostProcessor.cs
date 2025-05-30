﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

public class BloomPostProcessor : PostProcessor, IDisposable
{
	/// <summary>
	///     Dispose our RenderTargets. This is not covered by the Garbage Collector so we have to do it manually
	/// </summary>
	public void Dispose()
    {
        _bloomRenderTarget2DMip0.Dispose();
        _bloomRenderTarget2DMip1.Dispose();
        _bloomRenderTarget2DMip2.Dispose();
        _bloomRenderTarget2DMip3.Dispose();
        _bloomRenderTarget2DMip4.Dispose();
        _bloomRenderTarget2DMip5.Dispose();
    }

    private void ChangeBlendState()
    {
        _graphicsDev.BlendState = BlendState.AlphaBlend;
    }

    /// <summary>
    ///     Update the InverseResolution of the used rendertargets. This should be the InverseResolution of the processed image
    ///     We use SurfaceFormat.Color, but you can use higher precision buffers obviously.
    /// </summary>
    /// <param name="width">width of the image</param>
    /// <param name="height">height of the image</param>
    public void UpdateResolution(int width, int height)
    {
        _width = width;
        _height = height;

        if (_bloomRenderTarget2DMip0 != null) Dispose();

        _bloomRenderTarget2DMip0 = new RenderTarget2D(_graphicsDev,
            width,
            height, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        _bloomRenderTarget2DMip1 = new RenderTarget2D(_graphicsDev,
            width / 2,
            height / 2, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _bloomRenderTarget2DMip2 = new RenderTarget2D(_graphicsDev,
            width / 4,
            height / 4, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _bloomRenderTarget2DMip3 = new RenderTarget2D(_graphicsDev,
            width / 8,
            height / 8, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _bloomRenderTarget2DMip4 = new RenderTarget2D(_graphicsDev,
            width / 16,
            height / 16, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _bloomRenderTarget2DMip5 = new RenderTarget2D(_graphicsDev,
            width / 32,
            height / 32, false, _renderTargetFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    }

    #region fields & properties

    #region private fields

    //resolution
    private int _width;
    private int _height;

    //RenderTargets
    private RenderTarget2D _bloomRenderTarget2DMip0;
    private RenderTarget2D _bloomRenderTarget2DMip1;
    private RenderTarget2D _bloomRenderTarget2DMip2;
    private RenderTarget2D _bloomRenderTarget2DMip3;
    private RenderTarget2D _bloomRenderTarget2DMip4;
    private RenderTarget2D _bloomRenderTarget2DMip5;

    private readonly SurfaceFormat _renderTargetFormat;

    //Objects
    private readonly GraphicsDevice _graphicsDev;
    private readonly QuadRenderer _quadRenderer;
    private BlendState BlendStateBloom;

    //Shader + variables
    private readonly Effect _bloomEffect;
    private readonly Texture2D black;

    private readonly EffectPass _bloomPassExtract;
    private readonly EffectPass _bloomPassExtractLuminance;
    private readonly EffectPass _bloomPassDownsample;
    private readonly EffectPass _bloomPassUpsample;
    private readonly EffectPass _bloomPassUpsampleLuminance;

    private readonly EffectParameter _bloomParameterScreenTexture;
    private readonly EffectParameter _bloomInverseResolutionParameter;
    private readonly EffectParameter _bloomRadiusParameter;
    private readonly EffectParameter _bloomStrengthParameter;
    private readonly EffectParameter _bloomStreakLengthParameter;
    private readonly EffectParameter _bloomThresholdParameter;

    //Preset variables for different mip levels
    private float _bloomRadius1 = 1.0f;
    private float _bloomRadius2 = 1.0f;
    private float _bloomRadius3 = 1.0f;
    private float _bloomRadius4 = 1.0f;
    private float _bloomRadius5 = 1.0f;

    private float _bloomStrength1 = 1.0f;
    private float _bloomStrength2 = 1.0f;
    private float _bloomStrength3 = 1.0f;
    private float _bloomStrength4 = 1.0f;
    private float _bloomStrength5 = 1.0f;

    public float BloomStrengthMultiplier = 1.0f;

    private readonly float _radiusMultiplier = 1.0f;

    #endregion

    #region public fields + enums

    public bool BloomUseLuminance = true;
    public int BloomDownsamplePasses = 5;

    //enums
    public enum BloomPresets
    {
        Wide,
        Focussed,
        Small,
        SuperWide,
        Cheap,
        One
    }

    #endregion

    #region properties

    public BloomPresets BloomPreset
    {
        get => _bloomPreset;
        set
        {
            if (_bloomPreset == value) return;

            _bloomPreset = value;
            SetBloomPreset(_bloomPreset);
        }
    }

    private BloomPresets _bloomPreset = BloomPresets.SuperWide;


    private Texture2D BloomScreenTexture
    {
        set => _bloomParameterScreenTexture.SetValue(value);
    }

    private Vector2 BloomInverseResolution
    {
        get => _bloomInverseResolutionField;
        set
        {
            if (value != _bloomInverseResolutionField)
            {
                _bloomInverseResolutionField = value;
                _bloomInverseResolutionParameter.SetValue(_bloomInverseResolutionField);
            }
        }
    }

    private Vector2 _bloomInverseResolutionField;

    private float BloomRadius
    {
        get => _bloomRadius;

        set
        {
            if (Math.Abs(_bloomRadius - value) > 0.001f)
            {
                _bloomRadius = value;
                _bloomRadiusParameter?.SetValue(_bloomRadius * _radiusMultiplier);
            }
        }
    }

    private float _bloomRadius;

    private float BloomStrength
    {
        get => _bloomStrength;
        set
        {
            if (Math.Abs(_bloomStrength - value) > 0.001f)
            {
                _bloomStrength = value;
                _bloomStrengthParameter?.SetValue(_bloomStrength * BloomStrengthMultiplier);
            }
        }
    }

    private float _bloomStrength;

    public float BloomStreakLength
    {
        get => _bloomStreakLength;
        set
        {
            if (Math.Abs(_bloomStreakLength - value) > 0.001f)
            {
                _bloomStreakLength = value;
                _bloomStreakLengthParameter?.SetValue(_bloomStreakLength);
            }
        }
    }

    private float _bloomStreakLength;

    public float BloomThreshold
    {
        get => _bloomThreshold;
        set
        {
            if (Math.Abs(_bloomThreshold - value) > 0.001f)
            {
                _bloomThreshold = value;
                _bloomThresholdParameter?.SetValue(_bloomThreshold);
            }
        }
    }

    private float _bloomThreshold;


    public BloomPostProcessor SetThreshold(float val)
    {
        BloomThreshold = val;
        return this;
    }

    public BloomPostProcessor SetStrengthMultiplayer(float val)
    {
        BloomStrengthMultiplier = val;
        return this;
    }

    public BloomPostProcessor SetPreset(BloomPresets val)
    {
        SetBloomPreset(val);
        return this;
    }

    #endregion

    #endregion

    #region initialize

    public BloomPostProcessor(int executionOrder) : base(executionOrder)
    {
        //Temporaty fields
        var content = Core.Content;


        _graphicsDev = Core.GraphicsDevice;
        SamplerState = SamplerState.LinearWrap;
        //UpdateResolution(scene.SceneRenderTargetSize.X, scene.SceneRenderTargetSize.Y);

        //if quadRenderer == null -> new, otherwise not
        _quadRenderer = new QuadRenderer(_graphicsDev);
        Color[] tmp = { Color.Black };
        black = new Texture2D(_graphicsDev, 1, 1);
        black.SetData(tmp);

        _renderTargetFormat = SurfaceFormat.Color;

        //Load the shader parameters and passes for cheap and easy access
        _bloomEffect = content.LoadEffect<Effect>("qualityBloom", EffectResource.QualityBloom);
        _bloomInverseResolutionParameter = _bloomEffect.Parameters["InverseResolution"];
        _bloomRadiusParameter = _bloomEffect.Parameters["Radius"];
        _bloomStrengthParameter = _bloomEffect.Parameters["Strength"];
        _bloomStreakLengthParameter = _bloomEffect.Parameters["StreakLength"];
        _bloomThresholdParameter = _bloomEffect.Parameters["Threshold"];

        //For DirectX / Windows
        _bloomParameterScreenTexture = _bloomEffect.Parameters["ScreenTexture"];

        //If we are on OpenGL it's different, load the other one then!
        if (_bloomParameterScreenTexture == null)
            //for OpenGL / CrossPlatform
            _bloomParameterScreenTexture = _bloomEffect.Parameters["LinearSampler+ScreenTexture"];

        _bloomPassExtract = _bloomEffect.Techniques["Extract"].Passes[0];
        _bloomPassExtractLuminance = _bloomEffect.Techniques["ExtractLuminance"].Passes[0];
        _bloomPassDownsample = _bloomEffect.Techniques["Downsample"].Passes[0];
        _bloomPassUpsample = _bloomEffect.Techniques["Upsample"].Passes[0];
        _bloomPassUpsampleLuminance = _bloomEffect.Techniques["UpsampleLuminance"].Passes[0];

        //An interesting blendstate for merging the initial image with the bloom.
        BlendStateBloom = BlendState.Additive;

        //Setup the default preset values.
        //BloomPreset = BloomPresets.One;
        SetBloomPreset(_bloomPreset);
        //Default threshold.
        BloomThreshold = _bloomThreshold;

        BloomStrengthMultiplier = 1F;
    }

    public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
    {
        UpdateResolution(newWidth, newHeight);
    }

    public override void Process(RenderTarget2D source, RenderTarget2D destination)
    {
        SamplerState = SamplerState.AnisotropicClamp;
        //Check if we are initialized
        if (_graphicsDev == null)
            throw new Exception("Module not yet Loaded / Initialized. Use Load() first");

        _graphicsDev.RasterizerState = RasterizerState.CullNone;
        _graphicsDev.BlendState = BlendState.Opaque;
        _graphicsDev.SamplerStates[0] = SamplerState.LinearClamp;

        //EXTRACT  //Note: Is setRenderTargets(binding better?)
        //We extract the bright values which are above the Threshold and save them to Mip0
        _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip0);

        BloomScreenTexture = source;
        BloomInverseResolution = new Vector2(1.0f / _width, 1.0f / _height);

        if (BloomUseLuminance) _bloomPassExtractLuminance.Apply();
        else _bloomPassExtract.Apply();
        _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

        //Now downsample to the next lower mip texture
        if (BloomDownsamplePasses > 0)
        {
            //DOWNSAMPLE TO MIP1
            _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip1);

            BloomScreenTexture = _bloomRenderTarget2DMip0;
            //Pass
            _bloomPassDownsample.Apply();
            _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

            if (BloomDownsamplePasses > 1)
            {
                //Our input resolution is halfed, so our inverse 1/res. must be doubled
                BloomInverseResolution *= 2;

                //DOWNSAMPLE TO MIP2
                _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip2);

                BloomScreenTexture = _bloomRenderTarget2DMip1;
                //Pass
                _bloomPassDownsample.Apply();
                _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                if (BloomDownsamplePasses > 2)
                {
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP3
                    _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip3);

                    BloomScreenTexture = _bloomRenderTarget2DMip2;
                    //Pass
                    _bloomPassDownsample.Apply();
                    _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                    if (BloomDownsamplePasses > 3)
                    {
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP4
                        _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip4);

                        BloomScreenTexture = _bloomRenderTarget2DMip3;
                        //Pass
                        _bloomPassDownsample.Apply();
                        _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                        if (BloomDownsamplePasses > 4)
                        {
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP5
                            _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip5);

                            BloomScreenTexture = _bloomRenderTarget2DMip4;
                            //Pass
                            _bloomPassDownsample.Apply();
                            _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                            ChangeBlendState();

                            //UPSAMPLE TO MIP4
                            _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip4);
                            BloomScreenTexture = _bloomRenderTarget2DMip5;

                            BloomStrength = _bloomStrength5;
                            BloomRadius = _bloomRadius5;
                            if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                            else _bloomPassUpsample.Apply();
                            _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                            BloomInverseResolution /= 2;
                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP3
                        _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip3);
                        BloomScreenTexture = _bloomRenderTarget2DMip4;

                        BloomStrength = _bloomStrength4;
                        BloomRadius = _bloomRadius4;
                        if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                        else _bloomPassUpsample.Apply();
                        _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);
                        BloomInverseResolution /= 2;
                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP2
                    _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip2);
                    BloomScreenTexture = _bloomRenderTarget2DMip3;

                    BloomStrength = _bloomStrength3;
                    BloomRadius = _bloomRadius3;
                    if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                    else _bloomPassUpsample.Apply();
                    _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP1
                _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip1);
                BloomScreenTexture = _bloomRenderTarget2DMip2;

                BloomStrength = _bloomStrength2;
                BloomRadius = _bloomRadius2;
                if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                else _bloomPassUpsample.Apply();
                _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);

                BloomInverseResolution /= 2;
            }

            ChangeBlendState();

            //UPSAMPLE TO MIP0
            _graphicsDev.SetRenderTarget(_bloomRenderTarget2DMip0);
            BloomScreenTexture = _bloomRenderTarget2DMip1;

            BloomStrength = _bloomStrength1;
            BloomRadius = _bloomRadius1;

            if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
            else _bloomPassUpsample.Apply();
            _quadRenderer.RenderQuad(_graphicsDev, Vector2.One * -1, Vector2.One);
        }

        //Draw to destination
        BlendState = BlendState.Additive;
        DrawFullscreenQuad(_bloomRenderTarget2DMip0, destination);
        DrawFullscreenQuad(source, destination);
    }

    /// <summary>
    ///     A few presets with different values for the different mip levels of our bloom.
    /// </summary>
    /// <param name="preset">See BloomPresets enums. Example: BloomPresets.Wide</param>
    private void SetBloomPreset(BloomPresets preset)
    {
        switch (preset)
        {
            case BloomPresets.Wide:
            {
                _bloomStrength1 = 0.5f;
                _bloomStrength2 = 1;
                _bloomStrength3 = 2;
                _bloomStrength4 = 1;
                _bloomStrength5 = 2;
                _bloomRadius5 = 4.0f;
                _bloomRadius4 = 4.0f;
                _bloomRadius3 = 2.0f;
                _bloomRadius2 = 2.0f;
                _bloomRadius1 = 1.0f;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 5;
                break;
            }
            case BloomPresets.SuperWide:
            {
                _bloomStrength1 = 0.9f;
                _bloomStrength2 = 1;
                _bloomStrength3 = 1;
                _bloomStrength4 = 2;
                _bloomStrength5 = 6;
                _bloomRadius5 = 4.0f;
                _bloomRadius4 = 2.0f;
                _bloomRadius3 = 2.0f;
                _bloomRadius2 = 2.0f;
                _bloomRadius1 = 2.0f;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 5;
                break;
            }
            case BloomPresets.Focussed:
            {
                _bloomStrength1 = 0.8f;
                _bloomStrength2 = 1;
                _bloomStrength3 = 1;
                _bloomStrength4 = 1;
                _bloomStrength5 = 2;
                _bloomRadius5 = 4.0f;
                _bloomRadius4 = 2.0f;
                _bloomRadius3 = 2.0f;
                _bloomRadius2 = 2.0f;
                _bloomRadius1 = 2.0f;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 5;
                break;
            }
            case BloomPresets.Small:
            {
                _bloomStrength1 = 0.8f;
                _bloomStrength2 = 1;
                _bloomStrength3 = 1;
                _bloomStrength4 = 1;
                _bloomStrength5 = 1;
                _bloomRadius5 = 1;
                _bloomRadius4 = 1;
                _bloomRadius3 = 1;
                _bloomRadius2 = 1;
                _bloomRadius1 = 1;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 5;
                break;
            }
            case BloomPresets.Cheap:
            {
                _bloomStrength1 = 0.8f;
                _bloomStrength2 = 2;
                _bloomRadius2 = 2;
                _bloomRadius1 = 2;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 2;
                break;
            }
            case BloomPresets.One:
            {
                _bloomStrength1 = 4f;
                _bloomStrength2 = 1;
                _bloomStrength3 = 1;
                _bloomStrength4 = 1;
                _bloomStrength5 = 2;
                _bloomRadius5 = 1.0f;
                _bloomRadius4 = 1.0f;
                _bloomRadius3 = 1.0f;
                _bloomRadius2 = 1.0f;
                _bloomRadius1 = 1.0f;
                BloomStreakLength = 1;
                BloomDownsamplePasses = 5;
                break;
            }
        }
    }

    #endregion
}