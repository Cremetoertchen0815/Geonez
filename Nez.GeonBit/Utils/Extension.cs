using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Materials;
using Nez.Systems;

namespace Nez.GeonBit;

public static class Extension
{
    public static Model LoadModel(this NezContentManager c, string path)
    {
        return c.LoadModel(path, x => DefaultMaterialsFactory.GetDefaultMaterial(x));
    }
}