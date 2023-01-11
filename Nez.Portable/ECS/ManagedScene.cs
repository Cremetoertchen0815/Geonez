using System;

namespace Nez
{
	/// <summary>
	/// Adding this attribute to a scene makes it detectable to the Scene Manager.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ManagedScene : Attribute
	{
		public int SceneNumber;
		public bool AcceptsArgument;

		public ManagedScene(int sceneNr, bool acceptsArgs = true)
		{
			SceneNumber = sceneNr;
			AcceptsArgument = acceptsArgs;
		}
	}
}
