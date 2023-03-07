using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nez.GeonBit
{
    public static class ModelDrawHelpers
	{

		private static Matrix[] sharedDrawBoneMatrices;

		public static void Draw(this Model m, Effect effect, Matrix world)
		{
			var fx = effect as IEffectMatrices;
			int count = m.Bones.Count;
			if (sharedDrawBoneMatrices == null || sharedDrawBoneMatrices.Length < count)
			{
				sharedDrawBoneMatrices = new Matrix[count];
			}

			m.CopyAbsoluteBoneTransformsTo(sharedDrawBoneMatrices);
			foreach (ModelMesh mesh in m.Meshes)
			{
				IEffectMatrices obj = (effect as IEffectMatrices) ?? throw new InvalidOperationException();
				obj.World = world;

				mesh.Draw(effect);
			}
		}


		public static void Draw(this ModelMesh mesh, Effect effect)
		{
			for (int i = 0; i < mesh.MeshParts.Count; i++) mesh.MeshParts[i].Draw(effect);
        }

        public static void Draw(this ModelMesh mesh, Effect effect, Matrix world)
        {
			if (effect is IEffectMatrices ff) ff.World = world;
            for (int i = 0; i < mesh.MeshParts.Count; i++) mesh.MeshParts[i].Draw(effect);
        }

        public static void Draw(this ModelMeshPart modelMeshPart, Effect effect)
		{
			if (modelMeshPart.PrimitiveCount > 0)
			{
				Core.GraphicsDevice.SetVertexBuffer(modelMeshPart.VertexBuffer);
				Core.GraphicsDevice.Indices = modelMeshPart.IndexBuffer;
				for (int j = 0; j < effect.CurrentTechnique.Passes.Count; j++)
				{
					effect.CurrentTechnique.Passes[j].Apply();
					Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, modelMeshPart.VertexOffset, modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount);
				}
			}
		}
	}
}
