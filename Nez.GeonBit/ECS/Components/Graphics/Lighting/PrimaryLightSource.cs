﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Lights;
using System;

namespace Nez.GeonBit.ECS.Components.Graphics.Lighting;
public abstract class PrimaryLightSource : GeonComponent, IUpdatable, IShadowedLight
{
    public int LightSourceId { get; private set; }
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

	public void Remove() => throw new NotImplementedException();
	public void UpdateTransforms(ref Matrix worldTransformations) => throw new NotImplementedException();
}
