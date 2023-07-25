using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.Console;
internal class MetricsDisplay
{
    private MetricsDisplay() { }
    public readonly static MetricsDisplay Instance = new MetricsDisplay();
    public static bool Active { get; set; }

    public void Render()
    {
        var batcher = Graphics.Instance.Batcher;

        //Draw background
        int offsetX = 20;
        int offsetY = 1080 - 180;
        batcher.DrawRect(new Rectangle(offsetX, offsetY - 40, 300, 200), Color.Lerp(Color.Black, Color.Red, 0.1f) * 0.9f);

        //Draw text
        batcher.DrawString(Graphics.Instance.BitmapFont, "Rendering Metrics", new Vector2(offsetX + 20, offsetY - 30), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Draw Count:", new Vector2(offsetX + 20, offsetY + 0 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Sprite Count:", new Vector2(offsetX + 20, offsetY + 1 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Primitive Count:", new Vector2(offsetX + 20, offsetY + 2 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Texture Count:", new Vector2(offsetX + 20, offsetY + 3 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Render Target Count:", new Vector2(offsetX + 20, offsetY + 4 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Clear Count:", new Vector2(offsetX + 20, offsetY + 5 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Vertex Shader Count:", new Vector2(offsetX + 20, offsetY + 6 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, "Pixel Shader Cound:", new Vector2(offsetX + 20, offsetY + 7 * 20), Color.White);

        var metrics = Core.GraphicsDevice.Metrics;
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.DrawCount.ToString(), new Vector2(offsetX + 150, offsetY + 0 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.SpriteCount.ToString(), new Vector2(offsetX + 150, offsetY + 1 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.PrimitiveCount.ToString(), new Vector2(offsetX + 150, offsetY + 2 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.TextureCount.ToString(), new Vector2(offsetX + 150, offsetY + 3 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.TargetCount.ToString(), new Vector2(offsetX + 150, offsetY + 4 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.ClearCount.ToString(), new Vector2(offsetX + 150, offsetY + 5 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.VertexShaderCount.ToString(), new Vector2(offsetX + 150, offsetY + 6 * 20), Color.White);
        batcher.DrawString(Graphics.Instance.BitmapFont, metrics.PixelShaderCount.ToString(), new Vector2(offsetX + 150, offsetY + 7 * 20), Color.White);

    }
}
