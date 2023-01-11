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
// Static body component.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion

using Nez.GeonBit.Physics;

namespace Nez.GeonBit
{
	/// <summary>
	/// A Static Body component.
	/// This body will not respond to forces, and will always copy the transformations of the parent Game Object.
	/// It is optimized for static, constant things like terrain, trees, rocks, walls, etc.
	/// </summary>
	public class StaticBody : BasePhysicsComponent
	{
		// the core static body
		private Physics.StaticBody _body;

		/// <summary>
		/// The physical body in the core layer.
		/// </summary>
		internal override BasicPhysicalBody _PhysicalBody => _body;

		/// <summary>
		/// The shape used for this physical body.
		/// </summary>
		private Physics.CollisionShapes.ICollisionShape _shape = null;

		/// <summary>
		/// Create the static collision body from shape info.
		/// </summary>
		/// <param name="shapeInfo">Body shape info.</param>
		public StaticBody(IBodyShapeInfo shapeInfo) => CreateBody(shapeInfo.CreateShape());

		/// <summary>
		/// Create the static collision body from shape instance.
		/// </summary>
		/// <param name="shape">Shape to use.</param>
		public StaticBody(Physics.CollisionShapes.ICollisionShape shape) => CreateBody(shape);

		/// <summary>
		/// Create the actual collision body.
		/// </summary>
		/// <param name="shape">Collision shape.</param>
		private void CreateBody(Physics.CollisionShapes.ICollisionShape shape)
		{
			_shape = shape;
			_body = new Physics.StaticBody(shape)
			{
				EcsComponent = this
			};
		}

		/// <summary>
		/// Clone this component.
		/// </summary>
		/// <returns>Cloned copy of this component.</returns>
		public override Component Clone()
		{
			// create cloned component to return
			var ret = (KinematicBody)CopyBasics(new KinematicBody(_shape.Clone()));

			// return the cloned object
			return ret;
		}

		/// <summary>
		/// Called every time scene node transformation updates.
		/// Note: this is called only if GameObject is enabled and have Update events enabled.
		/// </summary>
		public override void OnTransformationUpdate() => _body.WorldTransform = Node.WorldTransformations;


		/// <summary>
		/// Called when this component is effectively added to scene, eg when added
		/// to a GameObject currently in scene or when its GameObject is added to scene.
		/// </summary>
		public override void OnAddedToEntity()
		{
			base.OnAddedToEntity();

			// transform to match game object transformations
			OnTransformationUpdate();
		}
	}
}
