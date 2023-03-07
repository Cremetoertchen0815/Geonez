using Nez;

namespace GeonGraphicsTest;
public class TestGame : Core
{
    protected override void Initialize()
    {
        base.Initialize();

        Scene = new GraphicsTestScene();
    }
}
