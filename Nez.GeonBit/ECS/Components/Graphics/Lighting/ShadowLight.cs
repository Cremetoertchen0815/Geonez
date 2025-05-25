using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Lights;

namespace Nez.GeonBit;

public abstract class ShadowLight : GeonComponent, IUpdatable, IShadowedLight
{
    //Shadow Fields
    protected readonly float _aspectRatio;
    private Vector3 _diffuse = Vector3.One;
    protected Vector3 _direction;
    protected float _farDistance = 100f;
    private Vector3 _forward = Vector3.Up;
    protected float _nearDistance = 0.01f;
    private RenderTarget2D _shadowMap;
    protected bool _shadowMatricesModified = true;
    private Vector3 _specular = Vector3.One;

    public ShadowLight(int id, Point shadowMapResolution = default)
    {
        ShadowSourceID = id;
        if (shadowMapResolution == default) shadowMapResolution = LightsManager.DefaultShadowMapResolution;
        _aspectRatio = (float)shadowMapResolution.X / shadowMapResolution.Y;
        ShadowMap = new RenderTarget2D(Core.GraphicsDevice, shadowMapResolution.X, shadowMapResolution.Y, false,
            SurfaceFormat.Single, DepthFormat.Depth24);
    }

    public float NearDistance
    {
        get => _nearDistance;
        set
        {
            _nearDistance = value;
            _shadowMatricesModified = true;
        }
    }

    public float FarDistance
    {
        get => _farDistance;
        set
        {
            _farDistance = value;
            _shadowMatricesModified = true;
        }
    }

    public Vector3 Forward
    {
        get => _forward;
        set
        {
            _forward = value;
            _shadowMatricesModified = true;
        }
    }

    public int ShadowSourceID { get; }

    //Shadow Properties
    public RenderTarget2D ShadowMap
    {
        get => _shadowMap;
        private set
        {
            _shadowMap = value;
            ParamsVersion++;
        }
    }

    public Matrix ShadowViewMatrix { get; protected set; }
    public Matrix ShadowProjectionMatrix { get; protected set; }

    public Vector3? Direction
    {
        get => _direction;
        set
        {
            _direction = value ?? Vector3.Zero;
            _shadowMatricesModified = true;
        }
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

    public Vector3 Position => Node?.WorldPosition ?? Vector3.Zero;

    public abstract Texture2D ShadowStencil { get; set; }

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
        worldTransformations.Decompose(out _, out _, out var position);

        // set world position. this will also recalc bounding sphere and update lights manager, if needed.
        Node.Position = position;
    }

    public void Update()
    {
        if (_shadowMatricesModified) CalculateMatrix();
    }

    public override void OnAddedToEntity()
    {
        Entity.Scene.Lighting.AddLight(this);
    }

    public override void OnRemovedFromEntity()
    {
        Remove();
    }

    internal abstract void CalculateMatrix();

    ///// <summary>
    ///// Recalculate light bounding sphere after transformations or radius change.
    ///// </summary>
    ///// <param name="updateInLightsManager">If true, will also update light position in lights manager.</param>
    //public virtual void RecalcBoundingSphere(bool updateInLightsManager = true)
    //{
    //	// calc light bounding sphere
    //	BoundingSphere = new BoundingSphere(Position, _range);

    //	// notify manager on update
    //	if (updateInLightsManager && LightsManager != null)
    //	{
    //		LightsManager.UpdateLightTransform(this);
    //	}
    //}
}