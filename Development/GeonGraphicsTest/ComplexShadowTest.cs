using BV.Game.Components.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.ECS.Components.Graphics.Lighting;
using Nez.GeonBit.ECS.Renderers;
using Nez.GeonBit.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeonGraphicsTest;
public class ComplexShadowTest : GeonScene
{
    public override void Initialize()
    {
        base.Initialize();

        AddRenderer(new GeonShadowMapRenderer(0));
        AddRenderer(new GeonDefaultRenderer(1, this));
        AddRenderer(new DefaultRenderer(2));
        
        Lighting.AmbientLight = Color.White * 0.25f;
        var world = AddSceneComponent(new Nez.GeonBit.Physics.PhysicsWorld());
        AddSceneComponent(new DebugCamMover());
        Camera.Node.Position = new Vector3(0, 20, 30);

        var lightEntity = CreateGeonEntity("MainLight", new Vector3(0, 50, -4f));
        var spotLight = lightEntity.AddComponent(new ShadowSpotLight(DebugCube.SHADOW_LEVEL, new Point(1024 * 4)) { Direction = Vector3.Down, Forward = Vector3.Backward, FarDistance = 60f, NearDistance = 5f, Diffuse = Color.White, Specular = Color.DarkGray });
        lightEntity.AddComponent(new ShapeRenderer(ShapeMeshes.SphereLowPoly));


        var backdrop = CreateGeonEntity("backdrop").AddComponent(new SkyBox() { RenderingQueue = RenderingQueue.BackgroundNoCull });
        var projectionPlane = CreateGeonEntity("projPlane", new Vector3(0f, 0f, 0f), NodeType.Simple);
        var projRend = projectionPlane.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Plane)
        {
            CastsShadows = true,
            PrimaryLight = DebugCube.SHADOW_LEVEL
        });
        projRend.Node.Scale = 100f * Vector3.One;
        projRend.Node.RotationX = -MathHelper.PiOver2;
        projRend.PrimaryLight = DebugCube.SHADOW_LEVEL;
        projRend.CastsShadows = false;
        projRend.SetMaterial(new BasicLitMaterial()
        {
            Alpha = 1f,
            ShadowsEnabled = true,
            SpecularColor = Color.Black
        });

        projectionPlane.AddComponent(new RigidBody(new EndlessPlaneInfo()));


        CreateEntity("ShadowMapLooki", Screen.Center + new Vector2(0, 200f)).AddComponent(new Nez.Sprites.SpriteRenderer(spotLight.ShadowMap) { Size = new Vector2(1024f) * 0.25f });


        for (int i = 0; i < 40; i++)
        {
            var entity = CreateGeonEntity("cube" + i, new Vector3(System.Random.Shared.NextSingle() * 20f - 10f, System.Random.Shared.NextSingle() * 20f + 5f, System.Random.Shared.NextSingle() * 20f - 10f));
            entity.AddComponent(new DebugCube());
        }

    }
}
