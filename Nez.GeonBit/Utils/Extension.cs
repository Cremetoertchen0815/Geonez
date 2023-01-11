using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;

namespace Nez.GeonBit
{
	public static class Extension
	{
		public static Model LoadModel(this NezContentManager c, string path) => c.LoadModel(path, x => Materials.DefaultMaterialsFactory.GetDefaultMaterial(x));
	}
}
