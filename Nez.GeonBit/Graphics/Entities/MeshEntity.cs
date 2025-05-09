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
// A basic renderable mesh.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Lights;
using Nez.GeonBit.Materials;

namespace Nez.GeonBit;

/// <summary>
///     A basic renderable mesh (part of a model).
/// </summary>
public class MeshEntity : BaseRenderableEntity, IShadowCaster
{
    // store last rendering radius (based on bounding sphere)
    private float _lastRadius;

    /// <summary>
    ///     Optional array of materials to use instead of the mesh default materials.
    /// </summary>
    private MaterialAPI[] _materials;

    /// <summary>
    ///     Optional custom render settings for this specific instance.
    ///     Note: this method is much less efficient than materials override.
    /// </summary>
    public MaterialOverrides MaterialOverride = new();

    /// <summary>
    ///     Create the mesh entity from mesh instance.
    /// </summary>
    /// <param name="model">Model to draw.</param>
    /// <param name="mesh">Specific mesh in model to draw.</param>
    public MeshEntity(Model model, ModelMesh mesh)
    {
        Model = model;
        Mesh = mesh;
    }

    /// <summary>
    ///     Mesh to render.
    /// </summary>
    public ModelMesh Mesh { get; protected set; }

    /// <summary>
    ///     Mesh's parent model.
    /// </summary>
    public Model Model { get; protected set; }

    /// <summary>
    ///     Add bias to distance from camera when sorting by distance from camera.
    /// </summary>
    public override float CameraDistanceBias => _lastRadius * 100f;

    /// <summary>
    ///     Get materials dictionary.
    /// </summary>
    internal MaterialAPI[] OverrideMaterials => _materials;

    public int PrimaryLight { get; set; }
    public bool CastsShadow { get; set; }
    public int ShadowCasterLOD { get; set; }
    public RasterizerState ShadowRasterizerState { get; set; }

    void IShadowCaster.RenderShadows(Matrix worldTransform)
    {
        Mesh.Draw(LightsManager.ShadowEffect, worldTransform);
    }

    /// <summary>
    ///     Set first alternative material for this mesh (useful for meshes with one effect).
    /// </summary>
    /// <param name="material">Material to set.</param>
    public void SetMaterial(MaterialAPI material)
    {
        _materials = new[] { material };
    }

    /// <summary>
    ///     Set alternative array of materials for this mesh.
    ///     Will replace mesh original materials.
    /// </summary>
    /// <param name="materials">Materials array to set.</param>
    public void SetMaterials(MaterialAPI[] materials)
    {
        _materials = materials;
    }

    /// <summary>
    ///     Get material for a given mesh id.
    /// </summary>
    /// <param name="meshPartIndex">MeshPart index to get material for.</param>
    public MaterialAPI GetMaterial(int meshPartIndex = 0)
    {
        // if we got alternative materials array defined, check if we got alternative material for this effect index
        if (_materials != null)
            // get material for effect index or null if overflow
            return meshPartIndex < _materials.Length ? _materials[meshPartIndex] : null;

        // if not found, return the default material attached to the default mesh effect (via 'Tag')
        return Mesh.MeshParts[meshPartIndex].GetDefaultMaterial();
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
        // call base draw entity
        base.DoEntityDraw(ref worldTransformations);

        // reset last radius
        _lastRadius = 0f;
        var scale = Math3D.GetScale(ref worldTransformations);
        var scaleLen = scale.Length();

        // check if in this mesh we have shared materials, eg same effects used for several mesh parts
        var gotSharedEffects = Mesh.Effects.Count != Mesh.MeshParts.Count;

        // iterate over mesh parts
        var index = 0;
        for (var i = 0; i < Mesh.MeshParts.Count; i++)
        {
            var meshPart = Mesh.MeshParts[i];
            // get material for this mesh and effect index
            var material = GetMaterial(index);

            // no material found? skip.
            // note: this can happen if user set alternative materials array with less materials than original mesh file
            if (material == null) break;

            // update per-entity override properties
            material = MaterialOverride.Apply(material);

            // if we don't have shared effects, eg every mesh part has its own effect, update material transformations
            if (!gotSharedEffects) material.Apply(ref worldTransformations, ref _lastBoundingSphere, PrimaryLight);

            // apply material effect on the mesh part. note: we first store original effect in mesh part's tag.
            if (meshPart.Tag == null) meshPart.Tag = meshPart.Effect;
            if (meshPart.Effect != material.Effect) meshPart.Effect = material.Effect;

            // next index.
            ++index;
        }

        // if we have shared effects, eg more than one mesh part with the same effect, we apply all materials here
        // this is to prevent applying the same material more than once
        if (gotSharedEffects)
            for (var i = 0; i < Mesh.Effects.Count; i++)
            {
                var effect = Mesh.Effects[i];
                effect.GetMaterial().Apply(ref worldTransformations, ref _lastBoundingSphere, PrimaryLight);
            }

        // update last radius
        _lastRadius = Math.Max(_lastRadius, Mesh.BoundingSphere.Radius * scaleLen);

        // draw the mesh itself
        Mesh.Draw();
    }

    /// <summary>
    ///     Get the bounding sphere of this mesh.
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
        var modelBoundingSphere = Mesh.BoundingSphere;
        modelBoundingSphere.Radius *= Math3D.GetScale(ref worldTransformations).Length();
        modelBoundingSphere.Center = worldTransformations.Translation;
        return modelBoundingSphere;
    }

    /// <summary>
    ///     Get the bounding box of this mesh.
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
        return BoundingBox.CreateFromSphere(GetBoundingSphere(parent, ref localTransformations,
            ref worldTransformations));
    }
}