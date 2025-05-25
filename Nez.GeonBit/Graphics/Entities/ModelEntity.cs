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
// A basic renderable model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.GeonBit.Graphics.Lights;
using Nez.GeonBit.Lights;
using Nez.GeonBit.Materials;

namespace Nez.GeonBit;

/// <summary>
///     A basic renderable model.
///     This type of model renderer renders the entire model as a single unit, and not as multiple meshes, but still
///     provide some control over materials etc.
/// </summary>
public class ModelEntity : BaseRenderableEntity, IShadowCaster
{
    /// <summary>
    ///     Dictionary with materials to use per meshes.
    ///     Key is mesh name, value is material to use for this mesh.
    /// </summary>
    private readonly Dictionary<string, MaterialAPI[]> _materials = new();

    // store last rendering radius (based on bounding sphere)
    private float _lastRadius;


    /// <summary>
    ///     Optional custom render settings for this specific instance.
    ///     Note: this method is much less efficient than materials override.
    /// </summary>
    public MaterialOverrides MaterialOverride = new();

    /// <summary>
    ///     Create the model entity from model instance.
    /// </summary>
    /// <param name="model">Model to draw.</param>
    public ModelEntity(Model model)
    {
        // store model
        Model = model;
    }

    /// <summary>
    ///     Create the model entity from asset path.
    /// </summary>
    /// <param name="path">Path of the model to load.</param>
    public ModelEntity(string path) : this(GeonDefaultRenderer.CurrentContentManager.Load<Model>(path))
    {
    }

    /// <summary>
    ///     Model to render.
    /// </summary>
    public Model Model { get; protected set; }

    /// <summary>
    ///     Should we process mesh parts?
    ///     This option is useful for inheriting types, it will iterate meshes before draw calls and call a virtual processing
    ///     function.
    /// </summary>
    protected virtual bool ProcessMeshParts => false;

    /// <summary>
    ///     Add bias to distance from camera when sorting by distance from camera.
    /// </summary>
    public override float CameraDistanceBias => _lastRadius * 100f;

    /// <summary>
    ///     Get materials dictionary.
    /// </summary>
    internal Dictionary<string, MaterialAPI[]> OverrideMaterialsDictionary => _materials;

    public int PrimaryLight { get; set; }
    public bool CastsShadow { get; set; }
    public int ShadowCasterLOD { get; set; }
    public RasterizerState ShadowRasterizerState { get; set; }

    void IShadowCaster.RenderShadows(Matrix worldTransform)
    {
        Model.Draw(LightsManager.ShadowEffect, worldTransform);
    }

    /// <summary>
    ///     Copy materials from another dictionary of materials.
    /// </summary>
    /// <param name="materials">Source materials to copy.</param>
    public void CopyMaterials(Dictionary<string, MaterialAPI[]> materials)
    {
        foreach (var pair in materials) _materials[pair.Key] = pair.Value;
    }

    /// <summary>
    ///     Set alternative material for a specific mesh id.
    /// </summary>
    /// <param name="material">Material to set.</param>
    /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
    public void SetMaterial(MaterialAPI material, string meshId = "")
    {
        _materials[meshId] = [material];
    }

    /// <summary>
    ///     Set alternative materials for a specific mesh id.
    /// </summary>
    /// <param name="material">
    ///     Materials to set (list where index is mesh-part index as value is material to use for this
    ///     part).
    /// </param>
    /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
    public void SetMaterials(MaterialAPI[] material, string meshId = "")
    {
        _materials[meshId] = material;
    }

    /// <summary>
    ///     Get material for a given mesh id.
    /// </summary>
    /// <param name="meshId">Mesh id to get material for.</param>
    /// <param name="meshPartIndex">MeshPart index to get material for.</param>
    public MaterialAPI GetMaterial(string meshId, int meshPartIndex = 0)
    {
        // material to return

        // try to get global material or material for this specific mesh
        if (_materials.TryGetValue(string.Empty, out var ret) || _materials.TryGetValue(meshId, out ret))
            // get material for effect index or null if overflow
            return meshPartIndex < ret.Length ? ret[meshPartIndex] : null;

        // if not found, return the default material attached to the mesh effect
        return Model.Meshes[meshId].MeshParts[meshPartIndex].GetDefaultMaterial();
    }

    /// <summary>
    ///     Return a list with all materials in model.
    ///     Note: if alternative materials are set, will return them.
    ///     Note2: prevent duplications, eg if even if more than one part uses the same material it will only return it once.
    /// </summary>
    /// <returns>List of materials.</returns>
    public List<MaterialAPI> GetMaterials()
    {
        var ret = new List<MaterialAPI>();
        for (var j = 0; j < Model.Meshes.Count; j++)
        {
            var mesh = Model.Meshes[j];
            for (var i = 0; i < mesh.MeshParts.Count; ++i)
            {
                var material = GetMaterial(mesh.Name, i);
                if (!ret.Contains(material)) ret.Add(material);
            }
        }

        return ret;
    }

    /// <summary>
    ///     Get the first material used in this renderer.
    /// </summary>
    /// <returns>List of materials.</returns>
    public MaterialAPI GetFirstMaterial()
    {
        return GetMaterial(Model.Meshes[0].Name);
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
        var scaleLen = Math3D.GetScale(ref worldTransformations).Length();

        // iterate model meshes
        for (var k = 0; k < Model.Meshes.Count; k++)
        {
            var mesh = Model.Meshes[k];

            var world = mesh.ParentBone.Transform * worldTransformations;

            // check if in this mesh we have shared materials, eg same effects used for several mesh parts
            var gotSharedEffects = mesh.Effects.Count != mesh.MeshParts.Count;

            // iterate over mesh parts
            var index = 0;
            for (var j = 0; j < mesh.MeshParts.Count; j++)
            {
                var meshPart = mesh.MeshParts[j];

                // get material for this mesh and effect index
                var material = GetMaterial(mesh.Name, index);

                // no material found? skip.
                // note: this can happen if user set alternative materials array with less materials than original mesh file
                if (material == null) break;

                // update per-entity override properties
                material = MaterialOverride.Apply(material);

                // if we don't have shared effects, eg every mesh part has its own effect, update material transformations
                if (!gotSharedEffects) material.Apply(ref world, ref _lastBoundingSphere, PrimaryLight);

                //Only change effect if really necessairy(every setting of the effect causes Monogame to internally generate enumerator objects)
                if (meshPart.Effect != material.Effect) meshPart.Effect = material.Effect;


                // next index.
                ++index;
            }

            // if we have shared effects, eg more than one mesh part with the same effect, we apply all materials here
            // this is to prevent applying the same material more than once
            if (gotSharedEffects)
                for (var i = 0; i < mesh.Effects.Count; i++)
                    mesh.Effects[i].GetMaterial().Apply(ref world, ref _lastBoundingSphere, PrimaryLight);

            // update last radius
            _lastRadius = Math.Max(_lastRadius, mesh.BoundingSphere.Radius * scaleLen);

            // iterate mesh parts
            if (ProcessMeshParts)
                for (var i = 0; i < mesh.MeshParts.Count; i++)
                    // call the before-drawing-mesh-part callback
                    BeforeDrawingMeshPart(mesh.MeshParts[i]);

            // draw the mesh itself
            mesh.Draw();
        }
    }

    /// <summary>
    ///     Called before drawing each mesh part.
    ///     This is useful to extend this model with animations etc.
    /// </summary>
    /// <param name="part">Mesh part we are about to draw.</param>
    protected virtual void BeforeDrawingMeshPart(ModelMeshPart part)
    {
    }

    /// <summary>
    ///     Get the bounding sphere of this entity.
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
        var modelBoundingSphere = ModelUtils.GetBoundingSphere(Model);
        modelBoundingSphere.Radius *= Math3D.GetScale(ref worldTransformations).Length();
        modelBoundingSphere.Center = worldTransformations.Translation;
        return modelBoundingSphere;
    }

    /// <summary>
    ///     Get the bounding box of this entity.
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
        // get bounding box in local space
        var modelBoundingBox = ModelUtils.GetBoundingBox(Model);

        // initialize minimum and maximum corners of the bounding box to max and min values
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // iterate bounding box corners and transform them
        var corners = modelBoundingBox.GetCorners();

        for (var i = 0; i < corners.Length; i++)
        {
            var corner = corners[i];

            // get curr position and update min / max
            var currPosition = Vector3.Transform(corner, worldTransformations);
            min = Vector3.Min(min, currPosition);
            max = Vector3.Max(max, currPosition);
        }

        // create and return transformed bounding box
        return new BoundingBox(min, max);
    }
}