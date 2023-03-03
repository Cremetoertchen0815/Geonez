using Microsoft.Xna.Framework;

namespace Nez.GeonBit.ECS.Components.Graphics.Lighting;
public class PrimarySpotLight : PrimaryLightSource
{
    public PrimarySpotLight(int id) : base(id)
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
        ShadowView = Matrix.CreateLookAt(Entity.Node.Position, Entity.Node.Position + Direction, Vector3.Up);
        ShadowProjection = Matrix.CreatePerspectiveFieldOfView(FOV, _aspectRatio, NearDistance, FarDistance);
    }
}
