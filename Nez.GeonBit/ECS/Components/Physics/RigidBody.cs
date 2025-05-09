﻿#region LICENSE

//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------

#endregion

#region File Description

//-----------------------------------------------------------------------------
// Rigid body component.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Nez.GeonBit.Physics;
using Nez.GeonBit.Physics.CollisionShapes;

namespace Nez.GeonBit;

/// <summary>
///     A rigid body component.
/// </summary>
public class RigidBody : BasePhysicsComponent, IUpdatable
{
    // are we currently in physics world?
    private readonly bool _isInWorld = false;

    // the core rigid body
    private Physics.RigidBody _body;

    // body inertia
    private float _intertia;

    // body mass
    private float _mass;

    /// <summary>
    ///     The shape used for this physical body.
    /// </summary>
    private ICollisionShape _shape;

    /// <summary>
    ///     Optional constant velocity to set for this physical body.
    /// </summary>
    public Vector3? ConstAngularVelocity;

    /// <summary>
    ///     Optional constant force to set for this physical body.
    /// </summary>
    public Vector3? ConstForce;

    /// <summary>
    ///     Optional constant angular force to set for this physical body.
    /// </summary>
    public Vector3? ConstTorqueForce;

    /// <summary>
    ///     Optional constant velocity to set for this physical body.
    /// </summary>
    public Vector3? ConstVelocity;

    /// <summary>
    ///     Optional game object to force update whenever this body updates transformations.
    ///     Useful if you want to attach a camera to a gameobject that is affected by this body and want to prevent
    ///     "tearing" due to physics / nodes update times.
    /// </summary>
    public Node SyncUpdateWith;

    /// <summary>
    ///     Create the physical body.
    /// </summary>
    /// <param name="shapeInfo">Body shape info.</param>
    /// <param name="mass">Body mass (0 for static).</param>
    /// <param name="inertia">Body inertia (0 for static).</param>
    /// <param name="friction">Body friction.</param>
    public RigidBody(IBodyShapeInfo shapeInfo, float mass = 0f, float inertia = 0f, float friction = 1f)
    {
        CreateBody(shapeInfo.CreateShape(), mass, inertia, friction);
    }

    /// <summary>
    ///     Create the physical body from shape instance.
    /// </summary>
    /// <param name="shape">Physical shape to use.</param>
    /// <param name="mass">Body mass (0 for static).</param>
    /// <param name="inertia">Body inertia (0 for static).</param>
    /// <param name="friction">Body friction.</param>
    public RigidBody(ICollisionShape shape, float mass = 0f, float inertia = 0f, float friction = 1f)
    {
        CreateBody(shape, mass, inertia, friction);
    }

    /// <summary>
    ///     The physical body in the core layer.
    /// </summary>
    internal override BasicPhysicalBody _PhysicalBody => _body;

    /// <summary>
    ///     Return true if you want this physical body to take over node transformations.
    /// </summary>
    protected override bool TakeOverNodeTransformations => true;

    /// <summary>
    ///     Get / set body mass.
    /// </summary>
    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value;
            _body.SetMassAndInertia(_mass, _intertia);
        }
    }

    /// <summary>
    ///     Get / set body inertia.
    /// </summary>
    public float Inertia
    {
        get => _intertia;
        set
        {
            _intertia = value;
            _body.SetMassAndInertia(_mass, _intertia);
        }
    }

    /// <summary>
    ///     Return if the physical body was updated and currently need to update the scene node.
    /// </summary>
    private bool NeedToUpdataNode => _body.IsActive && _body.IsDirty;

    /// <summary>
    ///     Get / set linear damping.
    /// </summary>
    public float LinearDamping
    {
        get => _body.LinearDamping;
        set => _body.LinearDamping = value;
    }

    /// <summary>
    ///     Get / set angular damping.
    /// </summary>
    public float AngularDamping
    {
        get => _body.AngularDamping;
        set => _body.AngularDamping = value;
    }

    /// <summary>
    ///     Get / set current body linear velocity.
    /// </summary>
    public Vector3 LinearVelocity
    {
        get => _body.LinearVelocity;
        set => _body.LinearVelocity = value;
    }

    /// <summary>
    ///     Get / set linear factor.
    /// </summary>
    public Vector3 LinearFactor
    {
        get => _body.LinearFactor;
        set => _body.LinearFactor = value;
    }

    /// <summary>
    ///     Get / set current body angular velocity.
    /// </summary>
    public Vector3 AngularVelocity
    {
        get => _body.AngularVelocity;
        set => _body.AngularVelocity = value;
    }

    /// <summary>
    ///     Get / set angular factor.
    /// </summary>
    public Vector3 AngularFactor
    {
        get => _body.AngularFactor;
        set => _body.AngularFactor = value;
    }

    /// <summary>
    ///     Set / get body gravity (if undefined, will use world default).
    /// </summary>
    public Vector3? Gravity
    {
        get => _body.Gravity;
        set => _body.Gravity = value;
    }

    /// <summary>
    ///     Get the bounding box of the physical body.
    /// </summary>
    public BoundingBox BoundingBox => _body.BoundingBox;

    /// <summary>
    ///     Called every frame in the Update() loop.
    ///     Note: this is called only if GameObject is enabled.
    /// </summary>
    public void Update()
    {
        // set const velocity
        if (ConstVelocity != null) _body.LinearVelocity = ConstVelocity.Value;
        // set const angular velocity
        if (ConstAngularVelocity != null) _body.AngularVelocity = ConstAngularVelocity.Value;
        // set const force
        if (ConstForce != null) _body.ApplyForce(ConstForce.Value);
        // set const torque force
        if (ConstTorqueForce != null) _body.ApplyTorque(ConstTorqueForce.Value);
        if (NeedToUpdataNode) UpdateNodeTransforms();
    }

    /// <summary>
    ///     Create the actual collision body.
    /// </summary>
    /// <param name="shape">Collision shape.</param>
    /// <param name="mass">Body mass.</param>
    /// <param name="inertia">Body inertia.</param>
    /// <param name="friction">Body friction.</param>
    private void CreateBody(ICollisionShape shape, float mass, float inertia, float friction)
    {
        // store params and create the body
        _mass = mass;
        _intertia = inertia;
        _body = new Physics.RigidBody(shape, mass, inertia)
        {
            Friction = friction
        };
        _shape = shape;

        // set self as attached data (needed for collision events)
        _body.EcsComponent = this;
    }

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component Clone()
    {
        // create cloned component to return
        var ret = (RigidBody)CopyBasics(new RigidBody(_shape.Clone(), Mass, Inertia, _body.Friction));

        // copy current state
        ret._body.CopyConditionFrom(_body);
        ret.Gravity = Gravity;
        ret.ConstForce = ConstForce;
        ret.ConstVelocity = ConstVelocity;
        ret.ConstTorqueForce = ConstTorqueForce;
        ret.ConstAngularVelocity = ConstAngularVelocity;

        // return the cloned body
        return ret;
    }

    /// <summary>
    ///     Update scene node transformations.
    /// </summary>
    internal override void UpdateNodeTransforms()
    {
        // update transforms
        var newTrans = _body.WorldTransform;
        Node.Root.SetWorldTransforms(ref newTrans);

        // if have object to sync update with, update the object as well
        if (SyncUpdateWith != null)
        {
            Node.Root.ForceFullUpdate(false);
            Node.Root.UpdateTransformations(true);
            SyncUpdateWith.UpdateTransformations(true);
        }
    }

    /// <summary>
    ///     Set damping.
    /// </summary>
    /// <param name="linear">Linear damping.</param>
    /// <param name="angular">Angular damping.</param>
    public void SetDamping(float linear, float angular)
    {
        _body.SetDamping(linear, angular);
    }

    /// <summary>
    ///     Clear all forces from the body.
    /// </summary>
    /// <param name="clearVelocity">If true, will also clear velocity.</param>
    public void ClearForces(bool clearVelocity = true)
    {
        _body.ClearForces(clearVelocity);
    }

    /// <summary>
    ///     Clear velocity from the body.
    /// </summary>
    public void ClearVelocity()
    {
        _body.ClearVelocity();
    }

    /// <summary>
    ///     Apply force on the physical body, from its center.
    /// </summary>
    /// <param name="force">Force vector to apply.</param>
    public void ApplyForce(Vector3 force)
    {
        _body.ApplyForce(force);
    }

    /// <summary>
    ///     Apply force on the physical body, from given position.
    /// </summary>
    /// <param name="force">Force vector to apply.</param>
    /// <param name="from">Force source position.</param>
    public void ApplyForce(Vector3 force, Vector3 from)
    {
        _body.ApplyForce(force, from);
    }

    /// <summary>
    ///     Apply force on the physical body, from its center.
    /// </summary>
    /// <param name="impulse">Impulse vector to apply.</param>
    public void ApplyImpulse(Vector3 impulse)
    {
        _body.ApplyImpulse(impulse);
    }

    /// <summary>
    ///     Apply impulse on the physical body, from given position.
    /// </summary>
    /// <param name="impulse">Impulse vector to apply.</param>
    /// <param name="from">Impulse source position.</param>
    public void ApplyImpulse(Vector3 impulse, Vector3 from)
    {
        _body.ApplyImpulse(impulse, from);
    }

    /// <summary>
    ///     Apply torque force on the body.
    /// </summary>
    /// <param name="torque">Torque force to apply.</param>
    /// <param name="asImpulse">If true, will apply torque as an impulse.</param>
    public void ApplyTorque(Vector3 torque, bool asImpulse = false)
    {
        _body.ApplyTorque(torque, asImpulse);
    }

    /// <summary>
    ///     Force the scene node to calculate world transformations and copy them into the rigid body transformations.
    ///     This will make the scene node world transform override the current physical body state.
    /// </summary>
    /// <param name="clearForces">If true, will also clear all forces and velocity currently applied on body.</param>
    public void CopyNodeWorldMatrix(bool clearForces = true)
    {
        // clear forces (if needed)
        if (clearForces) _body.ClearForces();

        // note: we can't just use SceneNode.WorldTransformations because its calculated differently because there's a physical body attached (ourselves..)
        WorldTransform = Node.BuildTransformationsMatrix() *
                         (Node.Parent != null ? Node.Parent.WorldTransformations : Matrix.Identity);
    }

    /// <summary>
    ///     Called when this component is effectively added to scene, eg when added
    ///     to a GameObject currently in scene or when its GameObject is added to scene.
    /// </summary>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        if (Position == default) Position = Node.Position;

        // add to physics world
        if (!_isInWorld) UpdateNodeTransforms();
    }
}