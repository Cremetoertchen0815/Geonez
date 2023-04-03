using Microsoft.Xna.Framework;
using Nez.GeonBit;
using Nez.GeonBit.Graphics.Misc;
using Nez.GeonBit.Materials;
using RigidBody = Nez.GeonBit.RigidBody;

namespace GeonGraphicsTest;
public class DebugCube : GeonComponent
{
    public const int SHADOW_LEVEL = 0;
    public override void OnAddedToEntity()
    {
        //Generate material
        var mat = new BasicLitMaterial(PCFQuality.MidPCF)
        {
            Alpha = 1f,
            DiffuseColor = Nez.Random.NextColor(),
            FogEnabled = false,
            ShadowsEnabled = true,
            ShadowBias = 0.0001f,
            SpecularColor = Color.Black,
            SpecularPower = 10f,
            TextureEnabled = false,
            NormalTexture = Entity.Scene.Content.LoadTexture("test_normal")
        };

        //Generate renderer
        var rend = new ShapeRenderer(ShapeMeshes.Cube)
        {
            CastsShadows = true,
            PrimaryLight = SHADOW_LEVEL,
            RenderingQueue = RenderingQueue.Solid,
            ShadowCasterRasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullClockwise
        };
        rend.SetMaterial(mat);
        Entity.AddComponentAsChild(rend);
        rend.Node.Scale = new Vector3(2.5f);

        //Add rigid body
        var rb = new RigidBody(new BoxInfo(new Vector3(5f)), 20f, 1f, 0f);
        rb.Restitution = 1f;
        Entity.AddComponent(rb);
    }
}
