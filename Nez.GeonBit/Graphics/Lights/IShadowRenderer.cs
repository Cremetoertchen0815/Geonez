using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Graphics.Lights
{
	public interface IShadowRenderer
	{
		bool CastsShadow { get; }
		void RenderShadows(Matrix worldTransform);
	}
}
