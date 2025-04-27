using System;
using System.Collections.Generic;
using Nez.Textures;

namespace Nez.Sprites;

/// <summary>
///     SpriteAnimator handles the display and animation of a sprite
/// </summary>
public class SpriteAnimator : SpriteRenderer, IUpdatable
{
    public enum LoopMode
    {
	    /// <summary>
	    ///     Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
	    /// </summary>
	    Loop,

	    /// <summary>
	    ///     Play the sequence once [A][B][C] then pause and set time to 0 [A]
	    /// </summary>
	    Once,

	    /// <summary>
	    ///     Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never
	    ///     stop playing
	    /// </summary>
	    ClampForever,

	    /// <summary>
	    ///     Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
	    /// </summary>
	    PingPong,

	    /// <summary>
	    ///     Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
	    /// </summary>
	    PingPongOnce
    }

    public enum State
    {
        None,
        Running,
        Paused,
        Completed
    }

    private float _elapsedTime;
    private LoopMode _loopMode;

    /// <summary>
    ///     animation playback speed
    /// </summary>
    public float Speed = 1;


    public SpriteAnimator()
    {
    }

    public SpriteAnimator(Sprite sprite)
    {
        SetSprite(sprite);
    }

    /// <summary>
    ///     the current state of the animation
    /// </summary>
    public State AnimationState { get; private set; } = State.None;

    /// <summary>
    ///     the current animation
    /// </summary>
    public SpriteAnimation CurrentAnimation { get; private set; }

    /// <summary>
    ///     the name of the current animation
    /// </summary>
    public string CurrentAnimationName { get; private set; }

    /// <summary>
    ///     index of the current frame in sprite array of the current animation
    /// </summary>
    public int CurrentFrame { get; set; }

    /// <summary>
    ///     checks to see if the CurrentAnimation is running
    /// </summary>
    public bool IsRunning => AnimationState == State.Running;

    /// <summary>
    ///     Provides access to list of available animations
    /// </summary>
    public Dictionary<string, SpriteAnimation> Animations { get; } = new();

    public virtual void Update()
    {
        if (AnimationState != State.Running || CurrentAnimation == null)
            return;

        var animation = CurrentAnimation;
        var secondsPerFrame = 1 / (animation.FrameRate * Speed);
        var iterationDuration = secondsPerFrame * animation.Sprites.Length;
        var pingPongIterationDuration = animation.Sprites.Length < 3
            ? iterationDuration
            : secondsPerFrame * (animation.Sprites.Length * 2 - 2);

        _elapsedTime += Time.DeltaTime;
        var time = Math.Abs(_elapsedTime);

        // Once and PingPongOnce reset back to Time = 0 once they complete
        if ((_loopMode == LoopMode.Once && time > iterationDuration) ||
            (_loopMode == LoopMode.PingPongOnce && time > pingPongIterationDuration))
        {
            AnimationState = State.Completed;
            _elapsedTime = 0;
            CurrentFrame = 0;
            Sprite = animation.Sprites[0];
            OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
            return;
        }

        if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
        {
            AnimationState = State.Completed;
            CurrentFrame = animation.Sprites.Length - 1;
            Sprite = animation.Sprites[CurrentFrame];
            OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
            return;
        }

        // figure out which frame we are on
        var i = Mathf.FloorToInt(time / secondsPerFrame);
        var n = animation.Sprites.Length;
        if (n > 2 && (_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce))
        {
            // create a pingpong frame
            var maxIndex = n - 1;
            CurrentFrame = maxIndex - Math.Abs(maxIndex - i % (maxIndex * 2));
        }
        else
        {
            // create a looping frame
            CurrentFrame = i % n;
        }

        Sprite = animation.Sprites[CurrentFrame];
    }

    /// <summary>
    ///     fired when an animation completes, includes the animation name;
    /// </summary>
    public event Action<string> OnAnimationCompletedEvent;

    /// <summary>
    ///     adds all the animations from the SpriteAtlas
    /// </summary>
    public SpriteAnimator AddAnimationsFromAtlas(SpriteAtlas atlas)
    {
        for (var i = 0; i < atlas.AnimationNames.Length; i++)
            Animations.Add(atlas.AnimationNames[i], atlas.SpriteAnimations[i]);
        return this;
    }


    /// <summary>
    ///     adds all the sprites & animations from the SpriteAtlas
    /// </summary>
    public SpriteAnimator AddEverythingFromAtlas(SpriteAtlas atlas)
    {
        //Add sprite animations
        for (var i = 0; i < atlas.AnimationNames.Length; i++)
            Animations.Add(atlas.AnimationNames[i], atlas.SpriteAnimations[i]);

        //Add non-sprite animations
        foreach (var element in atlas.NonAnimationSprites)
            Animations.Add(atlas.Names[element], new SpriteAnimation(new[] { atlas.Sprites[element] }, 1));
        return this;
    }

    /// <summary>
    ///     Adds a SpriteAnimation
    /// </summary>
    public SpriteAnimator AddAnimation(string name, SpriteAnimation animation)
    {
        // if we have no sprite use the first frame we find
        if (Sprite == null && animation.Sprites.Length > 0)
            SetSprite(animation.Sprites[0]);
        Animations[name] = animation;
        return this;
    }

    public SpriteAnimator AddAnimation(string name, Sprite[] sprites, float fps = 10)
    {
        return AddAnimation(name, fps, sprites);
    }

    public SpriteAnimator AddAnimation(string name, float fps, params Sprite[] sprites)
    {
        AddAnimation(name, new SpriteAnimation(sprites, fps));
        return this;
    }

    #region Playback

    /// <summary>
    ///     plays the animation with the given name. If no loopMode is specified it is defaults to Loop
    /// </summary>
    public void Play(string name, LoopMode? loopMode = null)
    {
        CurrentAnimation = Animations[name];
        CurrentAnimationName = name;
        CurrentFrame = 0;
        AnimationState = State.Running;

        Sprite = CurrentAnimation.Sprites[0];
        _elapsedTime = 0;
        _loopMode = loopMode ?? LoopMode.Loop;
    }

    /// <summary>
    ///     checks to see if the animation is playing (i.e. the animation is active. it may still be in the paused state)
    /// </summary>
    public bool IsAnimationActive(string name)
    {
        return CurrentAnimation != null && CurrentAnimationName.Equals(name);
    }

    /// <summary>
    ///     pauses the animator
    /// </summary>
    public void Pause()
    {
        AnimationState = State.Paused;
    }

    /// <summary>
    ///     unpauses the animator
    /// </summary>
    public void UnPause()
    {
        AnimationState = State.Running;
    }

    /// <summary>
    ///     stops the current animation and nulls it out
    /// </summary>
    public void Stop()
    {
        CurrentAnimation = null;
        CurrentAnimationName = null;
        CurrentFrame = 0;
        AnimationState = State.None;
    }

    #endregion
}