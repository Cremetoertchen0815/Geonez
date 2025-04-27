#region License

/// -------------------------------------------------------------------------------------
/// Notice: This file had been edited to integrate as core inside GeonBit.
/// Original license and attributes below. The license and copyright notice below affect
/// this file and this file only. https://github.com/tainicom/Aether.Extras
/// -------------------------------------------------------------------------------------
//   Copyright 2011-2016 Kastellanos Nikolaos
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
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Nez.ExtendedContent.GeonBit.Animation;
using Nez.ExtendedContent.GeonBit.Graphics;
using Nez.ExtendedContent.GeonBit.Serialization;

namespace Nez.ExtendedContent.GeonBit.Processors;

[ContentProcessor(DisplayName = "CPU AnimatedModel - GeonBit")]
internal class CpuAnimatedModelProcessor : DynamicModelProcessor, IContentProcessor
{
    // used to avoid creating clones/duplicates of the same VertexBufferContent
    private readonly Dictionary<VertexBufferContent, CpuAnimatedVertexBufferContent> _vertexBufferCache = new();
    private bool _fixRealBoneRoot;

    public CpuAnimatedModelProcessor()
    {
        VertexBufferType = DynamicModelContent.BufferType.DynamicWriteOnly;
        IndexBufferType = DynamicModelContent.BufferType.Default;
    }


    [DefaultValue(DynamicModelContent.BufferType.DynamicWriteOnly)]
    public new DynamicModelContent.BufferType VertexBufferType
    {
        get => base.VertexBufferType;
        set => base.VertexBufferType = value;
    }

    [DefaultValue(DynamicModelContent.BufferType.Default)]
    public new DynamicModelContent.BufferType IndexBufferType
    {
        get => base.IndexBufferType;
        set => base.IndexBufferType = value;
    }

#if !PORTABLE
    [DisplayName("MaxBones")]
#endif
    [DefaultValue(SkinnedEffect.MaxBones)]
    public virtual int MaxBones { get; set; } = SkinnedEffect.MaxBones;

#if !PORTABLE
    [DisplayName("Generate Keyframes Frequency")]
#endif
    [DefaultValue(0)] // (0=no, 30=30fps, 60=60fps)
    public virtual int GenerateKeyframesFrequency { get; set; }

#if !PORTABLE
    [DisplayName("Fix BoneRoot from MG importer")]
#endif
    [DefaultValue(false)]
    public virtual bool FixRealBoneRoot
    {
        get => _fixRealBoneRoot;
        set => _fixRealBoneRoot = value;
    }

    object IContentProcessor.Process(object input, ContentProcessorContext context)
    {
        var model = Process((NodeContent)input, context);
        var outputModel = new DynamicModelContent(model);

        foreach (var mesh in outputModel.Meshes)
        foreach (var part in mesh.MeshParts)
        {
            ProcessVertexBuffer(outputModel, context, part);
            ProcessIndexBuffer(outputModel, context, part);
        }

        // import animation
        var animationProcessor = new AnimationsProcessor
        {
            MaxBones = MaxBones,
            GenerateKeyframesFrequency = GenerateKeyframesFrequency,
            FixRealBoneRoot = _fixRealBoneRoot
        };
        var animation = animationProcessor.Process((NodeContent)input, context);
        outputModel.Tag = animation;

        //ProcessNode((NodeContent)input);

        return outputModel;
    }

    protected override void ProcessVertexBuffer(DynamicModelContent dynamicModel, ContentProcessorContext context,
        DynamicModelMeshPartContent part)
    {
        if (VertexBufferType != DynamicModelContent.BufferType.Default)
        {
            // Replace the default VertexBufferContent with CpuAnimatedVertexBufferContent.
            if (!_vertexBufferCache.TryGetValue(part.VertexBuffer, out var vb))
            {
                vb = new CpuAnimatedVertexBufferContent(part.VertexBuffer)
                {
                    IsWriteOnly = VertexBufferType == DynamicModelContent.BufferType.DynamicWriteOnly
                };
                _vertexBufferCache[part.VertexBuffer] = vb;
            }

            part.VertexBuffer = vb;
        }
    }
}