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
// A rotation animator that rotates the scene node.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Particles.Animators;

/// <summary>
///     Rotation animator (change rotation).
/// </summary>
public class RotationAnimator : BaseAnimator
{
    // store jitters, for cloning
    private readonly Vector3? _directionJitter;
    private readonly float _maxSpeed;
    private readonly float _minSpeed;

    /// <summary>
    ///     Create the rotation animator.
    /// </summary>
    /// <param name="properties">Base animator properties.</param>
    /// <param name="rotationDirection">Base rotation vector.</param>
    /// <param name="directionJitter">Rotation vector jitter.</param>
    /// <param name="minSpeed">Minimum rotation speed.</param>
    /// <param name="maxSpeed">Maximum rotation speed.</param>
    public RotationAnimator(BaseAnimatorProperties properties, Vector3 rotationDirection,
        Vector3? directionJitter = null, float minSpeed = 1f, float maxSpeed = 1f) : base(properties)
    {
        // set basic properties
        RotationDirection = rotationDirection;
        _directionJitter = directionJitter;
        _minSpeed = minSpeed;
        _maxSpeed = maxSpeed;
    }

    /// <summary>
    ///     Create the rotation animator.
    /// </summary>
    /// <param name="properties">Base animator properties.</param>
    /// <param name="rotationDirection">Base rotation direction vector.</param>
    /// <param name="speed">Rotation speed.</param>
    public RotationAnimator(BaseAnimatorProperties properties, Vector3 rotationDirection, float speed = 1f) :
        base(properties)
    {
        // set basic properties
        RotationDirection = rotationDirection;
        _directionJitter = null;
        _minSpeed = speed;
        _maxSpeed = speed;
    }

    /// <summary>
    ///     Create the rotation animator for random direction.
    /// </summary>
    /// <param name="properties">Base animator properties.</param>
    /// <param name="speed">Rotation speed.</param>
    /// <param name="speedJitter">Optional speed jiterring.</param>
    public RotationAnimator(BaseAnimatorProperties properties, float speed = 1f, float speedJitter = 0f) :
        base(properties)
    {
        // set basic properties
        RotationDirection = Vector3.Zero;
        _directionJitter = Vector3.One;
        _minSpeed = speed;
        _maxSpeed = speed + speedJitter;
    }

    /// <summary>
    ///     Rotation vector.
    /// </summary>
    public Vector3 RotationDirection { get; private set; }

    /// <summary>
    ///     Rotation speed.
    /// </summary>
    public float RotationSpeed { get; private set; }

    /// <summary>
    ///     Get if this animator is done, unrelated to time to live (for example, if transition is complete).
    /// </summary>
    protected override bool IsDone => false;

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component Clone()
    {
        // note: unlike in other clones that try to copy the entity perfectly, in this clone we create new with jitter
        // so we'll still have the random factor applied on the cloned entity.
        return new RotationAnimator(BaseProperties, RotationDirection,
            _directionJitter, _minSpeed, _maxSpeed);
    }

    /// <summary>
    ///     Called when GameObject spawns.
    /// </summary>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // add rotation jitter
        if (_directionJitter != null) RotationDirection = RandDirection(RotationDirection, _directionJitter.Value);

        // normalize rotation direction
        RotationDirection.Normalize();

        // random rotation speed
        RotationSpeed = _minSpeed == _maxSpeed ? _minSpeed : _minSpeed + Random.NextFloat() * (_maxSpeed - _minSpeed);

        // apply speed
        RotationDirection *= RotationSpeed;
    }

    /// <summary>
    ///     The animator implementation.
    /// </summary>
    protected override void DoAnimation(float speedFactor)
    {
        // rotate scene node
        Node.Rotation += RotationDirection * speedFactor;
    }
}