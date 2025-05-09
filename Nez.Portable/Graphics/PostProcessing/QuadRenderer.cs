﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez;

/// <summary>
///     Renders a simple quad to the screen. Uncomment the Vertex / Index buffers to make it a static fullscreen quad.
///     The performance effect is barely measurable though and you need to dispose of the buffers when finished!
/// </summary>
public class QuadRenderer
{
    private readonly short[] _indexBuffer;

    //buffers for rendering the quad
    private readonly VertexPositionTexture[] _vertexBuffer;

    public QuadRenderer(GraphicsDevice graphicsDevice)
    {
        _vertexBuffer = new VertexPositionTexture[4];
        _vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0));
        _vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
        _vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1));
        _vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1));

        _indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
    }

    public void RenderQuad(GraphicsDevice graphicsDevice, Vector2 v1, Vector2 v2)
    {
        var offsetX = 1f / graphicsDevice.Viewport.Bounds.Size.X;
        var offsetY = 1f / graphicsDevice.Viewport.Bounds.Size.Y;
        _vertexBuffer[0].Position.X = v1.X - offsetX;
        _vertexBuffer[0].Position.Y = v2.Y + offsetY;

        _vertexBuffer[1].Position.X = v2.X - offsetX;
        _vertexBuffer[1].Position.Y = v2.Y + offsetY;

        _vertexBuffer[2].Position.X = v1.X - offsetX;
        _vertexBuffer[2].Position.Y = v1.Y + offsetY;

        _vertexBuffer[3].Position.X = v2.X - offsetX;
        _vertexBuffer[3].Position.Y = v1.Y + offsetY;

        graphicsDevice.DrawUserIndexedPrimitives
            (PrimitiveType.TriangleList, _vertexBuffer, 0, 4, _indexBuffer, 0, 2);
    }
}