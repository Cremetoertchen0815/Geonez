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
// A scale animator that change the scaling of the scene node.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Particles.Animators;

/// <summary>
///     Scale animator (change scene node scaling).
/// </summary>
public class ScaleAnimator : BaseAnimator
{
    private readonly float _endScaleJitter;

    // store jitters, for the purpose of cloning
    private readonly float _scaleTimeJitter;
    private readonly float _startScaleJitter;

    /// <summary>
    ///     Create the scale animator.
    /// </summary>
    /// <param name="properties">Basic animator properties.</param>
    /// <param name="fromScale">Starting scale.</param>
    /// <param name="toScale">Ending scale.</param>
    /// <param name="scaleTime">How long to transition from starting to ending scale.</param>
    /// <param name="scaleTimeJitter">If provided, will add random jitter to scaling time.</param>
    /// <param name="startScaleJitter">If provided, will add random jitter to scaling starting value.</param>
    /// ///
    /// <param name="endScaleJitter">If provided, will add random jitter to scaling ending value.</param>
    public ScaleAnimator(BaseAnimatorProperties properties, Vector3 fromScale, Vector3 toScale, float scaleTime,
        float scaleTimeJitter = 0f, float startScaleJitter = 0f, float endScaleJitter = 0f) : base(properties)
    {
        // set basic properties
        FromScale = fromScale;
        ToScale = toScale;
        ScalingTime = scaleTime;
        _endScaleJitter = endScaleJitter;
        _scaleTimeJitter = scaleTimeJitter;
        _startScaleJitter = startScaleJitter;
    }

    /// <summary>
    ///     Create the scale animator.
    /// </summary>
    /// <param name="properties">Basic animator properties.</param>
    /// <param name="fromScale">Starting scale.</param>
    /// <param name="toScale">Ending scale.</param>
    /// <param name="scaleTime">How long to transition from starting to ending scale.</param>
    /// <param name="scaleTimeJitter">If provided, will add random jitter to scaling time.</param>
    /// <param name="startScaleJitter">If provided, will add random jitter to scaling starting value.</param>
    /// ///
    /// <param name="endScaleJitter">If provided, will add random jitter to scaling ending value.</param>
    public ScaleAnimator(BaseAnimatorProperties properties, float fromScale, float toScale, float scaleTime,
        float scaleTimeJitter = 0f, float startScaleJitter = 0f, float endScaleJitter = 0f) : base(properties)
    {
        // set basic properties
        FromScale = Vector3.One * fromScale;
        ToScale = Vector3.One * toScale;
        ScalingTime = scaleTime;
        _endScaleJitter = endScaleJitter;
        _scaleTimeJitter = scaleTimeJitter;
        _startScaleJitter = startScaleJitter;
    }

    /// <summary>
    ///     Starting scale.
    /// </summary>
    public Vector3 FromScale { get; private set; }

    /// <summary>
    ///     Ending scale.
    /// </summary>
    public Vector3 ToScale { get; private set; }

    /// <summary>
    ///     How long it takes to get from starting scale to ending scale.
    /// </summary>
    public float ScalingTime { get; private set; }

    /// <summary>
    ///     Get if this animator is done, unrelated to time to live (for example, if transition is complete).
    /// </summary>
    protected override bool IsDone => TimeAnimated >= ScalingTime;

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component Clone()
    {
        // note: unlike in other clones that try to copy the entity perfectly, in this clone we create new with jitters
        // so we'll still have the random factor applied on the cloned entity.
        return new ScaleAnimator(BaseProperties, FromScale, ToScale, ScalingTime,
            _scaleTimeJitter, _startScaleJitter, _endScaleJitter);
    }

    /// <summary>
    ///     Called when GameObject spawns.
    /// </summary>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        if (_scaleTimeJitter != 0f) ScalingTime += Random.NextFloat() * _scaleTimeJitter;
        if (_startScaleJitter != 0f) FromScale += Vector3.One * (Random.NextFloat() * _startScaleJitter);
        if (_endScaleJitter != 0f) ToScale += Vector3.One * (Random.NextFloat() * _endScaleJitter);
        Node.Scale = FromScale;
    }

    /// <summary>
    ///     The animator implementation.
    /// </summary>
    protected override void DoAnimation(float speedFactor)
    {
        // get current scaling step, and if done, skip
        var position = AnimatorUtils.CalcTransitionPercent(TimeAnimated, ScalingTime);

        // calc current scale value
        Node.Scale = FromScale * (1f - position) + ToScale * position;
    }
}