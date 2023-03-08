using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights
{
	internal interface IShadowedLight : ILightSource
	{
		Matrix ViewMatrix { get; }
		Matrix ProjectionMatrix { get; }
		RenderTarget2D ShadowMap { get; }
	}
}
