using Nez.Textures;

namespace Nez.Sprites;

public class SpriteAnimation
{
    public readonly float FrameRate;
    public readonly Sprite[] Sprites;

    public SpriteAnimation(Sprite[] sprites, float frameRate)
    {
        Sprites = sprites;
        FrameRate = frameRate;
    }
}