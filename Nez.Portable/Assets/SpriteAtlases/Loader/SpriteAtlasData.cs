using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Sprites;

/// <summary>
///     temporary class used when loading a SpriteAtlas and by the sprite atlas editor
/// </summary>
internal class SpriteAtlasData
{
    public List<int> AnimationFps = [];
    public List<List<int>> AnimationFrames = [];

    public List<string> AnimationNames = [];
    public List<string> Names = [];
    public List<Vector2> Origins = [];
    public List<Rectangle> SourceRects = [];

    public SpriteAtlas AsSpriteAtlas(Texture2D texture, bool generateStencil)
    {
        var atlas = new SpriteAtlas();
        var framesUsedByAnimations = new List<int>();
        var stencilTxt = generateStencil ? Sprite.GenerateStencil(texture) : null;

        //Generate all sprites
        atlas.Names = Names.ToArray();
        atlas.Sprites = new Sprite[atlas.Names.Length];

        for (var i = 0; i < atlas.Sprites.Length; i++)
            atlas.Sprites[i] = new Sprite(texture, SourceRects[i], Origins[i]) { StencilTexture = stencilTxt };

        //Generate animations
        atlas.AnimationNames = AnimationNames.ToArray();
        atlas.SpriteAnimations = new SpriteAnimation[atlas.AnimationNames.Length];
        for (var i = 0; i < atlas.SpriteAnimations.Length; i++)
        {
            var sprites = new Sprite[AnimationFrames[i].Count];
            for (var j = 0; j < sprites.Length; j++)
            {
                var frame = AnimationFrames[i][j];
                sprites[j] = atlas.Sprites[frame];
                framesUsedByAnimations.Add(frame);
            }

            atlas.SpriteAnimations[i] = new SpriteAnimation(sprites, AnimationFps[i]);
        }

        //Filter sprites for the ones used by the animation
        var nonAnimeSprites = new List<int>();

        for (var i = 0; i < atlas.Sprites.Length; i++)
            if (!framesUsedByAnimations.Contains(i))
                nonAnimeSprites.Add(i);
        atlas.NonAnimationSprites = nonAnimeSprites.ToArray();


        return atlas;
    }

    public void Clear()
    {
        Names.Clear();
        SourceRects.Clear();
        Origins.Clear();

        AnimationNames.Clear();
        AnimationFps.Clear();
        AnimationFrames.Clear();
    }

    public void SaveToFile(string filename)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        using (var writer = new StreamWriter(filename))
        {
            for (var i = 0; i < Names.Count; i++)
            {
                writer.WriteLine(Names[i]);

                var rect = SourceRects[i];
                writer.WriteLine("\t{0},{1},{2},{3}", rect.X, rect.Y, rect.Width, rect.Height);
                writer.WriteLine("\t{0},{1}", Origins[i].X, Origins[i].Y);
            }

            if (AnimationNames.Count > 0)
            {
                writer.WriteLine();

                for (var i = 0; i < AnimationNames.Count; i++)
                {
                    writer.WriteLine(AnimationNames[i]);
                    writer.WriteLine("\t{0}", AnimationFps[i]);
                    writer.WriteLine("\t{0}", string.Join(",", AnimationFrames[i]));
                }
            }
        }
    }
}