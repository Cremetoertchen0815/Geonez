﻿#region License

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Nez.ExtendedContent.GeonBit.Animation;

namespace Nez.ExtendedContent.GeonBit.Processors;

[ContentProcessor(DisplayName = "Animation - GeonBit")]
internal class AnimationsProcessor : ContentProcessor<NodeContent, AnimationsContent>
{
    private bool _fixRealBoneRoot;

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

    public override AnimationsContent Process(NodeContent input, ContentProcessorContext context)
    {
        if (_fixRealBoneRoot)
            MGFixRealBoneRoot(input, context);

        ValidateMesh(input, context, null);

        // Find the skeleton.
        var skeleton = MeshHelper.FindSkeleton(input);

        if (skeleton == null)
            throw new InvalidContentException("Input skeleton not found.");

        // We don't want to have to worry about different parts of the model being
        // in different local coordinate systems, so let's just bake everything.
        FlattenTransforms(input, skeleton);

        // Read the bind pose and skeleton hierarchy data.
        var bones = MeshHelper.FlattenSkeleton(skeleton);

        if (bones.Count > MaxBones)
            throw new InvalidContentException(string.Format("Skeleton has {0} bones, but the maximum supported is {1}.",
                bones.Count, MaxBones));

        var bindPose = new List<Matrix>();
        var invBindPose = new List<Matrix>();
        var skeletonHierarchy = new List<int>();
        var boneNames = new List<string>();

        foreach (var bone in bones)
        {
            bindPose.Add(bone.Transform);
            invBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
            skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            boneNames.Add(bone.Name);
        }

        // Convert animation data to our runtime format.
        Dictionary<string, ClipContent> clips;
        clips = ProcessAnimations(input, context, skeleton.Animations, bones, GenerateKeyframesFrequency);

        return new AnimationsContent(bindPose, invBindPose, skeletonHierarchy, boneNames, clips);
    }

    /// <summary>
    ///     MonoGame converts some NodeContent into BoneContent.
    ///     Here we revert that to get the original Skeleton and
    ///     add the real boneroot to the root node.
    /// </summary>
    private void MGFixRealBoneRoot(NodeContent input, ContentProcessorContext context)
    {
        for (var i = input.Children.Count - 1; i >= 0; i--)
        {
            var node = input.Children[i];
            if (node is BoneContent &&
                node.AbsoluteTransform == Matrix.Identity &&
                node.Children.Count == 1 &&
                node.Children[0] is BoneContent &&
                node.Children[0].AbsoluteTransform == Matrix.Identity
               )
            {
                //dettach real boneRoot
                var realBoneRoot = node.Children[0];
                node.Children.RemoveAt(0);
                //copy animation from node to boneRoot
                foreach (var animation in node.Animations)
                    realBoneRoot.Animations.Add(animation.Key, animation.Value);
                // convert fake BoneContent back to NodeContent
                input.Children[i] = new NodeContent
                {
                    Name = node.Name,
                    Identity = node.Identity,
                    Transform = node.Transform
                };
                foreach (var animation in node.Animations)
                    input.Children[i].Animations.Add(animation.Key, animation.Value);
                foreach (var opaqueData in node.OpaqueData)
                    input.Children[i].OpaqueData.Add(opaqueData.Key, opaqueData.Value);
                //attach real boneRoot to the root node
                input.Children.Add(realBoneRoot);

                break;
            }
        }
    }

    /// <summary>
    ///     Makes sure this mesh contains the kind of data we know how to animate.
    /// </summary>
    private void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
    {
        var mesh = node as MeshContent;
        if (mesh != null)
        {
            // Validate the mesh.
            if (parentBoneName != null)
                context.Logger.LogWarning(null, null,
                    "Mesh {0} is a child of bone {1}. AnimatedModelProcessor " +
                    "does not correctly handle meshes that are children of bones.",
                    mesh.Name, parentBoneName);

            if (!MeshHasSkinning(mesh))
            {
                context.Logger.LogWarning(null, null,
                    "Mesh {0} has no skinning information, so it has been deleted.",
                    mesh.Name);

                mesh.Parent.Children.Remove(mesh);
                return;
            }
        }
        else if (node is BoneContent)
        {
            // If this is a bone, remember that we are now looking inside it.
            parentBoneName = node.Name;
        }

        // Recurse (iterating over a copy of the child collection,
        // because validating children may delete some of them).
        foreach (var child in new List<NodeContent>(node.Children))
            ValidateMesh(child, context, parentBoneName);
    }

    /// <summary>
    ///     Checks whether a mesh contains skininng information.
    /// </summary>
    private bool MeshHasSkinning(MeshContent mesh)
    {
        foreach (var geometry in mesh.Geometry)
            if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()) &&
                !geometry.Vertices.Channels.Contains("BlendWeight0"))
                return false;

        return true;
    }

    /// <summary>
    ///     Bakes unwanted transforms into the model geometry,
    ///     so everything ends up in the same coordinate system.
    /// </summary>
    private void FlattenTransforms(NodeContent node, BoneContent skeleton)
    {
        foreach (var child in node.Children)
        {
            // Don't process the skeleton, because that is special.
            if (child == skeleton)
                continue;

            // Bake the local transform into the actual geometry.
            MeshHelper.TransformScene(child, child.Transform);

            // Having baked it, we can now set the local
            // coordinate system back to identity.
            child.Transform = Matrix.Identity;

            // Recurse.
            FlattenTransforms(child, skeleton);
        }
    }

    /// <summary>
    ///     Converts an intermediate format content pipeline AnimationContentDictionary
    ///     object to our runtime AnimationClip format.
    /// </summary>
    private Dictionary<string, ClipContent> ProcessAnimations(NodeContent input, ContentProcessorContext context,
        AnimationContentDictionary animations, IList<BoneContent> bones, int generateKeyframesFrequency)
    {
        // Build up a table mapping bone names to indices.
        var boneMap = new Dictionary<string, int>();

        for (var i = 0; i < bones.Count; i++)
        {
            var boneName = bones[i].Name;

            if (!string.IsNullOrEmpty(boneName))
                boneMap.Add(boneName, i);
        }

        // Convert each animation in turn.
        Dictionary<string, ClipContent> animationClips;
        animationClips = new Dictionary<string, ClipContent>();

        foreach (var animation in animations)
        {
            var clip = ProcessAnimation(input, context, animation.Value, boneMap, generateKeyframesFrequency);

            animationClips.Add(animation.Key, clip);
        }

        if (animationClips.Count == 0)
            //throw new InvalidContentException("Input file does not contain any animations.");
            context.Logger.LogWarning(null, null, "Input file does not contain any animations.");

        return animationClips;
    }

    /// <summary>
    ///     Converts an intermediate format content pipeline AnimationContent
    ///     object to our runtime AnimationClip format.
    /// </summary>
    private ClipContent ProcessAnimation(NodeContent input, ContentProcessorContext context, AnimationContent animation,
        Dictionary<string, int> boneMap, int generateKeyframesFrequency)
    {
        var keyframes = new List<KeyframeContent>();

        // For each input animation channel.
        foreach (var channel in
                 animation.Channels)
        {
            // Look up what bone this channel is controlling.

            if (!boneMap.TryGetValue(channel.Key, out var boneIndex))
            {
                //throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));
                context.Logger.LogWarning(null, null,
                    "Found animation for bone '{0}', which is not part of the skeleton.", channel.Key);

                continue;
            }

            foreach (var keyframe in channel.Value)
                keyframes.Add(new KeyframeContent(boneIndex, keyframe.Time, keyframe.Transform));
        }

        // Sort the merged keyframes by time.
        keyframes.Sort(CompareKeyframeTimes);

        //System.Diagnostics.Debugger.Launch();
        if (generateKeyframesFrequency > 0)
            keyframes = InterpolateKeyframes(animation.Duration, keyframes, generateKeyframesFrequency);

        if (keyframes.Count == 0)
            throw new InvalidContentException("Animation has no keyframes.");

        if (animation.Duration <= TimeSpan.Zero)
            throw new InvalidContentException("Animation has a zero duration.");

        return new ClipContent(animation.Duration, keyframes.ToArray());
    }

    private int CompareKeyframeTimes(KeyframeContent a, KeyframeContent b)
    {
        var cmpTime = a.Time.CompareTo(b.Time);
        if (cmpTime == 0)
            return a.Bone.CompareTo(b.Bone);

        return cmpTime;
    }

    private List<KeyframeContent> InterpolateKeyframes(TimeSpan duration, List<KeyframeContent> keyframes,
        int generateKeyframesFrequency)
    {
        if (generateKeyframesFrequency <= 0)
            return keyframes;

        var keyframeCount = keyframes.Count;

        // find bones
        var bonesSet = new HashSet<int>();
        var maxBone = 0;
        for (var i = 0; i < keyframeCount; i++)
        {
            var bone = keyframes[i].Bone;
            maxBone = Math.Max(maxBone, bone);
            bonesSet.Add(bone);
        }

        var boneCount = bonesSet.Count;

        // split bones 
        var boneFrames = new List<KeyframeContent>[maxBone + 1];
        for (var i = 0; i < keyframeCount; i++)
        {
            var bone = keyframes[i].Bone;
            if (boneFrames[bone] == null) boneFrames[bone] = new List<KeyframeContent>();
            boneFrames[bone].Add(keyframes[i]);
        }

        //            
        Debug.WriteLine("Duration: " + duration);
        Debug.WriteLine("keyframeCount: " + keyframeCount);

        for (var b = 0; b < boneFrames.Length; b++)
        {
            var keySpan = TimeSpan.FromTicks((long)(1f / generateKeyframesFrequency * TimeSpan.TicksPerSecond));
            boneFrames[b] = InterpolateFramesBone(b, boneFrames[b], keySpan);
        }

        var frames = keyframeCount / boneCount;

        var checkDuration = TimeSpan.FromSeconds((frames - 1) / generateKeyframesFrequency);
        if (duration == checkDuration) return keyframes;

        var newKeyframes = new List<KeyframeContent>();
        for (var b = 0; b < boneFrames.Length; b++)
        for (var k = 0; k < boneFrames[b].Count; ++k)
            newKeyframes.Add(boneFrames[b][k]);

        newKeyframes.Sort(CompareKeyframeTimes);

        return newKeyframes;
    }

    private static List<KeyframeContent> InterpolateFramesBone(int bone, List<KeyframeContent> frames, TimeSpan keySpan)
    {
        Debug.WriteLine(string.Empty);
        Debug.WriteLine("Bone: " + bone);
        if (frames == null)
        {
            Debug.WriteLine("Frames: " + "null");
            return frames;
        }

        Debug.WriteLine("Frames: " + frames.Count);
        Debug.WriteLine("MinTime: " + frames[0].Time);
        Debug.WriteLine("MaxTime: " + frames[frames.Count - 1].Time);

        for (var i = 0; i < frames.Count - 1; ++i) InterpolateFrames(bone, frames, keySpan, i);

        return frames;
    }

    private static void InterpolateFrames(int bone, List<KeyframeContent> frames, TimeSpan keySpan, int i)
    {
        var a = i;
        var b = i + 1;
        var diff = frames[b].Time - frames[a].Time;
        if (diff > keySpan)
        {
            var newTime = frames[a].Time + keySpan;
            var amount = (float)(keySpan.TotalSeconds / diff.TotalSeconds);

            frames[a].Transform.Decompose(out var pScale, out var pRotation, out var pTranslation);

            frames[b].Transform.Decompose(out var iScale, out var iRotation, out var iTranslation);

            //lerp
            Vector3.Lerp(ref pScale, ref iScale, amount, out var Scale);
            Quaternion.Lerp(ref pRotation, ref iRotation, amount, out var Rotation);
            Vector3.Lerp(ref pTranslation, ref iTranslation, amount, out var Translation);

            Matrix.CreateFromQuaternion(ref Rotation, out var rotation);

            var newMatrix = new Matrix
            {
                M11 = Scale.X * rotation.M11,
                M12 = Scale.X * rotation.M12,
                M13 = Scale.X * rotation.M13,
                M14 = 0,
                M21 = Scale.Y * rotation.M21,
                M22 = Scale.Y * rotation.M22,
                M23 = Scale.Y * rotation.M23,
                M24 = 0,
                M31 = Scale.Z * rotation.M31,
                M32 = Scale.Z * rotation.M32,
                M33 = Scale.Z * rotation.M33,
                M34 = 0,
                M41 = Translation.X,
                M42 = Translation.Y,
                M43 = Translation.Z,
                M44 = 1
            };

            frames.Insert(b, new KeyframeContent(bone, newTime, newMatrix));
        }
    }
}