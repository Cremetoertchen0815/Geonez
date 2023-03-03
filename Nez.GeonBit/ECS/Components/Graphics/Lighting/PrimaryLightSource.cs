using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nez.GeonBit;
public abstract class PrimaryLightSource : GeonComponent, IUpdatable
{
    public int LightSourceId { get; private set; }
    protected bool _shadowMatricesModified = true;

    //Shadow Fields
    protected readonly float _aspectRatio;
    protected Vector3 _direction;
    protected float _nearDistance;
    protected float _farDistance;

    //Shadow Properties
    public RenderTarget2D ShadowMap { get; private set; }
    public Matrix ShadowView { get; protected set; }
    public Matrix ShadowProjection { get; protected set; }
    public Vector3 Direction
    {
        get => _direction; 
        set
        {
            _direction = value;
            _shadowMatricesModified = true;
        }
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

    //Lighting Properties
    public Color DiffuseColor { get; set; }
    public Color SpecularColor { get; set; }


    public PrimaryLightSource(int id, Point shadowMapResolution = default)
    {
        LightSourceId = id;
        if (shadowMapResolution == default) return;
        _aspectRatio = (float)shadowMapResolution.X / shadowMapResolution.Y;
        ShadowMap = new RenderTarget2D(Core.GraphicsDevice, shadowMapResolution.X, shadowMapResolution.Y, false, SurfaceFormat.Single, DepthFormat.Depth24);
    }

    public override void OnAddedToEntity()
    {
        if (!Entity.Scene.LightSourcesPrimary.TryAdd(LightSourceId, this)) throw new ArgumentException("Light source with identical ID already added!");
    }

    public override void OnRemovedFromEntity()
    {
        if (Entity.Scene.LightSourcesPrimary.ContainsKey(LightSourceId)) Entity.Scene.LightSourcesPrimary.Remove(LightSourceId);
    }

    internal abstract void CalculateMatrix();

    public void Update()
    {
        if (_shadowMatricesModified) CalculateMatrix();
    }
}
