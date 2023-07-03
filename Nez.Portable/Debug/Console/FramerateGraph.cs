using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Nez.Console
{
	internal class FramerateGraph : PostProcessor, IUpdatable
	{
		public enum GraphType
		{
			Line,
			Fill
		}
        private float _positionZ;
        private Vector2 _rotationXY;

		/// <summary>
		/// wraps Transform.position along with a private Z position
		/// </summary>
		/// <value>The position.</value>
		public Vector3 Position;

        /// <summary>
        /// the scale of the object. 80 by default. You will need to adjust this depending on your Scene's backbuffer size.
        /// </summary>
        public Vector3 Scale = new Vector3(80f);

		/// <summary>
		/// wraps Transform.rotation for the Z rotation along with a private X and Y rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Vector3 Rotation;

        /// <summary>
        /// Matrix that represents the world transform. Useful for rendering.
        /// </summary>
        /// <value>The world matrix.</value>
        public Matrix WorldMatrix
        {
            get
            {
                // prep our rotations
                var rot = Rotation;
                var rotationMatrix = Matrix.CreateRotationX(rot.X);
                rotationMatrix *= Matrix.CreateRotationY(rot.Y);
                rotationMatrix *= Matrix.CreateRotationZ(rot.Z);

                // remember to invert the sign of the y position!
                var pos = Position;
                return Matrix.CreateScale(Scale) * rotationMatrix * Matrix.CreateTranslation(pos.X, -pos.Y, pos.Z);
            }
        }

        public List<(float, Color)> values = new List<(float, Color)>();

		/// <summary>
		/// Determines whether the drawn graph will be line only, or filled
		/// </summary>
		public GraphType Type { get; set; }

		public Vector2 Size = new Vector2(1, 1);
		private Vector2 _pos;

		/// <summary>
		/// Determines the vertical scaling of the graph.
		/// The value that is equal to MaxValue will be displayed at the top of the graph (at point Size.Y)
		/// </summary>
		public float MaxValue { get; set; }

		private BasicEffect _effect;
		private short[] lineListIndices;
		private short[] triangleStripIndices;

		public FramerateGraph(Vector2 size, Vector2 position) : base(int.MaxValue)
		{
			Size = size;
			MaxValue = 1;
			Type = GraphType.Line;
			_pos = position;


            _effect = new BasicEffect(Core.GraphicsDevice)
            {
                View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreateOrthographicOffCenter(0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height, 0, 1.0f, 1000.0f),
                World = Matrix.Identity,

                VertexColorEnabled = true
            };
            Rotation = new Vector3(MathHelper.Pi, 0, 0);
            Position = new Vector3(_pos.X, -_pos.Y, 0);
        }

        public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
            base.Process(source, destination);

            if (values.Count < 2)
                return;



            //creates scaling (for the transformation) based on the number of points to draw
            Scale = new Vector3(Size.X / values.Count, Size.Y / MaxValue, 1);
            //_effect.World = Matrix.CreateRotationX(MathHelper.Pi) * WorldMatrix; //flips the graph so that the higher values are above. Makes bottom left the graph origin.
            //_effect.View = camera.ViewMatrix3D;
            //_effect.Projection = camera.ProjectionMatrix;

            _effect.World = WorldMatrix;

            //different point lists for different types of graphs
            if (Type == GraphType.Line)
            {
                var pointList = new VertexPositionColor[values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    pointList[i] = new VertexPositionColor(new Vector3(i, values[i].Item1 < MaxValue ? values[i].Item1 : MaxValue, 0), values[i].Item2);
                }

                DrawLineList(pointList);
            }
            else if (Type == GraphType.Fill)
            {
                var pointList = new VertexPositionColor[values.Count * 2];
                for (int i = 0; i < values.Count; i++)
                {
                    //The vertices are created so that the triangles are inverted (back facing). When rotated they will become front facing.
                    //This is done to avoid changing rasterizer state to CullMode.CullClockwiseFace.
                    pointList[i * 2 + 1] = new VertexPositionColor(new Vector3(i, values[i].Item1 < MaxValue ? values[i].Item1 : MaxValue, 0), values[i].Item2);
                    pointList[i * 2] = new VertexPositionColor(new Vector3(i, 0, 0), values[i].Item2);
                }

                DrawTriangleStrip(pointList);
            }
        }

		private void DrawLineList(VertexPositionColor[] pointList)
		{
			//indices updated only need to be updated when the number of points has changed
			if (lineListIndices == null || lineListIndices.Length != ((pointList.Length * 2) - 2))
			{
				lineListIndices = new short[(pointList.Length * 2) - 2];
				for (int i = 0; i < pointList.Length - 1; i++)
				{
					lineListIndices[i * 2] = (short)(i);
					lineListIndices[(i * 2) + 1] = (short)(i + 1);
				}
			}

			foreach (var pass in _effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				_effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
					PrimitiveType.LineList,
					pointList,
					0,
					pointList.Length,
					lineListIndices,
					0,
					pointList.Length - 1
				);
			}
		}

		private void DrawTriangleStrip(VertexPositionColor[] pointList)
		{
			if (triangleStripIndices == null || triangleStripIndices.Length != pointList.Length)
			{
				triangleStripIndices = new short[pointList.Length];
				for (int i = 0; i < pointList.Length; i++)
					triangleStripIndices[i] = (short)i;
			}

			foreach (var pass in _effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				_effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
					PrimitiveType.TriangleStrip,
					pointList,
					0,
					pointList.Length,
					triangleStripIndices,
					0,
					pointList.Length - 2
				);
			}
		}

		private const int SampleCount = 600;
		private const int bestFrameRate = 120;
		private float fr;

		public void Update()
		{
			float newFr = (float)Math.Round(1 / Time.OriginalDeltaTime);
			float lerpFactor = MathHelper.Clamp( Math.Abs(newFr - fr) / 1000, 0.01F, 0.5F);
			fr = MathHelper.Lerp(fr, newFr, lerpFactor);
			values.Add((fr, Color.Lerp(Color.Red, Color.Lime, fr / bestFrameRate)));
			if (values.Count > SampleCount) values.RemoveRange(0, values.Count - SampleCount); //Remove access
			MaxValue = Math.Max(MaxValue, fr);
		}

		private static bool _isGraphInScene;

        public static bool isGraphInScene
		{
			get => _isGraphInScene;
			set
			{
				if (value && !isGraphInScene)
				{
					Core.Scene.AddPostProcessor(new FramerateGraph(new Vector2(200, 100), new Vector2(100, 200)) { Type = GraphType.Fill });
				}
				else if (!value)
				{
					var val = Core.Scene.GetPostProcessor<FramerateGraph>();
                    if (val is not null) Core.Scene.RemovePostProcessor(val);
				}
				_isGraphInScene = value;

            }
		}

		bool IUpdatable.Enabled => true;

        public int UpdateOrder => 0;
    }
}