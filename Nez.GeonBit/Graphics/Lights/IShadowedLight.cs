using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights;

public interface IShadowedLight : ILightSource
{
    int ShadowSourceID { get; }
    Matrix ShadowViewMatrix { get; }
    Matrix ShadowProjectionMatrix { get; }
    RenderTarget2D ShadowMap { get; }
    Texture2D ShadowStencil { get; }
}