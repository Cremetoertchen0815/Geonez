﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;

namespace Nez.GeonBit.ECS.Components.Graphics.Lighting;
public abstract class ShadowLight : GeonComponent, IUpdatable, IShadowedLight
{
    public int ShadowSourceID { get; private set; }
    protected bool _shadowMatricesModified = true;

    //Shadow Fields
    protected readonly float _aspectRatio;
    protected Vector3 _direction;
    protected float _nearDistance = 0.01f;
    protected float _farDistance = 100f;
    private Vector3 _forward = Vector3.Up;

    //Shadow Properties
    public RenderTarget2D ShadowMap { get; private set; }
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

    //Lighting Properties
    public Color Diffuse { get; set; }
    public Color Specular { get; set; }
	public uint ParamsVersion { get; set; }

	public Vector3 Position => Entity?.Node?.Position ?? Vector3.Zero;

	public ShadowLight(int id, Point shadowMapResolution = default)
    {
        ShadowSourceID = id;
        if (shadowMapResolution == default) return;
        _aspectRatio = (float)shadowMapResolution.X / shadowMapResolution.Y;
        ShadowMap = new RenderTarget2D(Core.GraphicsDevice, shadowMapResolution.X, shadowMapResolution.Y, false, SurfaceFormat.Single, DepthFormat.Depth24);
    }

    public override void OnAddedToEntity()
    {
        Entity.Scene.Lighting.AddLight(this);

	}

    public override void OnRemovedFromEntity() => Remove();

	public void Remove() => Entity.Scene.Lighting.RemoveLight(this);

	internal abstract void CalculateMatrix();

    public void Update()
    {
        if (_shadowMatricesModified) CalculateMatrix();
    }


	/// <summary>
	/// Update light transformations.
	/// </summary>
	/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
	public void UpdateTransforms(ref Matrix worldTransformations)
    {
		// if didn't really change skip
		// break transformation into components
		worldTransformations.Decompose(out var scale, out var rotation, out var position);

        // set world position. this will also recalc bounding sphere and update lights manager, if needed.
        Entity.Node.Position = position;
    }

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
