using BV.Game.Components.Debug;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.ECS.Renderers;
using Nez.GeonBit.Materials;
using Nez.Sprites;

namespace GeonGraphicsTest;
public class GraphicsTestScene : GeonScene
{

    private const int MAIN_SHADOW_PLANE = 0;

    public override void Initialize()
    {
        base.Initialize();

        ClearColor = Color.Black;
        GeonDefaultRenderer.ActiveLightsManager.ShadowsEnabed = true;
        AddRenderer(new GeonShadowMapRenderer(0));
        AddRenderer(new GeonDefaultRenderer(1, this));
        AddRenderer(new DefaultRenderer(2));

        var lightEntity = CreateGeonEntity("MainLight", new Vector3(0, 10, -4f));
        var spotLight = lightEntity.AddComponent(new PrimarySpotLight(MAIN_SHADOW_PLANE, new Point(1024)) { Direction = Vector3.Down, Forward = Vector3.Backward, FarDistance = 40f, NearDistance = 5f });
        lightEntity.AddComponent(new ShapeRenderer(ShapeMeshes.SphereLowPoly));


        CreateEntity("ShadowMapLooki", Screen.Center + new Vector2(0, 200f)).AddComponent(new Nez.Sprites.SpriteRenderer(spotLight.ShadowMap) { Size = new Vector2(1024f) * 0.25f});

        var backdrop = CreateGeonEntity("backdrop").AddComponent(new SkyBox() { RenderingQueue = RenderingQueue.BackgroundNoCull });

        var lightSrc = CreateGeonEntity("light", new Vector3(-3f, 0f, -8f), NodeType.Simple);
        lightSrc.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Sphere) { CastsShadows = true, PrimaryLight = 0,
                                                                            MaterialOverride = new MaterialOverrides() { DiffuseColor = Color.Lime }});

        var lightDst = CreateGeonEntity("cube", new Vector3(3f, -1.5f, -6f), NodeType.Simple);
        lightDst.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Sphere) {
            CastsShadows = true,
            PrimaryLight = 0, 
        });
        lightDst.Node.Rotation = new Vector3(120f, 5f, 10f);
        lightDst.Node.Tween("Position", new Vector3(3f, -50f, -6f), 2f).SetLoops(Nez.Tweens.LoopType.PingPong, -1).Start();


        var projectionPlane = CreateGeonEntity("projPlane", new Vector3(0f, -10f, -4f), NodeType.Simple);
        projectionPlane.Node.Scale = 10f * Vector3.One;
        projectionPlane.Node.RotationX = -MathHelper.PiOver2;
        projectionPlane.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Plane)
        {
            CastsShadows = true,
            PrimaryLight = 0,
            ShadowCasterRasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState.CullNone
        });

        AddSceneComponent(new DebugCamMover());
    }
}
