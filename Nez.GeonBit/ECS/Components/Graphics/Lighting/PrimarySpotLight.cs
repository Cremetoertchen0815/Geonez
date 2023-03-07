using Microsoft.Xna.Framework;

namespace Nez.GeonBit;
public class PrimarySpotLight : PrimaryLightSource
{
    public PrimarySpotLight(int id, Point shadowMapResolution = default) : base(id, shadowMapResolution)
    {
    }

    private float _fov = MathHelper.PiOver2;

    public float FOV
    {
        get => _fov; 
        set
        {
            _fov = value;
            _shadowMatricesModified = true;
        }
    }

    internal override void CalculateMatrix()
    {
        ShadowView = Matrix.CreateLookAt(Entity.Node.Position, Entity.Node.Position + Direction, Forward);
        ShadowProjection = Matrix.CreatePerspectiveFieldOfView(FOV, _aspectRatio, NearDistance, FarDistance);
    }
}
