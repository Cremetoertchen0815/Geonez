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
// Particles system emmiter.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System.Collections.Generic;

namespace Nez.GeonBit.Particles;

/// <summary>
///     All the basic properties of a particle type the particle system may emit.
/// </summary>
public struct ParticleType
{
    /// <summary>
    ///     The particle GameObject (we emit clone of these objects).
    /// </summary>
    public GeonEntity ParticlePrototype { get; }

    /// <summary>
    ///     How often to spawn particles (value range should be 0f - 1f).
    ///     In every spawn event if the Frequency >= Random(0f, 1f), new particles will emit.
    /// </summary>
    public float Frequency { get; }

    /// <summary>
    ///     Min particles amount to create every spawn.
    /// </summary>
    public uint MinParticlesPerSpawn { get; }

    /// <summary>
    ///     Max particles amount to create every spawn.
    /// </summary>
    public uint MaxParticlesPerSpawn { get; }

    /// <summary>
    ///     How much to change frequency over time.
    ///     For example, if value is -0.5, will decrease Frequency by 0.5f over the span time of 1 second.
    /// </summary>
    public float FrequencyChange { get; }

    /// <summary>
    ///     Get frequency with FrequencyChange applied.
    /// </summary>
    /// <param name="timeAlive">For how long the particle system was alive.</param>
    /// <returns>Actual frequency for current time.</returns>
    public float GetFrequency(float timeAlive)
    {
        return Frequency + FrequencyChange * timeAlive;
    }

    /// <summary>
    ///     Create the particle type.
    /// </summary>
    /// <param name="particle">Particle object prototype.</param>
    /// <param name="frequency">Spawn frequency.</param>
    /// <param name="minCountPerSpawn">How many min particles to spawn every time.</param>
    /// <param name="maxCountPerSpawn">How many max particles to spawn every time.</param>
    /// <param name="frequencyChange">Change frequency over time.</param>
    public ParticleType(GeonEntity particle, float frequency = 0.01f, uint minCountPerSpawn = 1,
        uint maxCountPerSpawn = 1, float frequencyChange = 0f)
    {
        ParticlePrototype = particle.Clone();
        Frequency = frequency;
        MinParticlesPerSpawn = minCountPerSpawn;
        MaxParticlesPerSpawn = maxCountPerSpawn;
        FrequencyChange = frequencyChange;
    }

    /// <summary>
    ///     Clone particle type.
    /// </summary>
    /// <returns>Cloned particle type.</returns>
    public ParticleType Clone()
    {
        return new ParticleType(ParticlePrototype, Frequency, MinParticlesPerSpawn, MaxParticlesPerSpawn,
            FrequencyChange);
    }
}

/// <summary>
///     Particle system component that emit predefined particles.
/// </summary>
public class ParticleSystem : GeonComponent, IUpdatable
{
    // list of particle types
    private readonly List<ParticleType> _particles;

    // for random values
    private readonly System.Random _random;

    // for how long this particle system exists
    private float _timeAlive;

    // time until next interval
    private float _timeForNextInterval;

    /// <summary>
    ///     If true, will add all particles to root scene node.
    ///     This is useful for when the particle system moves (and you want it to affect spawning position), but you
    ///     don't want the movement to move existing particles, only change spawning point.
    /// </summary>
    public bool AddParticlesToRoot;

    /// <summary>
    ///     If true and tile-to-live expires, will also destroy parent Game Object.
    /// </summary>
    public bool DestroyParentWhenExpired;

    /// <summary>
    ///     Spawn events intervals. If set, will only spawn particles between these intervals.
    /// </summary>
    public float Interval;

    /// <summary>
    ///     Speed factor (affect the particle system spawn rates).
    /// </summary>
    public float SpawningSpeedFactor = 1f;

    /// <summary>
    ///     If set, will destroy self once time to live expires.
    /// </summary>
    public float TimeToLive;

    /// <summary>
    ///     Create the new particles system.
    /// </summary>
    public ParticleSystem()
    {
        _particles = new List<ParticleType>();
        _random = new System.Random();
    }

    /// <summary>
    ///     Create the new particles system with a base particle type.
    /// </summary>
    /// <param name="type">First particles type in this system.</param>
    public ParticleSystem(ParticleType type) : this()
    {
        AddParticleType(type);
    }

    /// <summary>
    ///     Called every const X seconds.
    /// </summary>
    public void Update()
    {
        // get time factor
        var timeFactor = Time.DeltaTime * SpawningSpeedFactor;

        // increase time alive
        _timeAlive += timeFactor;

        // check if expired
        if (TimeToLive != 0f && _timeAlive > TimeToLive)
        {
            // destroy parent if needed (note: if destroying parent we don't need to destroy self as well)
            if (DestroyParentWhenExpired && !Entity.IsDestroyed)
            {
                Entity.Destroy();
                return;
            }

            // destroy self
            Destroy();
            return;
        }

        // check if there's intervals to wait
        if (_timeForNextInterval > 0f)
        {
            _timeForNextInterval -= timeFactor;
            return;
        }

        _timeForNextInterval = Interval;

        // iterate over particle types and emit them
        foreach (var particleType in _particles)
        {
            // get current spawn frequency
            var frequency = particleType.GetFrequency(_timeAlive);

            // negative? skip
            if (frequency <= 0f) continue;

            // check if should spawn particles
            if (frequency >= Random.NextFloat())
            {
                // rand quantity to spawn
                var toSpawn = (uint)Random.Range((int)particleType.MinParticlesPerSpawn,
                    (int)particleType.MaxParticlesPerSpawn);

                // spawn particles
                for (var i = 0; i < toSpawn; ++i)
                {
                    // create new particle and add to self game object
                    var newPart = particleType.ParticlePrototype.Clone().Node;
                    newPart.Parent = Node;

                    // if need to add particles to root
                    if (AddParticlesToRoot)
                    {
                        var newNode = newPart.Entity.Node;
                        var position = newNode.WorldPosition;
                        newPart.Parent = null;
                        newNode.Position = position;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Add particle type to this particles system.
    /// </summary>
    /// <param name="type">Particle type to add.</param>
    public void AddParticleType(ParticleType type)
    {
        _particles.Add(type);
    }

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component Clone()
    {
        var ret = new ParticleSystem
        {
            TimeToLive = TimeToLive,
            DestroyParentWhenExpired = DestroyParentWhenExpired,
            Interval = Interval,
            SpawningSpeedFactor = SpawningSpeedFactor,
            AddParticlesToRoot = AddParticlesToRoot
        };
        foreach (var particleType in _particles) ret._particles.Add(particleType.Clone());
        return ret;
    }
}