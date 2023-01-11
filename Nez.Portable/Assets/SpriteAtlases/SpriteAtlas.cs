﻿using Nez.Textures;
using System;

namespace Nez.Sprites
{
	/// <summary>
	/// A collection of indexable Sprites and Sprite Animations
	/// </summary>
	public class SpriteAtlas : IDisposable
	{
		public string[] Names;
		public Sprite[] Sprites;

		public int[] NonAnimationSprites;

		public string[] AnimationNames;
		public SpriteAnimation[] SpriteAnimations;

		public Sprite GetSprite(string name)
		{
			int index = Array.IndexOf(Names, name);
			return Sprites[index];
		}

		public SpriteAnimation GetAnimation(string name)
		{
			int index = Array.IndexOf(AnimationNames, name);
			return SpriteAnimations[index];
		}

		void IDisposable.Dispose()
		{
			// all our Sprites use the same Texture so we only need to dispose one of them
			if (Sprites != null)
			{
				Sprites[0].Texture2D.Dispose();
				Sprites = null;
			}
		}
	}
}
