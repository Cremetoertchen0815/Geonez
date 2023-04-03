using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using RigidBody = Nez.GeonBit.RigidBody;

namespace GeonGraphicsTest;
public class DebugCube : GeonComponent
{
    private static TextureCube _cubeMap = GenCubeMap();

    public const int SHADOW_LEVEL = 0;
    public override void OnAddedToEntity()
    {
        //Generate material
        var mat = new RefractiveMaterial()
        {
            Alpha = 1f,
            DiffuseColor = Random.NextColor(),
            FogEnabled = false,
            SpecularColor = Color.Black,
            SpecularPower = 10f,
            TextureEnabled = false,
            EnvironmentMap = _cubeMap,
            EnvironmentSpecular = Color.Black,
            FresnelFactor = 3f,
            RefractionIndex = 2f,
            NormalTexture = Core.Content.LoadTexture("Normal")
        };

        //Generate renderer
        var rend = new ShapeRenderer(ShapeMeshes.SphereSmooth)
        {
            CastsShadows = true,
            PrimaryLight = SHADOW_LEVEL,
            RenderingQueue = RenderingQueue.Solid,
            ShadowCasterRasterizerState = RasterizerState.CullClockwise
        };
        rend.SetMaterial(mat);
        Entity.AddComponentAsChild(rend);
        rend.Node.Scale = new Vector3(2.5f);

        //Add rigid body
        var rb = new RigidBody(new BoxInfo(new Vector3(5f)), 20f, 1f, 0f);
        rb.Restitution = 1f;
        Entity.AddComponent(rb);
    }

    private static TextureCube GenCubeMap()
    {
        var negX = Core.Content.LoadTexture("cubemap/negx");
        var posX = Core.Content.LoadTexture("cubemap/posx");
        var negY = Core.Content.LoadTexture("cubemap/negy");
        var posY = Core.Content.LoadTexture("cubemap/posy");
        var negZ = Core.Content.LoadTexture("cubemap/negz");
        var posZ = Core.Content.LoadTexture("cubemap/posz");
        var cub = new TextureCube(Core.GraphicsDevice, 2048, true, SurfaceFormat.Color);
        var datastore = new Color[2048 * 2048];
        negX.GetData(datastore);
        cub.SetData(CubeMapFace.NegativeX, datastore);
        posX.GetData(datastore);
        cub.SetData(CubeMapFace.PositiveX, datastore);
        negY.GetData(datastore);
        cub.SetData(CubeMapFace.NegativeY, datastore);
        posY.GetData(datastore);
        cub.SetData(CubeMapFace.PositiveY, datastore);
        negZ.GetData(datastore);
        cub.SetData(CubeMapFace.NegativeZ, datastore);
        posZ.GetData(datastore);
        cub.SetData(CubeMapFace.PositiveZ, datastore);
        return cub;
    }
}
