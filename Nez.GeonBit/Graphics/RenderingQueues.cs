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
// Define the rendering queues.
// Rendering queues are lists of items to draw with specific device settings and
// order. Its important in order to handle effects, opacity, etc.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nez.GeonBit
{
	/// <summary>
	/// Pre-defined rendering queue.
	/// Every rendering queue have drawing settings and their order determine the order in which object batches will be drawn.
	/// </summary>
	public enum RenderingQueue
	{
		/// <summary>
		/// Will not use rendering queue, but simply draw this entity the moment the draw function is called.
		/// This does not guarantee any specific order and will use default device settings.
		/// Is capable of casting shadows.
		/// </summary>
		NoQueue = -1,

		/// <summary>
		/// Draw solid entities without depth buffer and without any culling. 
		/// Everything drawn in the other queues will cover entities in this queue.
		/// Is not capable of casting shadows.
		/// </summary>
		SolidBackNoCull,

		/// <summary>
		/// Draw solid entities with depth buffer and no culling. 
		/// This is the default queue for simple 3D meshes without alpha channels.
		/// Is capable of casting shadows.
		/// </summary>
		SolidNoCull,

		/// <summary>
		/// Draw solid entities with depth buffer. 
		/// This is the default queue for simple 3D meshes without alpha channels.
		/// Is capable of casting shadows.
		/// </summary>
		Solid,

		/// <summary>
		/// Drawing settings for solid terrain meshes.
		/// Is capable of casting shadows.
		/// </summary>
		Terrain,

		/// <summary>
		/// Drawing settings for billboards.
		/// Is not capable of casting shadows.
		/// </summary>
		Billboards,

		/// <summary>
		/// Draw after all the solid queues, without affecting the depth buffer.
		/// This means it draw things in the background that will not hide any other objects.
		/// Is not capable of casting shadows.
		/// </summary>
		Background,

		/// <summary>
		/// Draw after all the solid queues, without affecting the depth buffer and without culling.
		/// This means it draw things in the background that will not hide any other objects.
		/// Is not capable of casting shadows.
		/// </summary>
		BackgroundNoCull,

		/// <summary>
		/// For entities with opacity, but does not order by distance from camera.
		/// This means its a good queue for entities with alpha channels on top of solid items, but its not suitable if entities with alpha may cover each other.
		/// Is capable of casting shadows.
		/// </summary>
		OpacityUnordered,

		/// <summary>
		/// For entities with opacity, order renderings by distance from camera.
		/// This is the best queue to use for dynamic entities with alpha channels that might cover each other.
		/// Is capable of casting shadows.
		/// </summary>
		Opacity,

		/// <summary>
		/// For entities that are mostly solid and opaque, but have some transparent elements in them.
		/// Is capable of casting shadows.
		/// </summary>
		Mixed,

		/// <summary>
		/// Special queue that draws everything as wireframe.
		/// Is not capable of casting shadows.
		/// </summary>
		Wireframe,

		/// <summary>
		/// For special effects and particles, but will still use depth buffer, and will not sort by distance from camera.
		/// Is not capable of casting shadows.
		/// </summary>
		EffectsUnordered,

		/// <summary>
		/// For special effects and particles, but will still use depth buffer, and will sort by distance from camera.
		/// Is not capable of casting shadows.
		/// </summary>
		Effects,

		/// <summary>
		/// For special effects and particles, does not use depth buffer (eg will always be rendered on top).
		/// Is not capable of casting shadows.
		/// </summary>
		EffectsOverlay,

		/// <summary>
		/// Renders last, on top of everything, without using depth buffer.
		/// Is not capable of casting shadows.
		/// </summary>
		Overlay,

		/// <summary>
		/// Render queue for debug purposes.
		/// Is not capable of casting shadows.
		/// Note: this queue only draws when in debug mode!
		/// </summary>
		Debug,
	}

	/// <summary>
	/// A single entity in a rendering queue.
	/// </summary>
	internal struct EntityInQueue
	{
		/// <summary>
		/// The renderable entity.
		/// </summary>
		public BaseRenderableEntity Entity;

		/// <summary>
		/// World transformations to draw with.
		/// </summary>
		public Matrix World;

		/// <summary>
		/// Create the entity-in-queue entry.
		/// </summary>
		/// <param name="entity">Entity to draw.</param>
		/// <param name="world">World transformations.</param>
		public EntityInQueue(BaseRenderableEntity entity, Matrix world)
		{
			Entity = entity;
			World = world;
		}
	}

	/// <summary>
	/// Rendering queue settings and entities.
	/// </summary>
	internal class RenderingQueueInstance
	{
		/// <summary>
		/// Current entities in queue.
		/// </summary>
		public List<EntityInQueue> Entities = new List<EntityInQueue>();

		/// <summary>
		/// Rasterizer settings of this queue.
		/// </summary>
		public RasterizerState RasterizerState = new RasterizerState();

		/// <summary>
		/// Depth stencil settings for this queue.
		/// </summary>
		public DepthStencilState DepthStencilState = new DepthStencilState();

		/// <summary>
		/// If true, will sort entities by distance from camera.
		/// </summary>
		public bool SortByCamera = false;

		/// <summary>
		/// If true, shadows will be rendered in this queue. Is false by default.
		/// </summary>
		public bool CanCastShadow = false;
	}

	/// <summary>
	/// Manage and draw the rendering queues.
	/// </summary>
	internal static class RenderingQueues
	{
		// List of built-in rendering queues.
		private static readonly List<RenderingQueueInstance> _renderingQueues = new List<RenderingQueueInstance>();

		private static bool _queueSorted = false;

		/// <summary>
		/// Init all built-in rendering queues.
		/// </summary>
		public static void Initialize()
		{
			_renderingQueues.Clear();

			// SolidBackNoCull
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = false;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				_renderingQueues.Add(queue);
			}

			// Solid No Cull
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = true;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Solid
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = true;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Terrain
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = true;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Billboards
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = true;
				_renderingQueues.Add(queue);
			}

			// Background
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				_renderingQueues.Add(queue);
			}

			// BackgroundNoCull
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = false;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				_renderingQueues.Add(queue);
			}

			// OpacityUnordered
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Opacity
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = true;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Mixed
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = true;
				queue.SortByCamera = true;
				queue.CanCastShadow = true;
				_renderingQueues.Add(queue);
			}

			// Wireframe
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.WireFrame;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				_renderingQueues.Add(queue);
			}

			// EffectsUnordered
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = false;
				_renderingQueues.Add(queue);
			}

			// Effects
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = true;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = true;
				_renderingQueues.Add(queue);
			}

			// EffectsOverlay
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.None;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = false;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = false;
				_renderingQueues.Add(queue);
			}

			// Overlay
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = false;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = false;
				_renderingQueues.Add(queue);
			}

			// debug stuff
			{
				var queue = new RenderingQueueInstance();
				queue.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
				queue.RasterizerState.DepthClipEnable = true;
				queue.RasterizerState.FillMode = FillMode.Solid;
				queue.RasterizerState.MultiSampleAntiAlias = true;
				queue.DepthStencilState.DepthBufferEnable = false;
				queue.DepthStencilState.DepthBufferWriteEnable = false;
				queue.SortByCamera = false;
				_renderingQueues.Add(queue);
			}
		}

		// default rasterizer state to reset to after every frame.
		private static readonly RasterizerState _defaultRasterizerState = new RasterizerState();

		/// <summary>
		/// Draw rendering queues.
		/// </summary>
		public static void DrawQueues()
		{
			// iterate drawing queues
			for (int i = 0; i < _renderingQueues.Count; i++)
			{
				var queue = _renderingQueues[i];
				// if no entities in queue, skip
				if (queue.Entities.Count == 0)
				{
					continue;
				}

				// apply queue states
				Core.GraphicsDevice.RasterizerState = queue.RasterizerState;
				Core.GraphicsDevice.DepthStencilState = queue.DepthStencilState;

				// if need to sort by distance from camera, do the sorting
				if (queue.SortByCamera)
				{
					var camPos = GeonDefaultRenderer.ActiveCamera.Position;
					queue.Entities.Sort(delegate (EntityInQueue x, EntityInQueue y)
					{
						return (int)(Vector3.Distance(camPos, y.World.Translation) * 100f - System.Math.Floor(y.Entity.CameraDistanceBias)) -
								(int)(Vector3.Distance(camPos, x.World.Translation) * 100f - System.Math.Floor(x.Entity.CameraDistanceBias));
					});
				}

				// draw all entities in queue
				for (int j = 0; j < queue.Entities.Count; j++)
				{
					var entityData = queue.Entities[j];
						entityData.Entity.DoEntityDraw(ref entityData.World);
				}

				// clear queue
				queue.Entities.Clear();

			}

			_queueSorted = false;

			// reset device states
			Core.GraphicsDevice.RasterizerState = _defaultRasterizerState;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		public static void RenderShadows()
		{

			// iterate drawing queues
			for (int i = 0; i < _renderingQueues.Count; i++)
			{
				var queue = _renderingQueues[i];
				// if no entities in queue, skip
				if (queue.Entities.Count == 0 || !queue.CanCastShadow)
				{
					continue;
				}

				// draw all entities in queue
				for (int j = 0; j < queue.Entities.Count; j++)
				{
					var entityData = queue.Entities[j];
					if (!entityData.Entity.ShadowDraw) continue;
					entityData.Entity.RenderShadows(entityData.World);
				}

				// don't clear queues
			}

			// reset device states
			Core.GraphicsDevice.RasterizerState = _defaultRasterizerState;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		/// <summary>
		/// Add entity to its rendering queue.
		/// </summary>
		/// <param name="entity">Entity to push to queue.</param>
		/// <param name="world">World transformations.</param>
		public static void AddEntity(BaseRenderableEntity entity, Matrix world)
		{
			// special case - skip debug if not in debug mode
			if (entity.RenderingQueue == RenderingQueue.Debug && !Core.DebugRenderEnabled)
			{
				return;
			}

			// add to the rendering queue
			_renderingQueues[(int)entity.RenderingQueue].Entities.Add(new EntityInQueue(entity, world));
		}
	}
}