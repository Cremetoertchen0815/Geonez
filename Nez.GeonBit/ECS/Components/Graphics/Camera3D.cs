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
// A 3d camera component.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;

namespace Nez.GeonBit;

/// <summary>
///     Camera types.
/// </summary>
public enum CameraType
{
    /// <summary>
    ///     Perspective camera.
    /// </summary>
    Perspective,

    /// <summary>
    ///     Orthographic camera.
    /// </summary>
    Orthographic
}

/// <summary>
///     This component implements a 3d camera.
/// </summary>
public class Camera3D : GeonComponent, IUpdatable
{
    /// <summary>
    ///     Default field of view.
    /// </summary>
    public static readonly float DefaultFieldOfView = MathHelper.PiOver4;

    // camera screen size
    private float _aspectRatio = 1.0f;

    // current camera type
    private CameraType _cameraType = CameraType.Perspective;
    private float _farClipPlane = 950.0f;

    // projection params
    private float _fieldOfView = MathHelper.PiOver4;
    private float _nearClipPlane = 1.0f;

    // true if we need to update projection matrix next time we try to get it
    private bool _needUpdateProjection = true;

    // current world position

    // current projection matrix
    private Matrix _projection;

    // current view matrix

    /// <summary>
    ///     Does this camera auto-update on update loop?
    ///     Note: if you turn this false you must call Update() manually.
    /// </summary>
    public bool AutoUpdate = true;

    /// <summary>
    ///     Set a target that the camera will always look at, regardless of scene node rotation.
    ///     Note: this override the LookAt position, even if set.
    /// </summary>
    public GeonEntity LookAtTarget;

    /// <summary>
    ///     If 'LookAtTarget' is used, this vector will be offset from target position.
    ///     For example, if you want the camera to look at 5 units above target, set this to Vector3(0, 5, 0).
    /// </summary>
    public Vector3 LookAtTargetOffset = Vector3.Zero;

    public Vector3? OverridePosition = null;

    /// <summary>
    ///     If defined, this will be used as screen size (affect aspect ratio in perspective camera,
    ///     and view size in Orthographic camera). If not set, the actual screen resolution will be used.
    /// </summary>
    public Point? ForceScreenSize { get; set; }

    /// <summary>
    ///     Set / get camera type.
    /// </summary>
    public CameraType CameraType
    {
        set
        {
            _cameraType = value;
            _needUpdateProjection = true;
        }
        get => _cameraType;
    }

    /// <summary>
    ///     Set / Get camera field of view.
    /// </summary>
    public float FieldOfView
    {
        get => _fieldOfView;
        set
        {
            _fieldOfView = value;
            _needUpdateProjection = true;
        }
    }

    /// <summary>
    ///     Set / Get camera near clip plane.
    /// </summary>
    public float NearClipPlane
    {
        get => _nearClipPlane;
        set
        {
            _nearClipPlane = value;
            _needUpdateProjection = true;
        }
    }

    /// <summary>
    ///     Set / Get camera far clip plane.
    /// </summary>
    public float FarClipPlane
    {
        get => _farClipPlane;
        set
        {
            _farClipPlane = value;
            _needUpdateProjection = true;
        }
    }

    /// <summary>
    ///     Get camera position.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    ///     Return the current camera projection matrix.
    /// </summary>
    public Matrix Projection
    {
        get
        {
            UpdateProjectionIfNeeded();
            return _projection;
        }
    }

    /// <summary>
    ///     Get / Set the current camera view matrix.
    /// </summary>
    public Matrix View { get; private set; }

    /// <summary>
    ///     Get camera forward vector.
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            var inv = Matrix.Invert(View);
            var ret = new Vector3(inv.M31, inv.M32, inv.M33);
            return -ret;
        }
    }

    /// <summary>
    ///     Get camera backward vector.
    /// </summary>
    public Vector3 Backward
    {
        get
        {
            var ret = Vector3.Transform(Vector3.Forward, Matrix.Invert(View));
            ret.Normalize();
            return ret;
        }
    }

    /// <summary>
    ///     Get camera bounding frustum.
    /// </summary>
    public BoundingFrustum ViewFrustum
    {
        get
        {
            UpdateProjectionIfNeeded();
            return new BoundingFrustum(View * _projection);
        }
    }

    /// <summary>
    ///     If set, camera will always look at this point, regardless of scene node rotation.
    /// </summary>
    public Vector3? LookAt { get; set; }

    /// <summary>
    ///     Get if this camera is the active camera in its scene.
    ///     Note: it doesn't mean that the scene this camera belongs to is currently active.
    /// </summary>
    public bool IsActiveCamera => GeonDefaultRenderer.ActiveCamera == this;

    /// <summary>
    ///     Called every frame in the Update() loop.
    ///     Note: this is called only if GameObject is enabled.
    /// </summary>
    public void Update()
    {
        // if we are the currently active camera, update view matrix
        if (IsActiveCamera && AutoUpdate)
            // update camera view
            UpdateCameraView();
    }

    /// <summary>
    ///     Store camera world position.
    /// </summary>
    /// <param name="view">Current view matrix</param>
    /// <param name="position">Camera world position.</param>
    public void UpdateViewPosition(Matrix view, Vector3 position)
    {
        View = view;
        Position = position;
    }

    /// <summary>
    ///     Update projection matrix after changes.
    /// </summary>
    private void UpdateProjectionIfNeeded()
    {
        // if don't need update, skip
        if (!_needUpdateProjection) return;

        // screen width and height

        // calc aspect ratio
        _aspectRatio = (float)Screen.Width / Screen.Height;

        // create view and projection matrix
        switch (_cameraType)
        {
            case CameraType.Perspective:
                _projection =
                    Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearClipPlane, _farClipPlane);
                break;

            case CameraType.Orthographic:
                _projection = Matrix.CreateOrthographic(Screen.Width, Screen.Height, _nearClipPlane, _farClipPlane);
                break;
        }

        // no longer need projection update
        _needUpdateProjection = false;
    }

    public override void OnAddedToEntity()
    {
        // if there's no active camera, set self as the active camera
        if (GeonDefaultRenderer.ActiveCamera == null) SetAsActive();
    }

    /// <summary>
    ///     Return a ray starting from the camera and pointing directly at mouse position (translated to 3d space).
    ///     This is a helper function that help to get ray collision based on camera and mouse.
    /// </summary>
    /// <returns>Ray from camera to mouse.</returns>
    public Ray RayFromMouse()
    {
        return RayFrom2dPoint(Input.MousePosition);
    }

    /// <summary>
    ///     Return a ray starting from the camera and pointing directly at a 3d position.
    /// </summary>
    /// <param name="point">Point to send ray to.</param>
    /// <returns>Ray from camera to given position.</returns>
    public Ray RayFrom3dPoint(Vector3 point)
    {
        return new Ray(Position, point - Position);
    }

    /// <summary>
    ///     Return a ray starting from the camera and pointing directly at a 2d position translated to 3d space.
    ///     This is a helper function that help to get ray collision based on camera and position on screen.
    /// </summary>
    /// <param name="point">Point to send ray to.</param>
    /// <returns>Ray from camera to given position.</returns>
    public Ray RayFrom2dPoint(Vector2 point)
    {
        // get graphic device
        var device = Core.GraphicsDevice;

        // convert point to near and far points as 3d vectors
        var nearsource = new Vector3(point.X, point.Y, 0f);
        var farsource = new Vector3(point.X, point.Y, 1f);

        // create empty world matrix
        var world = Matrix.CreateTranslation(0, 0, 0);

        // convert near point to world space
        var nearPoint = device.Viewport.Unproject(nearsource,
            _projection, View, world);

        // convert far point to world space
        var farPoint = device.Viewport.Unproject(farsource,
            _projection, View, world);

        // get direction
        var dir = farPoint - nearPoint;
        dir.Normalize();

        // return ray
        return new Ray(nearPoint, dir);
    }

    /// <summary>
    ///     Clone this component.
    /// </summary>
    /// <returns>Cloned copy of this component.</returns>
    public override Component Clone()
    {
        var ret = (Camera3D)base.Clone();
        ret.LookAt = LookAt;
        ret.LookAtTarget = LookAtTarget;
        ret.LookAtTargetOffset = LookAtTargetOffset;
        ret.CameraType = CameraType;
        ret.ForceScreenSize = ForceScreenSize;
        ret.FarClipPlane = FarClipPlane;
        ret.NearClipPlane = NearClipPlane;
        ret.FieldOfView = FieldOfView;
        ret.AutoUpdate = AutoUpdate;
        return ret;
    }

    /// <summary>
    ///     Get the 3d ray that starts from camera position and directed at current mouse position.
    /// </summary>
    /// <returns>Ray from camera to mouse position.</returns>
    public Ray GetMouseRay()
    {
        return RayFromMouse();
    }

    /// <summary>
    ///     Get the 3d ray that starts from camera position and directed at a given 2d position.
    /// </summary>
    /// <param name="position">Position to get ray to.</param>
    /// <returns>Ray from camera to given position.</returns>
    public Ray GetRay(Vector2 position)
    {
        return RayFrom2dPoint(position);
    }

    /// <summary>
    ///     Get the 3d ray that starts from camera position and directed at a given 3d position.
    /// </summary>
    /// <param name="position">Position to get ray to.</param>
    /// <returns>Ray from camera to given position.</returns>
    public Ray GetRay(Vector3 position)
    {
        return RayFrom3dPoint(position);
    }


    /// <summary>
    ///     Set this camera as the currently active camera.
    /// </summary>
    public void SetAsActive()
    {
        // if not in scene, throw exception
        //if (_GameObject == null || _GameObject.ParentScene == null)
        //{
        //    throw new System.InvalidOperationException("Cannot make a camera active when its not under any scene!");
        //}
        // update core graphics about new active camera
        GeonDefaultRenderer.ActiveCamera = this;
    }

    /// <summary>
    ///     Update camera view matrix.
    /// </summary>
    public void UpdateCameraView()
    {
        // if there's a lookat target, override current LookAt
        if (LookAtTarget != null) LookAt = LookAtTarget.Node.WorldPosition + LookAtTargetOffset;

        // new view matrix
        Matrix view;

        // get current world position (of the camera)
        var worldPos = Node.WorldPosition;

        var source = OverridePosition ?? worldPos;
        var target = LookAt ?? source + Vector3.Transform(Vector3.Forward, Node.WorldRotation);
        view = Matrix.CreateLookAt(source, target, Vector3.Up);

        // update the view matrix of the graphic camera component
        UpdateViewPosition(view, worldPos);
    }
}