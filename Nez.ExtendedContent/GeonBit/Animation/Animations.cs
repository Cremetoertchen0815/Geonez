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
using Microsoft.Xna.Framework;

namespace Nez.ExtendedContent.GeonBit.Animation;

public class Animations
{
    // transformation arrays

    // TODO: convert those from List<T> to simple T[] arrays.
    internal List<Matrix> _bindPose;
    internal Dictionary<string, int> _boneMap;

    // current clip name

    // current key frame.
    private int _currentKeyframe;
    internal List<Matrix> _invBindPose;
    internal List<int> _skeletonHierarchy;

    /// <summary>
    ///     Create the animations instance.
    /// </summary>
    /// <param name="bindPose"></param>
    /// <param name="invBindPose"></param>
    /// <param name="skeletonHierarchy"></param>
    /// <param name="boneMap"></param>
    /// <param name="clips"></param>
    internal Animations(List<Matrix> bindPose, List<Matrix> invBindPose, List<int> skeletonHierarchy,
        Dictionary<string, int> boneMap, Dictionary<string, Clip> clips)
    {
        // store params
        _bindPose = bindPose;
        _invBindPose = invBindPose;
        _skeletonHierarchy = skeletonHierarchy;
        _boneMap = boneMap;
        Clips = clips;

        // initialize
        BoneTransforms = new Matrix[_bindPose.Count];
        WorldTransforms = new Matrix[_bindPose.Count];
        AnimationTransforms = new Matrix[_bindPose.Count];

        // set default clip to first clip
        var keys = Clips.Keys.GetEnumerator();
        if (!keys.MoveNext()) throw new Exception("Cannot load skinned model without any animations!");
        SetClip(keys.Current);
    }

    /// <summary>
    ///     Get the currently playing clip name.
    /// </summary>
    public string CurrentClipName { get; private set; }

    /// <summary>
    ///     Clips dictionary.
    /// </summary>
    public Dictionary<string, Clip> Clips { get; }

    /// <summary>
    ///     Current clip playing.
    /// </summary>
    public Clip CurrentClip { get; private set; }

    /// <summary>
    ///     Current time in clip.
    /// </summary>
    public float CurrentTime { get; private set; }

    /// <summary>
    ///     This flag is true on the frame the animation cycle ended.
    ///     On all other update frames, its false.
    /// </summary>
    public bool HasEnded { get; private set; }

    /// <summary>
    ///     The current bone transform matrices, relative to their parent bones.
    /// </summary>
    public Matrix[] BoneTransforms { get; }

    /// <summary>
    ///     The current bone transform matrices, in absolute format.
    /// </summary>
    public Matrix[] WorldTransforms { get; }

    /// <summary>
    ///     The current bone transform matrices, relative to the animation bind pose.
    /// </summary>
    public Matrix[] AnimationTransforms { get; }

    /// <summary>
    ///     Clone animations.
    /// </summary>
    /// <returns>Cloned animations instance.</returns>
    public Animations Clone()
    {
        return new Animations(_bindPose, _invBindPose, _skeletonHierarchy, _boneMap, Clips);
    }

    /// <summary>
    ///     Set currently playing clip.
    /// </summary>
    /// <param name="clipName">Clip identifier.</param>
    public void SetClip(string clipName)
    {
        var clip = Clips[clipName];
        SetClip(clip);
        CurrentClipName = clipName;
    }

    /// <summary>
    ///     Set currently playing clip instance.
    /// </summary>
    /// <param name="clip">Clip instance.</param>
    private void SetClip(Clip clip)
    {
        if (clip == null)
            throw new ArgumentNullException("clip");

        CurrentClip = clip;
        CurrentTime = 0;
        _currentKeyframe = 0;

        // Initialize bone transforms to the bind pose.
        _bindPose.CopyTo(BoneTransforms, 0);
    }

    /// <summary>
    ///     Clear currently played clip.
    /// </summary>
    public void ClearClip()
    {
        CurrentClipName = null;
        CurrentClip = null;
    }

    /// <summary>
    ///     Get bone index from name.
    /// </summary>
    /// <param name="boneName">Bone name to get index for.</param>
    /// <returns>Bone index.</returns>
    public int GetBoneIndex(string boneName)
    {
        if (!_boneMap.TryGetValue(boneName, out var boneIndex)) boneIndex = -1;
        return boneIndex;
    }

    /// <summary>
    ///     Update animation step.
    /// </summary>
    /// <param name="time">How much to advance animation (time, in seconds, since last frame).</param>
    /// <param name="relativeToCurrentTime">If true, it means the time provided is relative from current time and not absolute.</param>
    /// <param name="rootTransform">Root transformations.</param>
    public void Update(float time, bool relativeToCurrentTime, Matrix rootTransform)
    {
        UpdateBoneTransforms(time, relativeToCurrentTime);
        UpdateWorldTransforms(rootTransform);
        UpdateAnimationTransforms();
    }

    /// <summary>
    ///     Update current bone transforms based on animation time, and advance animation.
    /// </summary>
    /// <param name="time">Time since last update (in seconds).</param>
    /// <param name="relativeToCurrentTime">If true, it means the time provided is relative from current time and not absolute.</param>
    public void UpdateBoneTransforms(float time, bool relativeToCurrentTime)
    {
        // reset the HasEnded flag
        HasEnded = false;

        // Update the animation position.
        if (relativeToCurrentTime)
        {
            // advance current time
            time += CurrentTime;

            // If we reached the end, loop back to the start.
            while (time >= CurrentClip.Duration.TotalSeconds)
            {
                time -= (float)CurrentClip.Duration.TotalSeconds;
                HasEnded = true;
            }
        }

        // assert on illegal time
        if (time < 0) throw new ArgumentOutOfRangeException("time out of range");

        // if got value too big, clip to animation duration
        if (time > CurrentClip.Duration.TotalSeconds) time = (float)CurrentClip.Duration.TotalSeconds;

        // If the position moved backwards, reset the keyframe index.
        if (time < CurrentTime)
        {
            _currentKeyframe = 0;
            _bindPose.CopyTo(BoneTransforms, 0);
        }

        // set current time
        CurrentTime = time;

        // Read keyframe matrices.
        IList<Keyframe> keyframes = CurrentClip.Keyframes;

        while (_currentKeyframe < keyframes.Count)
        {
            var keyframe = keyframes[_currentKeyframe];

            // Stop when we've read up to the current time position.
            if (keyframe.Time.TotalSeconds > CurrentTime)
                break;

            // Use this keyframe.
            BoneTransforms[keyframe.Bone] = keyframe.Transform;

            _currentKeyframe++;
        }
    }

    /// <summary>
    ///     Update world transformations (based on root transform).
    /// </summary>
    /// <param name="rootTransform"></param>
    public void UpdateWorldTransforms(Matrix rootTransform)
    {
        // Root bone.
        Matrix.Multiply(ref BoneTransforms[0], ref rootTransform, out WorldTransforms[0]);

        // Child bones.
        for (var bone = 1; bone < WorldTransforms.Length; bone++)
        {
            var parentBone = _skeletonHierarchy[bone];

            Matrix.Multiply(ref BoneTransforms[bone], ref WorldTransforms[parentBone], out WorldTransforms[bone]);
        }
    }

    /// <summary>
    ///     Update bones positions.
    /// </summary>
    public void UpdateAnimationTransforms()
    {
        for (var bone = 0; bone < AnimationTransforms.Length; bone++)
        {
            var _tmpInvBindPose = _invBindPose[bone]; //can not pass it as 'ref'
            Matrix.Multiply(ref _tmpInvBindPose, ref WorldTransforms[bone], out AnimationTransforms[bone]);
        }
    }
}