using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit;

public class ShadowSpotLight : ShadowLight
{
    private float _fov = MathHelper.PiOver2;

    public ShadowSpotLight(int id, Point shadowMapResolution = default) : base(id, shadowMapResolution)
    {
    }

    public float FOV
    {
        get => _fov;
        set
        {
            _fov = value;
            _shadowMatricesModified = true;
        }
    }

    public override Texture2D ShadowStencil { get; set; }

    internal override void CalculateMatrix()
    {
        ShadowViewMatrix = Matrix.CreateLookAt(Position, Position + Direction ?? Vector3.Zero, Forward);
        ShadowProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FOV, _aspectRatio, NearDistance, FarDistance);
    }
}