﻿using Microsoft.Xna.Framework;
using System;


namespace Nez
{
	public class Torus3D : GeometricPrimitive3D
	{
		[Range(0.01f, 1f)]
		public float Thickness
		{
			get => _thickness;
			set => GenerateTorusGeometry(value, _tessellation, _color);
		}

		[Range(5, 300)]
		public int Tessellation
		{
			get => _tessellation;
			set => GenerateTorusGeometry(_thickness, value, _color);
		}

		private float _thickness;
		private int _tessellation;
		private Color _color;


		public Torus3D() : this(0.5f, 50, Color.Red)
		{
		}

		public Torus3D(float thickness, int tessellation, Color color) => GenerateTorusGeometry(thickness, tessellation, color);

		private void GenerateTorusGeometry(float torusThickness, int torusTessellation, Color torusColor)
		{
			_thickness = torusThickness;
			_tessellation = torusTessellation;
			_color = torusColor;

			_vertices.Clear();
			_indices.Clear();

			float diameter = 1f;

			// First we loop around the main ring of the torus. 
			for (int i = 0; i < torusTessellation; i++)
			{
				float outerAngle = i * MathHelper.TwoPi / torusTessellation;

				// Create a transform matrix that will align geometry to slice perpendicularly though the current ring position. 
				var vertTransform = Matrix.CreateTranslation(diameter / 2, 0, 0) * Matrix.CreateRotationY(outerAngle);

				// Now we loop along the other axis, around the side of the tube. 
				for (int j = 0; j < torusTessellation; j++)
				{
					float innerAngle = j * MathHelper.TwoPi / torusTessellation;

					float dx = (float)Math.Cos(innerAngle);
					float dy = (float)Math.Sin(innerAngle);

					// Create a vertex
					var normal = new Vector3(dx, dy, 0);
					var pos = normal * torusThickness / 2;

					pos = Vector3.Transform(pos, vertTransform);
					normal = Vector3.TransformNormal(normal, vertTransform);

					AddVertex(pos, torusColor, normal);

					// and create indices for two triangles. 
					int nextI = (i + 1) % torusTessellation;
					int nextJ = (j + 1) % torusTessellation;

					AddIndex(i * torusTessellation + j);
					AddIndex(i * torusTessellation + nextJ);
					AddIndex(nextI * torusTessellation + j);

					AddIndex(i * torusTessellation + nextJ);
					AddIndex(nextI * torusTessellation + nextJ);
					AddIndex(nextI * torusTessellation + j);
				}
			}

			InitializePrimitive();
		}
	}
}