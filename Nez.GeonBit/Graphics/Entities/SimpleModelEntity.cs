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
// A simpler renderable model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.GeonBit
{

	/// <summary>
	/// A basic renderable model with minimum control over materials and meshes, but with best performance compared to other
	/// model renderer types.
	/// Use this class if you want lots of models of the same type that don't require any special properties.
	/// </summary>
	public class SimpleModelEntity : BaseRenderableEntity
	{
		/// <summary>
		/// Model to render.
		/// </summary>
		public Model Model
		{
			get; protected set;
		}

		/// <summary>
		/// Add bias to distance from camera when sorting by distance from camera.
		/// </summary>
		public override float CameraDistanceBias => _lastRadius * 100f;

		// store last rendering radius (based on bounding sphere)
		private float _lastRadius = 0f;

		/// <summary>
		/// Create the model entity from model instance.
		/// </summary>
		/// <param name="model">Model to draw.</param>
		public SimpleModelEntity(Model model) => Model = model;

		/// <summary>
		/// Create the model entity from asset path.
		/// </summary>
		/// <param name="path">Path of the model to load.</param>
		public SimpleModelEntity(string path) : this(GeonDefaultRenderer.CurrentContentManager.Load<Model>(path))
		{
		}

		/// <summary>
		/// Draw this model.
		/// </summary>
		/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
		public override void DoEntityDraw(ref Matrix worldTransformations)
		{
			// call base draw entity
			base.DoEntityDraw(ref worldTransformations);

			// reset last radius
			_lastRadius = 0f;
			float scaleLen = Math3D.GetScale(ref worldTransformations).Length();

			// iterate model meshes
			for (int i = 0; i < Model.Meshes.Count; i++)
			{
				var mesh = Model.Meshes[i];
				// iterate over mesh effects and apply them (set world matrix etc)
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					var effect = mesh.Effects[j];
					var material = effect.GetMaterial();
					material.Apply(ref worldTransformations, ref _lastBoundingSphere);
				}

				// update last radius
				_lastRadius = System.Math.Max(_lastRadius, mesh.BoundingSphere.Radius * scaleLen);

				// draw the mesh itself
				mesh.Draw();
			}
		}

		/// <summary>
		/// Prepare material to draw this model.
		/// </summary>
		/// <param name="material">Material to prepare.</param>
		/// <param name="world">World transformations.</param>
		protected void PrepareMaterial(Materials.MaterialAPI material, Matrix world) =>
			// set world / view / projection matrix of the effect
			material.World = world;

		/// <summary>
		/// Get the bounding sphere of this entity.
		/// </summary>
		/// <param name="parent">Parent node that's currently drawing this entity.</param>
		/// <param name="localTransformations">Local transformations from the direct parent node.</param>
		/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
		/// <returns>Bounding box of the entity.</returns>
		protected override BoundingSphere CalcBoundingSphere(Node parent, ref Matrix localTransformations, ref Matrix worldTransformations)
		{
			var modelBoundingSphere = ModelUtils.GetBoundingSphere(Model);
			modelBoundingSphere.Radius *= Math3D.GetScale(ref worldTransformations).Length();
			modelBoundingSphere.Center = worldTransformations.Translation;
			return modelBoundingSphere;

		}

		/// <summary>
		/// Get the bounding box of this entity.
		/// </summary>
		/// <param name="parent">Parent node that's currently drawing this entity.</param>
		/// <param name="localTransformations">Local transformations from the direct parent node.</param>
		/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
		/// <returns>Bounding box of the entity.</returns>
		protected override BoundingBox CalcBoundingBox(Node parent, ref Matrix localTransformations, ref Matrix worldTransformations)
		{
			// get bounding box in local space
			var modelBoundingBox = ModelUtils.GetBoundingBox(Model);

			// initialize minimum and maximum corners of the bounding box to max and min values
			var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			// iterate bounding box corners and transform them
			var array = modelBoundingBox.GetCorners();
			for (int i = 0; i < array.Length; i++)
			{
				var corner = array[i];
				// get curr position and update min / max
				var currPosition = Vector3.Transform(corner, worldTransformations);
				min = Vector3.Min(min, currPosition);
				max = Vector3.Max(max, currPosition);
			}

			// create and return transformed bounding box
			return new BoundingBox(min, max);
		}
	}
}