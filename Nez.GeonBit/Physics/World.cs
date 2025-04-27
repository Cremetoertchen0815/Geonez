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
// Wrap and init the physics world.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System.Runtime.CompilerServices;
using BulletSharp;
using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Physics;

/// <summary>
///     GeonBit.Core.Physics implement physics related stuff.
/// </summary>
[CompilerGenerated]
internal class NamespaceDoc
{
}

/// <summary>
///     Data provided to physics collision callbacks.
/// </summary>
public struct CollisionData
{
    /// <summary>
    ///     Collision point.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    ///     Create the collision data.
    /// </summary>
    /// <param name="position">Collision point.</param>
    public CollisionData(Vector3 position)
    {
        Position = position;
    }
}

/// <summary>
///     Raycast results object.
/// </summary>
public struct RaycastResults
{
    /// <summary>
    ///     All the data of a single raycast result.
    /// </summary>
    public struct SingleResult
    {
        /// <summary>
        ///     Physical body we collided with.
        /// </summary>
        public BasePhysicsComponent CollisionBody;

        /// <summary>
        ///     Collision normal.
        /// </summary>
        public Vector3 CollisionNormal;

        /// <summary>
        ///     Collision position in world space.
        /// </summary>
        public Vector3 CollisionPoint;

        /// <summary>
        ///     Hit fraction.
        /// </summary>
        public double HitFraction;
    }

    /// <summary>
    ///     Did the raycast hit anything?
    /// </summary>
    public bool HasHit;

    /// <summary>
    ///     List of hit results.
    /// </summary>
    public SingleResult[] Collisions;

    /// <summary>
    ///     Get first / main result.
    ///     Careful not to call this without checking HasHit first.
    /// </summary>
    public SingleResult Collision => Collisions[0];
}

/// <summary>
///     The physical world.
/// </summary>
public class PhysicsWorld : SceneComponent
{
    /// <summary>
    ///     Physics max sub steps per frame.
    /// </summary>
    public static int MaxSubStep = 14;

    /// <summary>
    ///     Physics time factor.
    /// </summary>
    public static float TimeFactor = 1f;

    /// <summary>
    ///     Physics fixed timestep interval.
    /// </summary>
    public static float FixedTimeStep = 1f / 32f;

    private readonly BroadphaseInterface _broadphase;

    // physical world components
    private readonly CollisionConfiguration _config;

    // debug renderer
    private readonly PhysicsDebugDraw _debugDraw;
    private readonly Dispatcher _dispatcher;
    private readonly ConstraintSolver _solver;

    // current gravity vector
    private BulletSharp.Math.Vector3 _gravity;

    // physical world
    internal DynamicsWorld _world;

    /// <summary>
    ///     Init the physics world.
    /// </summary>
    public PhysicsWorld()
    {
        // init components
        _config = new DefaultCollisionConfiguration();
        _dispatcher = new CollisionDispatcher(_config);
        _broadphase = new DbvtBroadphase();
        _solver = new SequentialImpulseConstraintSolver();

        // create world instance
        _world = new DiscreteDynamicsWorld(
            _dispatcher,
            _broadphase,
            _solver,
            _config)
        {
            // for better performance
            ForceUpdateAllAabbs = false
        };

        // create debug renderer
        _debugDraw = new PhysicsDebugDraw(Core.GraphicsDevice);

        // set default gravity
        SetGravity(Vector3.Down * 9.8f);

        Initialize();
    }

    /// <summary>
    ///     Init the physics world.
    /// </summary>
    ~PhysicsWorld()
    {
        Destroy();
    }

    /// <summary>
    ///     Destroy the physical world.
    /// </summary>
    public void Destroy()
    {
        _world = null;
    }

    /// <summary>
    ///     Initialize physical-engine related stuff and set callbacks to respond to contact start / ended / processed events.
    /// </summary>
    public static void Initialize()
    {
        // set collision start callback
        ManifoldPoint.ContactAdded += (cp, obj0, partId0, index0, obj1, partId1, index1) =>
        {
            // get physical bodies
            var body0 = obj0.CollisionObject.UserObject as BasicPhysicalBody;
            var body1 = obj1.CollisionObject.UserObject as BasicPhysicalBody;

            // if one of the bodies don't support collision skip
            if (body0 == null || body1 == null) return;

            // store both bodies for the collision end event
            cp.UserPersistentData = new CollisionPersistData(body0, body1);

            // send collision events
            var data = new CollisionData(ToMonoGame.Vector(cp.PositionWorldOnA));
            body0.CallCollisionStart(body1, ref data);
            body1.CallCollisionStart(body0, ref data);
        };

        // set while-collising callback
        PersistentManifold.ContactProcessed += (cp, body0, body1) =>
        {
            if (cp.UserPersistentData == null) return;
            var data = (CollisionPersistData)cp.UserPersistentData;
            data.Body0.CallCollisionProcess(data.Body1);
            data.Body1.CallCollisionProcess(data.Body0);
        };

        // set collising-ended callback
        PersistentManifold.ContactDestroyed += userPersistantData =>
        {
            if (userPersistantData == null) return;
            var data = (CollisionPersistData)userPersistantData;
            data.Body0.CallCollisionEnd(data.Body1);
            data.Body1.CallCollisionEnd(data.Body0);
        };
    }

    /// <summary>
    ///     Called every frame to advance physics simulator.
    /// </summary>
    /// <param name="timeFactor">How much to advance this world step (or: time since last frame).</param>
    public override void Update()
    {
        if (TimeFactor == 0) return;
        _world.StepSimulation(Time.DeltaTime * TimeFactor, MaxSubStep, FixedTimeStep);
    }

    /// <summary>
    ///     Set gravity vector.
    /// </summary>
    /// <param name="gravity"></param>
    public void SetGravity(Vector3 gravity)
    {
        _gravity = ToBullet.Vector(gravity);
        _world.SetGravity(ref _gravity);
    }

    /// <summary>
    ///     Perform a raycast test and return colliding results.
    /// </summary>
    /// <param name="start">Start ray vector.</param>
    /// <param name="end">End ray vector.</param>
    /// <param name="returnNearest">If true, will only return the nearest object collided.</param>
    public RaycastResults Raycast(Vector3 start, Vector3 end, bool returnNearest = true)
    {
        // convert start and end vectors to bullet vectors
        var bStart = ToBullet.Vector(start);
        var bEnd = ToBullet.Vector(end);

        // create class to hold results
        var resultsCallback = returnNearest
            ? new ClosestRayResultCallback(ref bStart, ref bEnd) as RayResultCallback
            : new AllHitsRayResultCallback(bStart, bEnd);

        // perform ray cast
        return Raycast(bStart, bEnd, resultsCallback);
    }

    /// <summary>
    ///     Perform a raycast test and return colliding results, while ignoring 'self' object.
    /// </summary>
    /// <param name="start">Start ray vector.</param>
    /// <param name="end">End ray vector.</param>
    /// <param name="self">Physical body to ignore.</param>
    public RaycastResults Raycast(Vector3 start, Vector3 end, BasePhysicsComponent self)
    {
        // convert start and end vectors to bullet vectors
        var bStart = ToBullet.Vector(start);
        var bEnd = ToBullet.Vector(end);

        // create class to hold results
        RayResultCallback resultsCallback =
            new KinematicClosestNotMeRayResultCallback(self._PhysicalBody._BulletEntity);

        // perform ray cast
        return Raycast(bStart, bEnd, resultsCallback);
    }

    /// <summary>
    ///     Perform a raycast test and return colliding results.
    /// </summary>
    /// <param name="start">Start ray vector.</param>
    /// <param name="end">End ray vector.</param>
    /// <param name="resultsCallback">BulletSharp results callback.</param>
    internal RaycastResults Raycast(Vector3 start, Vector3 end, RayResultCallback resultsCallback)
    {
        // convert start and end vectors to bullet vectors
        var bStart = ToBullet.Vector(start);
        var bEnd = ToBullet.Vector(end);

        // perform the ray test
        return Raycast(bStart, bEnd, resultsCallback);
    }

    /// <summary>
    ///     Perform a raycast test and return colliding results, using native bullet objects.
    /// </summary>
    /// <param name="bStart">Start ray vector (bullet vector).</param>
    /// <param name="bEnd">End ray vector (bullet vector).</param>
    /// <param name="resultsCallback">BulletSharp results callback.</param>
    internal RaycastResults Raycast(BulletSharp.Math.Vector3 bStart, BulletSharp.Math.Vector3 bEnd,
        RayResultCallback resultsCallback)
    {
        // perform the ray test
        _world.RayTestRef(ref bStart, ref bEnd, resultsCallback);

        // create results object to return
        var results = new RaycastResults();

        // parse data based on type
        // closest result / closest but not me types:
        if (resultsCallback is ClosestRayResultCallback)
        {
            // convert to closest results type
            var closestReults = resultsCallback as ClosestRayResultCallback;

            // set results data
            results.HasHit = closestReults.HasHit;
            if (results.HasHit)
            {
                results.Collisions = new RaycastResults.SingleResult[1];
                results.Collisions[0].HitFraction = closestReults.ClosestHitFraction;
                results.Collisions[0].CollisionNormal = ToMonoGame.Vector(closestReults.HitNormalWorld);
                results.Collisions[0].CollisionPoint = ToMonoGame.Vector(closestReults.HitPointWorld);
                results.Collisions[0].CollisionBody =
                    (closestReults.CollisionObject.UserObject as BasicPhysicalBody).EcsComponent;
            }
        }
        // all results type
        else if (resultsCallback is AllHitsRayResultCallback)
        {
            // convert to all results type
            var allResults = resultsCallback as AllHitsRayResultCallback;

            // set results data
            results.HasHit = allResults.HasHit;
            if (results.HasHit)
            {
                results.Collisions = new RaycastResults.SingleResult[allResults.CollisionObjects.Count];
                for (var i = 0; i < allResults.CollisionObjects.Count; ++i)
                {
                    results.Collisions[i].HitFraction = allResults.HitFractions[i];
                    results.Collisions[i].CollisionNormal = ToMonoGame.Vector(allResults.HitNormalWorld[i]);
                    results.Collisions[i].CollisionPoint = ToMonoGame.Vector(allResults.HitPointWorld[i]);
                    results.Collisions[i].CollisionBody =
                        (allResults.CollisionObjects[i].UserObject as BasicPhysicalBody).EcsComponent;
                }
            }
        }

        // finally, return parsed results
        return results;
    }

    /// <summary>
    ///     Add a physical body to the world
    /// </summary>
    /// <param name="body">Physics entity to add.</param>
    public void AddBody(BasicPhysicalBody body)
    {
        CountAndAlert.Count(CountAndAlert.PredefAlertTypes.AddedOrCreated);
        body.AddSelfToBulletWorld(_world);
        body._world = this;
    }

    /// <summary>
    ///     Update single object's aabb.
    /// </summary>
    /// <param name="body">Body to update.</param>
    public void UpdateSingleAabb(BasicPhysicalBody body)
    {
        _world.UpdateSingleAabb(body._BulletEntity);
    }

    /// <summary>
    ///     Remove a physical body from the world.
    /// </summary>
    /// <param name="body"></param>
    public void RemoveBody(BasicPhysicalBody body)
    {
        // this might happen after the world was destroyed, hence the _world != null test.
        if (_world != null) body.RemoveSelfFromBulletWorld(_world);

        body._world = null;
    }

    /// <summary>
    ///     Debug-draw the physical world.
    /// </summary>
    public void DebugDraw()
    {
        _debugDraw.DrawDebugWorld(_world);
    }

    /// <summary>
    ///     Class to store persistent collision data, so that bullet detach events will work.
    /// </summary>
    private struct CollisionPersistData
    {
        public readonly BasicPhysicalBody Body0;
        public readonly BasicPhysicalBody Body1;

        public CollisionPersistData(BasicPhysicalBody body0, BasicPhysicalBody body1)
        {
            Body0 = body0;
            Body1 = body1;
        }
    }
}