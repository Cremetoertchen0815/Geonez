using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.GeonBit.Graphics.Lights
{
	public interface IRangedLight : ILightSource
	{
		/// <summary>
		/// Light bounding sphere.
		/// </summary>
		public BoundingSphere BoundingSphere { get; }

		/// <summary>
		/// Light range.
		/// </summary>
		public float Range { get; set; }

		/// <summary>
		/// Min lights region index this light is currently in.
		/// </summary>
		Vector3 MinRegionIndex { get; set; }

		/// <summary>
		/// Max lights region index this light is currently in.
		/// </summary>
		Vector3 MaxRegionIndex { get; set; }
	}
}
