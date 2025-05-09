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
// A static mesh that is built from other models and meshes at runtime.
// This allows you to build parts of the level dynamically, and then reduce draw
// calls significantly by merging them into one static batch.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Lights;
using Nez.GeonBit.Materials;

namespace Nez.GeonBit;

/// <summary>
///     Vertex type we are using for the combined mesh.
/// </summary>
public enum VertexTypes
{
    /// <summary>
    ///     Vertex with position only.
    /// </summary>
    VertexPosition,

    /// <summary>
    ///     Vertex with position and color.
    /// </summary>
    VertexPositionColor,

    /// <summary>
    ///     Vertex with position and texture.
    /// </summary>
    VertexPositionTexture,

    /// <summary>
    ///     Vertex with position, normal and texture.
    /// </summary>
    VertexPositionNormalTexture,

    /// <summary>
    ///     Vertex for normal mapping, with position, normal, tangent, binormal and texture coords.
    /// </summary>
    VertexPositionNormalTangentTexture
}

/// <summary>
///     A special renderer that takes several other renderers (meshes, models etc) and merge them into a single batch.
///     This optimization allows you to build levels out of static models, and reduce draw calls by merging them into one
///     static mesh at runtime.
///     Note: While its a good way to reduce draw calls, don't create combined meshes too big or else you'll lose some
///     culling optimizations.
/// </summary>
public class CombinedMeshesEntity<VertexType> : BaseRenderableEntity, IShadowCaster
    where VertexType : struct, IVertexType
{
    // vertex type we use in this combined mesh.
    private readonly VertexTypes _vtype;

    // store all vertices positions - needed to calculate bounding box / sphere. This list is cleared once built.
    private List<Vector3> _allPoints = new();

    /// <summary>
    ///     Combined mesh bounding box, in local space.
    /// </summary>
    private BoundingBox _localBoundingBox;

    /// <summary>
    ///     Combined mesh bounding sphere, in local space.
    /// </summary>
    private BoundingSphere _localBoundingSphere;

    // dictionary to hold the combined parts and their materials.
    // key is material so we can draw them in chunks sharing the same material and texture.
    private Dictionary<MaterialAPI, CombinedMeshesPart> _parts = new();

    /// <summary>
    ///     Did we already build this combined mesh entity? happens on first draw, or when "build" is called.
    /// </summary>
    private bool _wasBuilt;

    /// <summary>
    ///     Create the combined mesh entity.
    /// </summary>
    public CombinedMeshesEntity()
    {
        if (typeof(VertexType) == typeof(VertexPosition))
            _vtype = VertexTypes.VertexPosition;
        else if (typeof(VertexType) == typeof(VertexPositionColor))
            _vtype = VertexTypes.VertexPositionColor;
        else if (typeof(VertexType) == typeof(VertexPositionTexture))
            _vtype = VertexTypes.VertexPositionTexture;
        else if (typeof(VertexType) == typeof(VertexPositionNormalTexture))
            _vtype = VertexTypes.VertexPositionNormalTexture;
        else if (typeof(VertexType) == typeof(VertexPositionNormalTangentTexture))
            _vtype = VertexTypes.VertexPositionNormalTangentTexture;
        else
            throw new ArgumentException("Unsupported vertex type in combined mesh!");
    }

    public int PrimaryLight { get; set; }
    public bool CastsShadow { get; set; }
    public int ShadowCasterLOD { get; set; }
    public RasterizerState ShadowRasterizerState { get; set; }

    void IShadowCaster.RenderShadows(Matrix worldTransform)
    {
        // get graphic device
        var device = Core.GraphicsDevice;

        // iterate combined parts
        foreach (var combinedPart in _parts)
        {
            // get vertices and indexes
            var buffers = combinedPart.Value;

            // set vertex and indices buffers
            device.SetVertexBuffer(buffers._VertexBuffer);
            device.Indices = buffers._IndexBuffer;

            LightsManager.ShadowEffect.CurrentTechnique.Passes[0].Apply();

            // draw
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, buffers.PrimitiveCount);
        }
    }

    /// <summary>
    ///     Clone this combined entity.
    /// </summary>
    /// <returns>Cloned copy.</returns>
    public CombinedMeshesEntity<VertexType> Clone()
    {
        var ret = new CombinedMeshesEntity<VertexType>
        {
            _localBoundingBox = _localBoundingBox,
            _localBoundingSphere = _localBoundingSphere,
            _allPoints = new List<Vector3>(_allPoints),
            _parts = new Dictionary<MaterialAPI, CombinedMeshesPart>(_parts),
            _wasBuilt = _wasBuilt
        };
        return ret;
    }

    /// <summary>
    ///     Add a model to the combined mesh.
    /// </summary>
    /// <param name="model">Model to add.</param>
    /// <param name="transform">World transformations.</param>
    /// <param name="material">Optional material to use instead of the model default materials.</param>
    public void AddModel(Model model, Matrix transform, MaterialAPI material = null)
    {
        // sanity check - if build was called assert
        if (_wasBuilt)
            throw new InvalidOperationException("Cannot add model to Combined Mesh Entity after it was built!");

        // iterate model meshes and add them
        for (var i = 0; i < model.Meshes.Count; i++)
        {
            var mesh = model.Meshes[i];
            AddModelMesh(mesh, transform, material);
        }
    }

    /// <summary>
    ///     Convert to vertex type from template.
    /// </summary>
    /// <param name="ver">Any vertex type to convert.</param>
    /// <returns>Vertex as template VertexType.</returns>
    private VertexType ToVertexType(IVertexType ver)
    {
        return (VertexType)ver;
    }

    /// <summary>
    ///     Convert to specific vertex type.
    /// </summary>
    /// <param name="ver">Any vertex type to convert.</param>
    /// <returns>Vertex as chosen vertex type.</returns>
    private ToVType ToSpecificVertexType<ToVType>(IVertexType ver)
    {
        return (ToVType)ver;
    }

    /// <summary>
    ///     Add a model mesh to the combined mesh.
    /// </summary>
    /// <param name="mesh">Mesh to add.</param>
    /// <param name="transform">World transformations.</param>
    /// <param name="material">Optional material to use instead of the mesh default materials.</param>
    public void AddModelMesh(ModelMesh mesh, Matrix transform, MaterialAPI material = null)
    {
        // sanity check - if build was called assert
        if (_wasBuilt)
            throw new InvalidOperationException("Cannot add meshes to Combined Mesh Entity after it was built!");

        // did we get material override to set?
        var externalMaterial = material != null;

        // iterate mesh parts
        for (var i1 = 0; i1 < mesh.MeshParts.Count; i1++)
        {
            var meshPart = mesh.MeshParts[i1];
            // if we didn't get external material to use, get material from mesh part.
            if (!externalMaterial) material = meshPart.GetMaterial();

            // get the combined chunk to add this meshpart to
            var combinedPart = GetCombinedPart(material);

            // make sure its not readonly
            if (meshPart.VertexBuffer.BufferUsage == BufferUsage.WriteOnly ||
                meshPart.IndexBuffer.BufferUsage == BufferUsage.WriteOnly)
                throw new InvalidOperationException("Cannot add mesh with write-only buffers to Combined Mesh!");

            // make sure vertex buffer uses position-normal-texture
            if (meshPart.VertexBuffer.VertexDeclaration.VertexStride < 8)
                throw new InvalidOperationException(
                    "Combined meshes can only use vertex buffers with position, normal and texture!");

            // get vertex buffer parameters
            var vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
            var vertexBufferSize = meshPart.NumVertices * vertexStride;

            // get vertex data as float
            var vertexData = new float[vertexBufferSize / sizeof(float)];
            meshPart.VertexBuffer.GetData(vertexData);

            // iterate through vertices and add them
            var verticesInPart = 0;
            for (var i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
            {
                // get curr position with transformations
                var currPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]),
                    transform);

                // get other vertex properties based on type and add to vertices buffer
                switch (_vtype)
                {
                    case VertexTypes.VertexPosition:
                    {
                        // add to buffer
                        var vertexToAdd = new VertexPosition(currPosition);
                        combinedPart.Vertices.Add(ToVertexType(vertexToAdd));
                        break;
                    }
                    case VertexTypes.VertexPositionColor:
                    {
                        // get color
                        var currColor = new Color(vertexData[i + 3], vertexData[i + 4], vertexData[i + 5],
                            vertexData[i + 6]);

                        // add to buffer
                        var vertexToAdd = new VertexPositionColor(currPosition, currColor);
                        combinedPart.Vertices.Add(ToVertexType(vertexToAdd));
                        break;
                    }
                    case VertexTypes.VertexPositionNormalTexture:
                    {
                        // get normal
                        var normal = new Vector3(vertexData[i + 3], vertexData[i + 4], vertexData[i + 5]);
                        normal = Vector3.Normalize(Vector3.TransformNormal(normal, transform));

                        // get texture coords
                        var textcoords = new Vector2(vertexData[i + 6], vertexData[i + 7]);

                        // add to buffer
                        var vertexToAdd = new VertexPositionNormalTexture(currPosition, normal, textcoords);
                        combinedPart.Vertices.Add(ToVertexType(vertexToAdd));
                        break;
                    }
                    case VertexTypes.VertexPositionNormalTangentTexture:
                    {
                        // get normal
                        var normal = new Vector3(vertexData[i + 3], vertexData[i + 4], vertexData[i + 5]);
                        normal = Vector3.Normalize(Vector3.TransformNormal(normal, transform));

                        // get tangent
                        var tangent = new Vector3(vertexData[i + 6], vertexData[i + 7], vertexData[i + 8]);

                        // get binormal
                        var binormal = new Vector3(vertexData[i + 9], vertexData[i + 10], vertexData[i + 11]);

                        // get texture coords
                        var textcoords = new Vector2(vertexData[i + 12], vertexData[i + 13]);

                        // add to buffer
                        var vertexToAdd =
                            new VertexPositionNormalTangentTexture(currPosition, normal, textcoords, tangent, binormal);
                        combinedPart.Vertices.Add(ToVertexType(vertexToAdd));
                        break;
                    }
                    case VertexTypes.VertexPositionTexture:
                    {
                        // get texture coords
                        var textcoords = new Vector2(vertexData[i + 3], vertexData[i + 4]);

                        // add to buffer
                        var vertexToAdd = new VertexPositionTexture(currPosition, textcoords);
                        combinedPart.Vertices.Add(ToVertexType(vertexToAdd));
                        break;
                    }
                    default:
                        throw new Exception("Forgot to implement type support?");
                }

                // add to temp list of all points and increase vertices count
                _allPoints.Add(currPosition);
                verticesInPart++;
            }

            // set indexes
            var drawOrder = new ushort[meshPart.IndexBuffer.IndexCount];
            meshPart.IndexBuffer.GetData(drawOrder);
            combinedPart.PushIndexes(drawOrder);

            // increase indexes offset
            combinedPart.IndexOffset += verticesInPart;

            // increase primitives count
            combinedPart.PrimitiveCount += meshPart.PrimitiveCount;
        }
    }

    /// <summary>
    ///     Add array of vertices to the combined mesh.
    /// </summary>
    /// <param name="vertices">Vertices array to add.</param>
    /// <param name="indexes">Draw order / indexes array.</param>
    /// <param name="transform">World transformations.</param>
    /// <param name="material">Material to use with the vertices.</param>
    public void AddVertices(VertexType[] vertices, ushort[] indexes, Matrix transform, MaterialAPI material)
    {
        // sanity check - if build was called assert
        if (_wasBuilt)
            throw new InvalidOperationException("Cannot add vertices to Combined Mesh Entity after it was built!");

        // if transform is identity skip everything here
        if (transform == Matrix.Identity)
        {
            AddVertices(vertices, indexes, material);
            return;
        }

        // transform all vertices from array
        var i = 0;
        var processed = new VertexType[vertices.Length];
        for (var j = 0; j < vertices.Length; j++)
        {
            var vertex = vertices[j];
            // get current vertex
            var curr = vertex;

            // apply transformations
            switch (_vtype)
            {
                case VertexTypes.VertexPosition:
                {
                    var currVer = ToSpecificVertexType<VertexPosition>(curr);
                    currVer.Position = Vector3.Transform(currVer.Position, transform);
                    processed[i++] = ToVertexType(currVer);
                    break;
                }
                case VertexTypes.VertexPositionColor:
                {
                    var currVer = ToSpecificVertexType<VertexPositionColor>(curr);
                    currVer.Position = Vector3.Transform(currVer.Position, transform);
                    processed[i++] = ToVertexType(currVer);
                    break;
                }
                case VertexTypes.VertexPositionNormalTexture:
                {
                    var currVer = ToSpecificVertexType<VertexPositionNormalTexture>(curr);
                    currVer.Position = Vector3.Transform(currVer.Position, transform);
                    currVer.Normal = Vector3.Normalize(Vector3.TransformNormal(currVer.Normal, transform));
                    processed[i++] = ToVertexType(currVer);
                    break;
                }
                case VertexTypes.VertexPositionNormalTangentTexture:
                {
                    var currVer = ToSpecificVertexType<VertexPositionNormalTangentTexture>(curr);
                    currVer.Position = Vector3.Transform(currVer.Position, transform);
                    currVer.Normal = Vector3.Normalize(Vector3.TransformNormal(currVer.Normal, transform));
                    currVer.Tangent = Vector3.Normalize(Vector3.TransformNormal(currVer.Tangent, transform));
                    currVer.Binormal = Vector3.Normalize(Vector3.TransformNormal(currVer.Binormal, transform));
                    processed[i++] = ToVertexType(currVer);
                    break;
                }
                case VertexTypes.VertexPositionTexture:
                {
                    var currVer = ToSpecificVertexType<VertexPositionTexture>(curr);
                    currVer.Position = Vector3.Transform(currVer.Position, transform);
                    processed[i++] = ToVertexType(currVer);
                    break;
                }
            }
        }

        // add processed vertices
        AddVertices(processed, indexes, material);
    }

    /// <summary>
    ///     Get position from vertex.
    /// </summary>
    /// <param name="vertex">Vertex to extract position from.</param>
    /// <returns>Vertex position.</returns>
    private Vector3 GetPosition(VertexType vertex)
    {
        switch (_vtype)
        {
            case VertexTypes.VertexPosition:
                return ToSpecificVertexType<VertexPosition>(vertex).Position;
            case VertexTypes.VertexPositionColor:
                return ToSpecificVertexType<VertexPositionColor>(vertex).Position;
            case VertexTypes.VertexPositionNormalTexture:
                return ToSpecificVertexType<VertexPositionNormalTexture>(vertex).Position;
            case VertexTypes.VertexPositionTexture:
                return ToSpecificVertexType<VertexPositionTexture>(vertex).Position;
            case VertexTypes.VertexPositionNormalTangentTexture:
                return ToSpecificVertexType<VertexPositionNormalTangentTexture>(vertex).Position;
            default:
                throw new Exception("Forgot to implement type?");
        }
    }

    /// <summary>
    ///     Add array of vertices to the combined mesh.
    /// </summary>
    /// <param name="vertices">Vertices array to add.</param>
    /// <param name="indexes">Draw order / indexes array.</param>
    /// <param name="material">Material to use with the vertices.</param>
    public void AddVertices(VertexType[] vertices, ushort[] indexes, MaterialAPI material)
    {
        // sanity check - if build was called assert
        if (_wasBuilt)
            throw new InvalidOperationException("Cannot add vertices to Combined Mesh Entity after it was built!");

        // get the combined chunk to add these vertices to
        var combinedPart = GetCombinedPart(material);

        // add vertices to combined part
        combinedPart.Vertices.AddRange(vertices);
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            _allPoints.Add(GetPosition(vertex));
        }

        // set indexes
        combinedPart.PushIndexes(indexes);

        // increase index offset in combined part
        combinedPart.IndexOffset += vertices.Length;

        // update primitives count
        combinedPart.PrimitiveCount += indexes.Length / 3;
    }

    /// <summary>
    ///     Get combined mesh part from material.
    /// </summary>
    /// <param name="material">Material to get combined part for.</param>
    /// <returns>Combined mesh part.</returns>
    private CombinedMeshesPart GetCombinedPart(MaterialAPI material)
    {
        // try to get from existing parts and if not found create it
        if (!_parts.TryGetValue(material, out var combinedPart))
        {
            combinedPart = new CombinedMeshesPart();
            _parts[material] = combinedPart;
        }

        // return combined part
        return combinedPart;
    }

    /// <summary>
    ///     Clear everything from the combined meshed renderer.
    /// </summary>
    public void Clear()
    {
        _parts.Clear();
        _allPoints.Clear();
        RebuildBoundingBoxAndSphere();
        _wasBuilt = false;
    }

    /// <summary>
    ///     Build the combined meshes renderer.
    /// </summary>
    public void Build()
    {
        // build parts
        foreach (var combinedPart in _parts) combinedPart.Value.Build();

        // build bounding box and sphere
        RebuildBoundingBoxAndSphere();

        // clear points
        _allPoints.Clear();

        // mark as built
        _wasBuilt = true;
    }

    /// <summary>
    ///     Draw this model.
    /// </summary>
    /// <param name="worldTransformations">
    ///     World transformations to apply on this entity (this is what you should use to draw
    ///     this entity).
    /// </param>
    public override void DoEntityDraw(ref Matrix worldTransformations)
    {
        // get graphic device
        var device = Core.GraphicsDevice;

        // iterate combined parts
        foreach (var combinedPart in _parts)
        {
            // get and setup material
            var material = combinedPart.Key;
            material.Apply(ref worldTransformations, ref _lastBoundingSphere, PrimaryLight);

            // get vertices and indexes
            var buffers = combinedPart.Value;

            // set vertex and indices buffers
            device.SetVertexBuffer(buffers._VertexBuffer);
            device.Indices = buffers._IndexBuffer;

            // draw with material
            material.IterateEffectPasses(pass =>
            {
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, buffers.PrimitiveCount);
            });
        }
    }

    /// <summary>
    ///     Rebuild bounding box and bounding sphere.
    /// </summary>
    private void RebuildBoundingBoxAndSphere()
    {
        if (_allPoints.Count != 0)
        {
            _localBoundingBox = BoundingBox.CreateFromPoints(_allPoints);
            _localBoundingSphere = BoundingSphere.CreateFromPoints(_allPoints);
        }
        else
        {
            _localBoundingBox = new BoundingBox();
            _localBoundingSphere = new BoundingSphere();
        }
    }

    /// <summary>
    ///     Get the bounding sphere of this entity.
    /// </summary>
    /// <param name="parent">Parent node that's currently drawing this entity.</param>
    /// <param name="localTransformations">Local transformations from the direct parent node.</param>
    /// <param name="worldTransformations">
    ///     World transformations to apply on this entity (this is what you should use to draw
    ///     this entity).
    /// </param>
    /// <returns>Bounding box of the entity.</returns>
    protected override BoundingSphere CalcBoundingSphere(Node parent, ref Matrix localTransformations,
        ref Matrix worldTransformations)
    {
        // get bounding sphere in local space
        var modelBoundingSphere = _localBoundingSphere;

        // apply transformations on bounding sphere
        var scale = Math3D.GetScale(ref worldTransformations);
        modelBoundingSphere.Radius *= Math.Max(scale.X, Math.Max(scale.Y, scale.Z));
        modelBoundingSphere.Center = Vector3.Transform(modelBoundingSphere.Center, worldTransformations);
        return modelBoundingSphere;
    }

    /// <summary>
    ///     Get the bounding box of this entity.
    /// </summary>
    /// <param name="parent">Parent node that's currently drawing this entity.</param>
    /// <param name="localTransformations">Local transformations from the direct parent node.</param>
    /// <param name="worldTransformations">
    ///     World transformations to apply on this entity (this is what you should use to draw
    ///     this entity).
    /// </param>
    /// <returns>Bounding box of the entity.</returns>
    protected override BoundingBox CalcBoundingBox(Node parent, ref Matrix localTransformations,
        ref Matrix worldTransformations)
    {
        // get bounding box in local space
        var modelBoundingBox = _localBoundingBox;

        // initialize minimum and maximum corners of the bounding box to max and min values
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // iterate bounding box corners and transform them
        var array = modelBoundingBox.GetCorners();
        for (var i = 0; i < array.Length; i++)
        {
            var corner = array[i];
            // get curr position and update min / max
            var currPosition = Vector3.Transform(corner, worldTransformations);
            min = Vector3.Min(min, currPosition);
            max = Vector3.Max(max, currPosition);
        }

        // create and return transformed bounding box
        return new BoundingBox(min, max);
    }

    /// <summary>
    ///     Represent the vertices and indexes of a combined mesh part.
    ///     This represent a chunk of couple of meshes combined together, all vertices are in world space after transformations
    ///     applied.
    /// </summary>
    private class CombinedMeshesPart
    {
        /// <summary>
        ///     Index buffer.
        /// </summary>
        public IndexBuffer _IndexBuffer;

        /// <summary>
        ///     Vertex buffer.
        /// </summary>
        public VertexBuffer _VertexBuffer;

        /// <summary>
        ///     Vertices buffer.
        /// </summary>
        public ResizableRentedArray<VertexType> Vertices { get; } = new();

        /// <summary>
        ///     Vertices indexes.
        /// </summary>
        public ResizableRentedArray<ushort> Indexes { get; } = new();

        /// <summary>
        ///     Primitives count.
        /// </summary>
        public int PrimitiveCount { get; internal set; }

        /// <summary>
        ///     Count index offset while building the combined mesh.
        /// </summary>
        public int IndexOffset { get; internal set; }

        /// <summary>
        ///     Push array of indexes into the combined mesh.
        /// </summary>
        /// <param name="drawOrder">Array of indexes to push.</param>
        public void PushIndexes(ushort[] drawOrder)
        {
            for (var i = 0; i < drawOrder.Length; i++)
            {
                var currIndex = (short)drawOrder[i];

                // calculate absolute index
                var absIndex = (ushort)(currIndex + (ushort)IndexOffset);

                // make sure didn't overflow
                if (absIndex < IndexOffset)
                    throw new ArgumentOutOfRangeException("Too many vertices were pushed into combined mesh!");

                // add to indexes array
                Indexes.Add(absIndex);
            }
        }

        /// <summary>
        ///     Build vertex and indexes buffer and clear lists.
        /// </summary>
        public void Build()
        {
            // get device
            var device = Core.GraphicsDevice;

            // build vertex buffer
            Vertices.Trim();
            _VertexBuffer = new VertexBuffer(device, typeof(VertexType), Vertices.Count, BufferUsage.WriteOnly);
            _VertexBuffer.SetData(Vertices.InternalArray);
            Vertices.Clear();

            // build indexes buffer
            Indexes.Trim();
            _IndexBuffer = new IndexBuffer(device, typeof(ushort), Indexes.Count, BufferUsage.WriteOnly);
            _IndexBuffer.SetData(Indexes.InternalArray);
            Indexes.Clear();
        }
    }
}