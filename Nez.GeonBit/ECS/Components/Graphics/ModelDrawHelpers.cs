﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nez.GeonBit
{
    public static class ModelDrawHelpers
    {

        public static void Draw(this Model m, Effect effect, Matrix world)
        {
            var fx = effect as IEffectMatrices;
            foreach (ModelMesh mesh in m.Meshes)
            {
                IEffectMatrices obj = (effect as IEffectMatrices) ?? throw new InvalidOperationException();
                obj.World = mesh.ParentBone.Transform * world;

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
