﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Nez
{
	/// <summary>
	/// this effect requires that you render it twice. The first time horizontally (prepareForHorizontalBlur) and then
	/// vertically (prepareForVerticalBlur).
	/// </summary>
	public class GaussianBlurEffect : Effect
	{
		/// <summary>
		/// amount to blur. A range of 0.5 - 6 works well. Defaults to 2.
		/// </summary>
		/// <value>The blur amount.</value>
		[Range(0f, 6f, 0.2f)]
		public float BlurAmount
		{
			get => _blurAmount;
			set
			{
				if (_blurAmount != value)
				{
					// avoid 0 which will get is NaNs
					if (value == 0)
						value = 0.001f;

					_blurAmount = value;
					CalculateSampleWeights();
				}
			}
		}

		/// <summary>
		/// horizontal delta for the blur. Typically 1 / backbuffer width
		/// </summary>
		/// <value>The horizontal blur delta.</value>
		[Range(0.0001f, 0.005f, true)]
		public float HorizontalBlurDelta
		{
			get => _horizontalBlurDelta;
			set
			{
				if (value != _horizontalBlurDelta)
				{
					_horizontalBlurDelta = value;
					SetBlurEffectParameters(_horizontalBlurDelta, 0, _horizontalSampleOffsets);
				}
			}
		}

		/// <summary>
		/// vertical delta for the blur. Typically 1 / backbuffer height
		/// </summary>
		/// <value>The vertical blur delta.</value>
		[Range(0.0001f, 0.005f, true)]
		public float VerticalBlurDelta
		{
			get => _verticalBlurDelta;
			set
			{
				if (value != _verticalBlurDelta)
				{
					_verticalBlurDelta = value;
					SetBlurEffectParameters(0, _verticalBlurDelta, _verticalSampleOffsets);
				}
			}
		}

		private float _blurAmount = 2f;
		private float _horizontalBlurDelta = 0.01f;
		private float _verticalBlurDelta = 0.01f;
		private int _sampleCount = 16;
		private float[] _sampleWeights;
		private Vector2[] _verticalSampleOffsets;
		private Vector2[] _horizontalSampleOffsets;
		private EffectParameter _gaussTexParam;
		private EffectParameter _gaussTexRowParam;
        private EffectParameter _gaussTexOffsetMultiplierParam;
        private EffectParameter _gaussTexWidthParam;
		private EffectParameter _gaussTexWidthInvParam;
		private Texture2D _gaussTexture;


		public GaussianBlurEffect() : base(Core.GraphicsDevice, EffectResource.GaussianBlurBytes)
		{
			_gaussTexParam = Parameters["gaussTexture"];
			_gaussTexRowParam = Parameters["texRow"];
            _gaussTexOffsetMultiplierParam = Parameters["offsetMultiplier"];
            _gaussTexWidthParam = Parameters["gausTexWidth"];
			_gaussTexWidthInvParam = Parameters["gausTexWidthInv"];

			// Look up how many samples our gaussian blur effect supports.
			_gaussTexWidthParam.SetValue((float)_sampleCount);
			_gaussTexWidthInvParam.SetValue(1f / _sampleCount);

			// Create temporary arrays for computing our filter settings.
			_sampleWeights = new float[_sampleCount];
			_verticalSampleOffsets = new Vector2[_sampleCount];
			_horizontalSampleOffsets = new Vector2[_sampleCount];
			_gaussTexture = new Texture2D(GraphicsDevice, _sampleCount, 2, false, SurfaceFormat.Color);

			// The first sample always has a zero offset.
			_verticalSampleOffsets[0] = Vector2.Zero;
			_horizontalSampleOffsets[0] = Vector2.Zero;

			// we can calculate the sample weights just once since they are always the same for horizontal or vertical blur
			CalculateSampleWeights();

			SetBlurEffectParameters(_horizontalBlurDelta, 0, _horizontalSampleOffsets);
			SetBlurEffectParameters(_verticalBlurDelta, 0, _verticalSampleOffsets);
			GenerateGaussTexture();
			PrepareForHorizontalBlur();
		}

		/// <summary>
		/// prepares the Effect for performing a horizontal blur
		/// </summary>
		public void PrepareForHorizontalBlur() => _gaussTexRowParam.SetValue(0.25f);

		/// <summary>
		/// prepares the Effect for performing a vertical blur
		/// </summary>
		public void PrepareForVerticalBlur() => _gaussTexRowParam.SetValue(0.75f);

		/// <summary>
		/// computes sample weightings and texture coordinate offsets for one pass of a separable gaussian blur filter.
		/// </summary>
		private void SetBlurEffectParameters(float dx, float dy, Vector2[] offsets)
		{
			// Add pairs of additional sample taps, positioned along a line in both directions from the center.
			for (int i = 0; i < _sampleCount / 2; i++)
			{
				// To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
				// hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
				// will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
				// than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
				float sampleOffset = i * 2 + 1.5f;

				var delta = new Vector2(dx, dy) * sampleOffset;

				// Store texture coordinate offsets for the positive and negative taps.
				offsets[i * 2 + 1] = delta;
				offsets[i * 2 + 2] = -delta;
			}
		}

		/// <summary>
		/// calculates the sample weights and passes them along to the shader
		/// </summary>
		private void CalculateSampleWeights()
		{
			// The first sample always has a zero offset.
			_sampleWeights[0] = ComputeGaussian(0);

			// Maintain a sum of all the weighting values.
			float totalWeights = _sampleWeights[0];

			// Add pairs of additional sample taps, positioned along a line in both directions from the center.
			for (int i = 0; i < _sampleCount / 2; i++)
			{
				// Store weights for the positive and negative taps.
				float weight = ComputeGaussian(i + 1);

				_sampleWeights[i * 2 + 1] = weight;
				_sampleWeights[i * 2 + 2] = weight;

				totalWeights += weight * 2;
			}

			// Normalize the list of sample weightings, so they will always sum to one.
			for (int i = 0; i < _sampleWeights.Length; i++)
				_sampleWeights[i] /= totalWeights;
		}

		private void GenerateGaussTexture()
		{
			var maxOffset = Mathf.Sqrt(_horizontalSampleOffsets.Union(_verticalSampleOffsets).Max(x => x.LengthSquared()));
			var maxOffsetInv = 1 / maxOffset;
			_gaussTexOffsetMultiplierParam.SetValue(maxOffset);

            var data = new Color[_sampleCount * 2];
			for (int i = 0; i < _sampleCount; i++) data[i] = new Color(_sampleWeights[i], _horizontalSampleOffsets[i].X * maxOffsetInv, _horizontalSampleOffsets[i].Y * maxOffsetInv, 1f);
			for (int i = _sampleCount; i < _sampleCount * 2; i++) data[i] = new Color(_sampleWeights[i], _verticalSampleOffsets[i].X * maxOffsetInv, _verticalSampleOffsets[i].Y * maxOffsetInv, 1f);

			_gaussTexture.SetData(data);
			_gaussTexParam.SetValue(_gaussTexture);
			_gaussTexture.SaveAsPng(System.IO.File.OpenWrite("lol.png"), _gaussTexture.Width, _gaussTexture.Height);
        }

		/// <summary>
		/// Evaluates a single point on the gaussian falloff curve.
		/// Used for setting up the blur filter weightings.
		/// </summary>
		private float ComputeGaussian(float n) => (float)((1.0 / Math.Sqrt(2 * Math.PI * _blurAmount)) *
							Math.Exp(-(n * n) / (2 * _blurAmount * _blurAmount)));
	}
}