﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;

namespace BV.Game.Components.Debug;

/// <summary>
/// Allows the 3D camera to be moved around the scene freely with keyboard inputs(Keys A/D for X position, LShift/Space for Y position, W/S for Z position, Arrow keys for cam pan/tilt)
/// </summary>
internal class DebugCamMover : SceneComponent
{
    private VirtualJoystick _ctrlA;
    private VirtualJoystick _ctrlB;
    private VirtualAxis _ctrlC;
    private Camera3D _cam;

    public override void OnEnabled()
    {
        _ctrlA = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.A, Microsoft.Xna.Framework.Input.Keys.D, Microsoft.Xna.Framework.Input.Keys.W, Microsoft.Xna.Framework.Input.Keys.S));
        _ctrlB = new VirtualJoystick(true, new VirtualJoystick.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.Left, Microsoft.Xna.Framework.Input.Keys.Right, Microsoft.Xna.Framework.Input.Keys.Up, Microsoft.Xna.Framework.Input.Keys.Down));
        _ctrlC = new VirtualAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Microsoft.Xna.Framework.Input.Keys.LeftShift, Microsoft.Xna.Framework.Input.Keys.Space));
        _cam = ((GeonScene)Scene).Camera;
    }

    public override void OnDisabled()
    {
        _ctrlA.Deregister();
        _ctrlB.Deregister();
        _ctrlC.Deregister();
    }


    public override void Update()
    {
        _cam.Node.Position += new Vector3(_ctrlA.Value.X, _ctrlC.Value, _ctrlA.Value.Y);
        _cam.Node.Rotation += new Vector3(_ctrlB.Value.Y, _ctrlB.Value.X, 0) * -0.02f;
    }
}