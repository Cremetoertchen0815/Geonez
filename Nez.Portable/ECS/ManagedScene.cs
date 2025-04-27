using System;

namespace Nez;

/// <summary>
///     Adding this attribute to a scene makes it detectable to the Scene Manager.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ManagedScene : Attribute
{
    public bool AcceptsArgument;
    public int SceneNumber;

    public ManagedScene(int sceneNr, bool acceptsArgs = true)
    {
        SceneNumber = sceneNr;
        AcceptsArgument = acceptsArgs;
    }
}