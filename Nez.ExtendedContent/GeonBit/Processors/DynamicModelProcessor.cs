#region License

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Nez.ExtendedContent.GeonBit.Graphics;

namespace Nez.ExtendedContent.GeonBit.Serialization;

[ContentProcessor(DisplayName = "DynamicModel - GeonBit")]
public class DynamicModelProcessor : ModelProcessor, IContentProcessor
{
    // used to avoid creating clones/duplicates of the same VertexBufferContent
    private readonly Dictionary<VertexBufferContent, DynamicVertexBufferContent> _vertexBufferCache = new();

    // used to avoid creating clones/duplicates of the same Index Buffer
    private readonly Dictionary<Collection<int>, DynamicIndexBufferContent> _indexBufferCache = new();

#if WINDOWS
        // override OutputType
        [Browsable(false)]
#endif
    Type IContentProcessor.OutputType => typeof(DynamicModelContent);

    [DefaultValue(DynamicModelContent.BufferType.Dynamic)]
    public virtual DynamicModelContent.BufferType VertexBufferType { get; set; } =
        DynamicModelContent.BufferType.Dynamic;

    [DefaultValue(DynamicModelContent.BufferType.Dynamic)]
    public virtual DynamicModelContent.BufferType IndexBufferType { get; set; } =
        DynamicModelContent.BufferType.Dynamic;

    object IContentProcessor.Process(object input, ContentProcessorContext context)
    {
        var model = Process((NodeContent)input, context);
        var dynamicModel = new DynamicModelContent(model);

        foreach (var mesh in dynamicModel.Meshes)
        foreach (var part in mesh.MeshParts)
        {
            ProcessVertexBuffer(dynamicModel, context, part);
            ProcessIndexBuffer(dynamicModel, context, part);
        }

        return dynamicModel;
    }

    protected virtual void ProcessVertexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context,
        DynamicModelMeshPartContent part)
    {
        if (VertexBufferType != DynamicModelContent.BufferType.Default)
        {
            // Replace the default VertexBufferContent with CpuAnimatedVertexBufferContent.
            if (!_vertexBufferCache.TryGetValue(part.VertexBuffer, out var vb))
            {
                vb = new DynamicVertexBufferContent(part.VertexBuffer)
                {
                    IsWriteOnly = VertexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly
                };
                _vertexBufferCache[part.VertexBuffer] = vb;
            }

            part.VertexBuffer = vb;
        }
    }

    protected virtual void ProcessIndexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context,
        DynamicModelMeshPartContent part)
    {
        if (IndexBufferType != DynamicModelContent.BufferType.Default)
        {
            if (!_indexBufferCache.TryGetValue(part.IndexBuffer, out var ib))
            {
                ib = new DynamicIndexBufferContent(part.IndexBuffer)
                {
                    IsWriteOnly = IndexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly
                };
                _indexBufferCache[part.IndexBuffer] = ib;
            }

            part.IndexBuffer = ib;
        }
    }
}