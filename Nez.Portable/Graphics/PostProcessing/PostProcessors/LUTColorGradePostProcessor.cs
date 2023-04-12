﻿using Microsoft.Xna.Framework.Graphics;

namespace Nez
{
	public class LUTColorGradePostProcessor : PostProcessor
	{
		private EffectParameter _LUT;
		private EffectParameter _SizeParameter;
		private EffectParameter _SizeRootParameter;
		private EffectParameter _WidthParameter;
		private EffectParameter _HeightParameter;
		private float _Size;
		private float _SizeRoot;

		public LUTColorGradePostProcessor(int execOrder) : base(execOrder)
		{

			Effect = Core.Content.LoadEffect<Effect>("LUTColorGrade", EffectResource.LUTColorGrade);
			_LUT = Effect.Parameters["LUT"];
			_SizeParameter = Effect.Parameters["Size"];
			_SizeRootParameter = Effect.Parameters["SizeRoot"];
			_WidthParameter = Effect.Parameters["width"];
			_HeightParameter = Effect.Parameters["height"];

			LUT = Core.Content.LoadTexture("nez/textures/defaultLUT");
			Size = 32;
			SizeRoot = 8;
		}

		public float Size
		{
			get => _SizeParameter.GetValueSingle();
			set
			{
				_Size = value;
				_SizeParameter.SetValue(value);
				RecalcSize();
			}
		}

		public float SizeRoot
		{
			get => _SizeRootParameter.GetValueSingle();
			set
			{
				_SizeRoot = value;
				_SizeRootParameter.SetValue(value);
				RecalcSize();
			}
		}

		private void RecalcSize()
		{
			_WidthParameter.SetValue(Size * SizeRoot);
			_HeightParameter.SetValue(Size * Size / SizeRoot);
		}

		public Texture2D LUT
		{
			set => _LUT.SetValue(value);
		}
	}
}
