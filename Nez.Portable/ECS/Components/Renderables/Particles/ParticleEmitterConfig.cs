﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Particles;

[Serializable]
public class ParticleEmitterConfig : IDisposable
{
    public float Angle, AngleVariance;
    public Blend BlendFuncDestination;

    public Blend BlendFuncSource;
    public float Duration;
    public float EmissionRate;
    public ParticleEmitterType EmitterType;
    public Color FinishColor = Color.White, FinishColorVariance = Color.Transparent;
    public float FinishParticleSize, FinishParticleSizeVariance;
    public Vector2 Gravity;
    public uint MaxParticles;


    // Particle ivars only used when a maxRadius value is provided. These values are used for
    // the special purpose of creating the spinning portal emitter
    // Max radius at which particles are drawn when rotating
    public float MaxRadius;

    // Variance of the maxRadius
    public float MaxRadiusVariance;

    // Radius from source below which a particle dies
    public float MinRadius;

    // Variance of the minRadius
    public float MinRadiusVariance;
    public float ParticleLifespan, ParticleLifespanVariance;
    public float RadialAcceleration, RadialAccelVariance;

    // Numeber of degress to rotate a particle around the source pos per second
    public float RotatePerSecond;

    // Variance in degrees for rotatePerSecond
    public float RotatePerSecondVariance;
    public float RotationEnd, RotationEndVariance;

    public float RotationStart, RotationStartVariance;

    /// <summary>
    ///     If true, particles will simulate in world space. ie when the parent Transform moves it will have no effect on any
    ///     already active Particles.
    /// </summary>
    public bool SimulateInWorldSpace = true;

    /// <summary>
    ///     sourcePosition is read in but internally it is not used. The ParticleEmitter.localPosition is what the emitter will
    ///     use for positioning
    /// </summary>
    public Vector2 SourcePosition;

    public Vector2 SourcePositionVariance;
    public float Speed, SpeedVariance;
    public Sprite Sprite;
    public Color StartColor = Color.White, StartColorVariance = Color.Transparent;
    public float StartParticleSize, StartParticleSizeVariance;
    public float TangentialAcceleration, TangentialAccelVariance;


    void IDisposable.Dispose()
    {
        if (Sprite != null)
        {
            Sprite.Texture2D.Dispose();
            Sprite.Texture2D = null;
            Sprite = null;
        }
    }
}