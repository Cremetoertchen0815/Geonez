using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights
{
	internal interface IShadowedLight : ILightSource
	{
		Matrix ShadowViewMatrix { get; }
		Matrix ShadowProjectionMatrix { get; }
		RenderTarget2D ShadowMap { get; }
	}
}
