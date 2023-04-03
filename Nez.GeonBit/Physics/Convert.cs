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
// Provide utilities to convert between MonoGame and Bullet classes.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace Nez.GeonBit.Physics
{
    /// <summary>
    /// Convert from MonoGame to Bullet3d.
    /// </summary>
    public static class ToBullet
    {
        /// <summary>
        /// Convert a vector from MonoGame to Bullet3d.
        /// </summary>
        /// <param name="vec">Vector to convert.</param>
        /// <returns>Bullet vector.</returns>
        public static BulletSharp.Math.Vector3 Vector(Vector3 vec) => new BulletSharp.Math.Vector3(vec.X, vec.Y, vec.Z);

        /// <summary>
        /// Convert an array of vectors from MonoGame to Bullet3d.
        /// </summary>
        /// <param name="vecs">Vectors array to convert.</param>
        /// <returns>Array of Bullet vectors.</returns>
        public static BulletSharp.Math.Vector3[] Vectors(Vector3[] vecs)
        {
            var bvectors = new BulletSharp.Math.Vector3[vecs.Length];
            int i = 0;
            foreach (var vector in vecs)
            {
                bvectors[i++] = ToBullet.Vector(vector);
            }
            return bvectors;
        }

        /// <summary>
        /// Convert a matrix from MonoGame to Bullet3d.
        /// </summary>
        /// <param name="matrix">Matrix to convert.</param>
        /// <returns>Bullet matrix.</returns>
        public static BulletSharp.Math.Matrix Matrix(Matrix matrix) => new BulletSharp.Math.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    /// <summary>
    /// Convert from Bullet3d to MonoGame.
    /// </summary>
    public static class ToMonoGame
    {
        /// <summary>
        /// Convert a vector from Bullet to MonoGame.
        /// </summary>
        /// <param name="vec">Vector to convert.</param>
        /// <returns>MonoGame vector</returns>
        public static Vector3 Vector(BulletSharp.Math.Vector3 vec) => new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);

        /// <summary>
        /// Convert an array of vectors from Bullet3d to MonoGame.
        /// </summary>
        /// <param name="bvecs">Bullet3d vectors array to convert.</param>
        /// <returns>Array of MonoGame vectors.</returns>
        public static Vector3[] Vectors(BulletSharp.Math.Vector3[] bvecs)
        {
            var vectors = new Vector3[bvecs.Length];
            int i = 0;
            foreach (var bvector in bvecs)
            {
                vectors[i++] = ToMonoGame.Vector(bvector);
            }
            return vectors;
        }

        /// <summary>
        /// Convert a matrix from Bullet to MonoGame.
        /// </summary>
        /// <param name="matrix">Matrix to convert.</param>
        /// <returns>MonoGame matrix.</returns>
        public static Matrix Matrix(BulletSharp.Math.Matrix matrix) => new Matrix(
                (float)matrix.M11, (float)matrix.M12, (float)matrix.M13, (float)matrix.M14,
                (float)matrix.M21, (float)matrix.M22, (float)matrix.M23, (float)matrix.M24,
                (float)matrix.M31, (float)matrix.M32, (float)matrix.M33, (float)matrix.M34,
                (float)matrix.M41, (float)matrix.M42, (float)matrix.M43, (float)matrix.M44);
    }
}
