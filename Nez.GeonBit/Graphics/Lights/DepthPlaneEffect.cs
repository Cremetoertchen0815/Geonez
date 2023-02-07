using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit.Graphics.Lights;
public class DepthPlaneEffect : Effect, IEffectMatrices
{
    public DepthPlaneEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, Core.Content.Load<byte[]>("engine/fx/geon/depth_plane"))
        => _paramWVP = Parameters["LightViewProjection"];

    //Fields
    private Matrix _world = Matrix.Identity;
    private Matrix _view = Matrix.Identity;
    private Matrix _projection = Matrix.Identity;

    //Effect parameters
    private EffectParameter _paramWVP;

    public Matrix World
    {
        get => _world;
        set
        {
            _world = value;
            _paramWVP.SetValue(_world * _view * _projection);
        }
    }
    public Matrix View
    {
        get => _view;
        set
        {
            _view = value;
            _paramWVP.SetValue(_world * _view * _projection);
        }
    }
    public Matrix Projection
    {
        get => _projection;
        set
        {
            _projection = value;
            _paramWVP.SetValue(_world * _view * _projection);
        }
    }
}
