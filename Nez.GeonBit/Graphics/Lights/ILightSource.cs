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
	}
}
