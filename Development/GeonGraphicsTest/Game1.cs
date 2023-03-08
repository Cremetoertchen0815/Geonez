using Nez;

namespace GeonGraphicsTest;
public class TestGame : Core
{
    protected override void Initialize()
    {
        base.Initialize();
        Screen.AASamples = 0;
        Scene = new GraphicsTestScene();
    }
}
