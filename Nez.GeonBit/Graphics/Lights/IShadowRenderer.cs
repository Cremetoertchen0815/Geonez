using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights
{
	public interface IShadowCaster
	{
		Matrix ShadowViewMatrix { get; }
		Matrix ShadowProjectionMatrix { get; }
		RenderTarget2D ShadowMap { get; }
	}
}
