using Microsoft.Xna.Framework;
using Nez.GeonBit.Graphics.Lights;

namespace Nez.GeonBit;

public class DirectionalLight : GeonComponent, ILightSource
{
    private Vector3 _diffuse = Vector3.One;
    private Vector3 _direction;
    private Vector3 _specular = Vector3.One;

    public Vector3? Direction
    {
        get => _direction;
        set => _direction = value ?? Vector3.Zero;
    }

    //Lighting Properties
    public Vector3 Diffuse
    {
        get => _diffuse;
        set
        {
            _diffuse = value;
            ParamsVersion++;
        }
    }

    public Vector3 Specular
    {
        get => _specular;
        set
        {
            _specular = value;
            ParamsVersion++;
        }
    }

    public uint ParamsVersion { get; set; }

    public Vector3 Position => Vector3.Zero;

    public void Remove()
    {
        Entity.Scene.Lighting.RemoveLight(this);
    }


    /// <summary>
    ///     Update light transformations.
    /// </summary>
    /// <param name="worldTransformations">
    ///     World transformations to apply on this entity (this is what you should use to draw
    ///     this entity).
    /// </param>
    public void UpdateTransforms(ref Matrix worldTransformations)
    {
        // if didn't really change skip
        // break transformation into components
        worldTransformations.Decompose(out var scale, out var rotation, out var position);

        // set world position. this will also recalc bounding sphere and update lights manager, if needed.
        Entity.Node.Position = position;
    }

    public override void OnAddedToEntity()
    {
        Entity.Scene.Lighting.AddLight(this);
    }

    public override void OnRemovedFromEntity()
    {
        Remove();
    }
}