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
        var mat = new LitMaterial(PCFQuality.MidPCF)
        {
            Alpha = 1f,
            DiffuseColor = Nez.Random.NextColor(),
            FogEnabled = false,
            ShadowsEnabled = true,
            ShadowBias = 0.001f,
            SpecularColor = Color.Black,
            SpecularPower = 40f,
            TextureEnabled = false
        };

        //Generate renderer
        var rend = new ShapeRenderer(ShapeMeshes.Cube)
        {
            CastsShadows = true,
            PrimaryLight = SHADOW_LEVEL,
            RenderingQueue = RenderingQueue.Solid
        };
        rend.SetMaterial(mat);
        Entity.AddComponentAsChild(rend);
        rend.Node.Scale = new Vector3(2.5f);

        //Add rigid body
        var rb = new RigidBody(new BoxInfo(new Vector3(5f)), 20f);
        Entity.AddComponent(rb);
    }
}
