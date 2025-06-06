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
// Math and vector-related utils & functions.
//
// Author: Ronen Ness.
// Since: 2016.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;

namespace Nez.GeonBit;

/// <summary>
///     Contain different math utils and vector-related helper functions.
/// </summary>
public static class Math3D
{
    // used as default random object if not provided
    private static readonly System.Random _rand = new();

    /// <summary>
    ///     Convert degrees to radians.
    /// </summary>
    /// <param name="degrees">Degrees to convert to radians.</param>
    /// <returns>Converted degrees as radians.</returns>
    public static float DegreeToRadian(float degrees)
    {
        return (float)(Math.PI / 180 * degrees);
    }

    /// <summary>
    ///     Convert radians to degrees.
    /// </summary>
    /// <param name="radians">Radians to convert to degrees.</param>
    /// <returns>Converted radians as degrees.</returns>
    public static float RadianToDegree(float radians)
    {
        return (float)(radians * (180.0 / Math.PI));
    }

    /// <summary>
    ///     Return a vector pointing to the 'left' side of a given vector.
    /// </summary>
    /// <param name="vector">Vector to get left vector from.</param>
    /// <param name="zeroY">If true, will zero Y component.</param>
    /// <returns>Vector pointing to the left of the given vector.</returns>
    public static Vector3 GetLeftVector(Vector3 vector, bool zeroY = false)
    {
        var ret = Vector3.Transform(vector, Matrix.CreateFromAxisAngle(Vector3.Up, DegreeToRadian(90)));
        if (zeroY)
        {
            ret.Y = 0.0f;
            ret.Normalize();
        }

        return ret;
    }

    /// <summary>
    ///     Return a vector pointing to the 'right' side of a given vector.
    /// </summary>
    /// <param name="vector">Vector to get right vector from.</param>
    /// <param name="zeroY">If true, will zero Y component.</param>
    /// <returns>Vector pointing to the right of the given vector.</returns>
    public static Vector3 GetRightVector(Vector3 vector, bool zeroY = false)
    {
        var ret = Vector3.Transform(vector, Matrix.CreateFromAxisAngle(Vector3.Up, DegreeToRadian(-90)));
        if (zeroY)
        {
            ret.Y = 0.0f;
            ret.Normalize();
        }

        return ret;
    }

    /// <summary>
    ///     Extract the correct scale from matrix.
    /// </summary>
    /// <param name="mat">Matrix to get scale from.</param>
    /// <returns>Matrix scale.</returns>
    public static Vector3 GetScale(ref Matrix mat)
    {
        mat.Decompose(out var scale, out var rot, out var pos);
        return scale;
    }

    /// <summary>
    ///     Extract the correct rotation from matrix.
    /// </summary>
    /// <param name="mat">Matrix to get rotation from.</param>
    /// <returns>Matrix rotation.</returns>
    public static Quaternion GetRotation(ref Matrix mat)
    {
        mat.Decompose(out var scale, out var rot, out var pos);
        return rot;
    }

    /// <summary>
    ///     Extract yaw, pitch and roll from existing matrix.
    /// </summary>
    /// <param name="matrix">Matrix to extract from.</param>
    /// <param name="yaw">Out yaw value.</param>
    /// <param name="pitch">Out pitch value.</param>
    /// <param name="roll">Out roll value.</param>
    public static void ExtractYawPitchRoll(Matrix matrix, out float yaw, out float pitch, out float roll)
    {
        yaw = (float)Math.Atan2(matrix.M13, matrix.M33);
        pitch = (float)Math.Asin(-matrix.M23);
        roll = (float)Math.Atan2(matrix.M21, matrix.M22);
    }

    /// <summary>
    ///     Wrap an angle to be between 0 and 360.
    /// </summary>
    /// <param name="angle">Angle to wrap (degrees).</param>
    /// <returns>Wrapped angle.</returns>
    public static uint WrapAngle(int angle)
    {
        while (angle < 0) angle += 360;
        while (angle > 360) angle -= 360;
        return (uint)angle;
    }

    /// <summary>
    ///     Wrap an radian to be between 0 and 2PI.
    /// </summary>
    /// <param name="radian">Radian to wrap.</param>
    /// <returns>Wrapped radian.</returns>
    public static float WrapRadian(float radian)
    {
        var max = DegreeToRadian(360);
        while (radian < 0) radian += max;
        while (radian > max) radian -= max;
        return radian;
    }

    /// <summary>
    ///     Return distance between two angles, in degrees.
    ///     For example: AnglesDistance(90, 45) will return 45,
    ///     AnglesDistance(1, 360) will return 1, etc..
    /// </summary>
    /// <param name="angle1">First angle to check distance from (degrees).</param>
    /// <param name="angle2">Second angle to check distance from (degrees).</param>
    /// <returns>Return minimal degree between two angles.</returns>
    public static uint AnglesDistance(uint angle1, uint angle2)
    {
        // calc distance from 1 to 2
        var a = (int)angle1 - (int)angle2;
        while (a < 0) a += 360;

        // if less than 180, this is the shortest distance between angles
        if (a <= 180) return (uint)a;
        // if more than 180, shortest distance is 360 - a
        return (uint)(360 - a);
    }

    /// <summary>
    ///     pick a random index based of list of probabilities (array of floats representing chances).
    /// </summary>
    /// <param name="probabilities">Array of floats representing chance for every index.</param>
    /// <param name="rand">Optional random instance to provide (if null will create new one internally).</param>
    /// <returns>The index of the item picked randomly from the list of probabilities.</returns>
    public static uint PickBasedOnProbability(float[] probabilities, System.Random rand = null)
    {
        // if not provided, create default random object
        if (rand == null) rand = _rand;

        // get random double
        var diceRoll = rand.NextDouble();

        // multiply diceroll by total
        var fac = 0.0;
        for (var i = 0; i < probabilities.Length; ++i) fac += probabilities[i];
        diceRoll *= fac;

        // iterate over probabilities and pick the one that match
        var cumulative = 0.0;
        for (var i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (diceRoll < cumulative) return (uint)i;
        }

        // should never happen!
        throw new Exception("Internal error with PickBasedOnProbability!");
    }
}