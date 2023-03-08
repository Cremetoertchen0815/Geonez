using Microsoft.Xna.Framework;
using Nez.GeonBit.Lights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit.Graphics.Lights
{
	public interface ILightSource
	{
		/// <summary>
		/// Is this light source currently visible?
		/// </summary>
		bool Enabled { get; set; }

		LightsManager LightsManager { get; set; }

		/// <summary>
		/// So we can cache lights and identify when they were changed.
		/// </summary>
		uint ParamsVersion { get; set; }

		/// <summary>
		/// Return if this light is a directional light.
		/// </summary>
		public virtual bool IsDirectionalLight => Direction != null;

		/// <summary>
		/// Light position in world space.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Light direction, if its a directional light.
		/// </summary>
		public Vector3? Direction { get; set; }

		/// <summary>
		/// Light color and strength (A field = light strength).
		/// </summary>
		public Color Diffuse { get; set; }

		/// <summary>
		/// Specular factor.
		/// </summary>
		public Color Specular { get; set; }

		/// <summary>
		/// Remove self from parent lights manager.
		/// </summary>
		void Remove();

		/// <summary>
		/// Update light transformations.
		/// </summary>
		/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
		void UpdateTransforms(ref Matrix worldTransformations);


		///// <summary>
		///// Update light transformations.
		///// </summary>
		///// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
		//public virtual void UpdateTransforms(ref Matrix worldTransformations)
		//{
		//	// if didn't really change skip
		//	if (_transform == worldTransformations) { return; }

		//	// break transformation into components
		//	_transform.Decompose(out var scale, out var rotation, out var position);

		//	// set world position. this will also recalc bounding sphere and update lights manager, if needed.
		//	Position = position;
		//}

		///// <summary>
		///// Recalculate light bounding sphere after transformations or radius change.
		///// </summary>
		///// <param name="updateInLightsManager">If true, will also update light position in lights manager.</param>
		//public virtual void RecalcBoundingSphere(bool updateInLightsManager = true)
		//{
		//	// calc light bounding sphere
		//	BoundingSphere = new BoundingSphere(Position, _range);

		//	// notify manager on update
		//	if (updateInLightsManager && LightsManager != null)
		//	{
		//		LightsManager.UpdateLightTransform(this);
		//	}
		//}
	}
}
