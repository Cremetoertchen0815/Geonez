﻿#region License

//   Copyright 2016 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#endregion

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Nez.ExtendedContent.GeonBit.Graphics;

public class DynamicModelMeshContent
{
    public DynamicModelMeshContent(ModelMeshContent source)
    {
        Source = source;

        //deep clone MeshParts
        MeshParts = new List<DynamicModelMeshPartContent>(source.MeshParts.Count);
        foreach (var mesh in source.MeshParts)
            MeshParts.Add(new DynamicModelMeshPartContent(mesh));
    }

    protected internal ModelMeshContent Source { get; protected set; }

    // Summary:
    //     Gets the mesh name.
    public string Name => Source.Name;

    // Summary:
    //     Gets the parent bone.
    [ContentSerializerIgnore] public ModelBoneContent ParentBone => Source.ParentBone;

    // Summary:
    //     Gets the bounding sphere for this mesh.
    public BoundingSphere BoundingSphere => Source.BoundingSphere;

    // Summary:
    //     Gets the children mesh parts associated with this mesh.
    [ContentSerializerIgnore] public List<DynamicModelMeshPartContent> MeshParts { get; }

    // Summary:
    //     Gets a user defined tag object.
    [ContentSerializer(SharedResource = true)]
    public object Tag
    {
        get => Source.Tag;
        set => Source.Tag = value;
    }
}