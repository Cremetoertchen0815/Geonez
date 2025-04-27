﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;
using Nez.Textures;

namespace Nez.Sprites;

/// <summary>
///     Provides methods for parsing SpriteAtlas data from a binary content file.
/// </summary>
internal static class SpriteAtlasLoader
{
	/// <summary>
	///     parses a .atlas file and loads up a SpriteAtlas with it's associated Texture
	/// </summary>
	internal static SpriteAtlas ParseSpriteAtlas(string dataFile, NezContentManager content, bool generateStencil,
        bool premultiplyAlpha)
    {
        var contentData = content.Load<byte[]>(dataFile);
        using (var str = new MemoryStream(contentData))
        {
            SpriteAtlasData spriteAtlas;
            using (var reader = new BinaryReader(str))
            {
                spriteAtlas = ParseBinarySpriteAtlasData(reader);
                return spriteAtlas.AsSpriteAtlas(
                    premultiplyAlpha
                        ? TextureUtils.TextureFromStreamPreMultiplied(str)
                        : Texture2D.FromStream(Core.GraphicsDevice, str), generateStencil);
            }
        }
    }

    internal static SpriteAtlasData ParseBinarySpriteAtlasData(BinaryReader r)
    {
        var spriteAtlas = new SpriteAtlasData();

        //Read sprites
        var len = r.ReadInt32();
        for (var i = 0; i < len; i++)
        {
            spriteAtlas.Names.Add(r.ReadString());
            spriteAtlas.SourceRects.Add(new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));
            spriteAtlas.Origins.Add(new Vector2(r.ReadSingle(), r.ReadSingle()));
        }

        //Read sprites animations
        len = r.ReadInt32();
        for (var i = 0; i < len; i++)
        {
            spriteAtlas.AnimationNames.Add(r.ReadString());
            spriteAtlas.AnimationFps.Add(r.ReadInt32());

            var frames = new List<int>();
            var lenB = r.ReadInt32();
            for (var j = 0; j < lenB; j++) frames.Add(r.ReadInt32());
            spriteAtlas.AnimationFrames.Add(frames);
        }

        return spriteAtlas;
    }

    /// <summary>
    ///     parses a .atlas file into a temporary SpriteAtlasData class. If leaveOriginsRelative is true, origins will be left
    ///     as 0 - 1 range instead
    ///     of multiplying them by the width/height.
    /// </summary>
    internal static SpriteAtlasData ParseSpriteAtlasData(string dataFile, bool leaveOriginsRelative = false)
    {
        var spriteAtlas = new SpriteAtlasData();

        var parsingSprites = true;
        var commaSplitter = new[] { ',' };

        string line = null;
        using (var streamFile = File.OpenRead(dataFile))
        {
            using (var stream = new StreamReader(streamFile))
            {
                while ((line = stream.ReadLine()) != null)
                {
                    // once we hit an empty line we are done parsing sprites so we move on to parsing animations
                    if (parsingSprites && string.IsNullOrWhiteSpace(line))
                    {
                        parsingSprites = false;
                        continue;
                    }

                    if (parsingSprites)
                    {
                        spriteAtlas.Names.Add(line);

                        // source rect
                        line = stream.ReadLine();
                        var lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);
                        var rect = new Rectangle(int.Parse(lineParts[0]), int.Parse(lineParts[1]),
                            int.Parse(lineParts[2]), int.Parse(lineParts[3]));
                        spriteAtlas.SourceRects.Add(rect);

                        // origin
                        line = stream.ReadLine();
                        lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);
                        var origin = new Vector2(float.Parse(lineParts[0], CultureInfo.InvariantCulture),
                            float.Parse(lineParts[1], CultureInfo.InvariantCulture));

                        if (leaveOriginsRelative)
                            spriteAtlas.Origins.Add(origin);
                        else
                            spriteAtlas.Origins.Add(origin * new Vector2(rect.Width, rect.Height));
                    }
                    else
                    {
                        // catch the case of a newline at the end of the file
                        if (string.IsNullOrWhiteSpace(line))
                            break;

                        spriteAtlas.AnimationNames.Add(line);

                        // animation fps
                        line = stream.ReadLine();
                        spriteAtlas.AnimationFps.Add(int.Parse(line));

                        // animation frames
                        line = stream.ReadLine();
                        var frames = new List<int>();
                        spriteAtlas.AnimationFrames.Add(frames);
                        var lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var part in lineParts)
                            frames.Add(int.Parse(part));
                    }
                }
            }
        }

        return spriteAtlas;
    }
}