#region LICENSE

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
// A basic renderable bounding box.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit;

/// <summary>
///     Draw a bounding box.
///     Note: for debug purposes only, don't use in actual game.
/// </summary>
public class BoundingBoxEntity : BaseRenderableEntity
{
    // Initialize an array of indices for the box. 12 lines require 24 indices
    private static readonly short[] _bBoxIndices =
    [
        0, 1, 1, 2, 2, 3, 3, 0, // Front edges
        4, 5, 5, 6, 6, 7, 7, 4, // Back edges
        0, 4, 1, 5, 2, 6, 3, 7 // Side edges connecting front and back
    ];

    // drawing effect

    // vertex list (1 per box corner, total of 8 corners)
    private readonly VertexPositionColor[] _primitiveList = new VertexPositionColor[8];

    // bounding box to draw.
    private BoundingBox _boundingBox;

    /// <summary>
    ///     If true, it means bounding box is already transformed and we don't need to apply world matrix on it.
    /// </summary>
    public bool IsBoxAlreadyTransformed = true;

    /// <summary>
    ///     Create the bounding box entity.
    /// </summary>
    public BoundingBoxEntity()
    {
        // create effect
        BoxEffect = new BasicEffect(Core.GraphicsDevice)
        {
            TextureEnabled = false
        };
    }

    /// <summary>
    ///     Get / Set the bounding box to draw.
    /// </summary>
    public BoundingBox Box
    {
        // get bounding box
        get => _boundingBox;

        // set bounding box
        set
        {
            // only if changed, update bounding box
            if (_boundingBox == default || !_boundingBox.Equals(value))
            {
                _boundingBox = value;
                OnBoundingBoxUpdate();
            }
        }
    }

    /// <summary>
    ///     Get effect we draw box with.
    /// </summary>
    public BasicEffect BoxEffect { get; }

    /// <summary>
    ///     If true, this entity will only show in debug / editor mode.
    /// </summary>
    public override bool IsDebugEntity => true;

    /// <summary>
    ///     Called when bounding box changes.
    /// </summary>
    public void OnBoundingBoxUpdate()
    {
        // get bounding box corners
        var corners = Box.GetCorners();

        // Assign the 8 box vertices
        for (var i = 0; i < corners.Length; i++) _primitiveList[i] = new VertexPositionColor(corners[i], Color.White);
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
        // not visible / no active camera? skip
        if (!Visible || GeonDefaultRenderer.ActiveCamera == null) return;

        // set world / view / projection matrix
        BoxEffect.World = IsBoxAlreadyTransformed ? Matrix.Identity : worldTransformations;
        BoxEffect.View = GeonDefaultRenderer.ActiveCamera.View;
        BoxEffect.Projection = GeonDefaultRenderer.ActiveCamera.Projection;

        // Draw the box with a LineList
        for (var i = 0; i < BoxEffect.CurrentTechnique.Passes.Count; i++)
        {
            BoxEffect.CurrentTechnique.Passes[i].Apply();
            Core.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList, _primitiveList, 0, 8,
                _bBoxIndices, 0, 12);
        }
    }
}