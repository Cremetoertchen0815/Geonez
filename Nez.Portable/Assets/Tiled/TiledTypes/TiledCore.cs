using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Tiled;

public class TmxDocument
{
    public string TmxDirectory;

    public TmxDocument()
    {
        TmxDirectory = string.Empty;
    }
}

public interface ITmxElement
{
    string Name { get; }
}

public class TmxList<T> : KeyedCollection<string, T> where T : ITmxElement
{
    private readonly Dictionary<string, int> _nameCount = new();

    public new void Add(T t)
    {
        var tName = t.Name;

        // Rename duplicate entries by appending a number
        if (Contains(tName))
            _nameCount[tName] += 1;
        else
            _nameCount.Add(tName, 0);
        base.Add(t);
    }

    protected override string GetKeyForItem(T item)
    {
        var name = item.Name;
        var count = _nameCount[name];

        var dupes = 0;

        // For duplicate keys, append a counter. For pathological cases, insert underscores to ensure uniqueness
        while (Contains(name))
        {
            name = name + string.Concat(Enumerable.Repeat("_", dupes)) + count;
            dupes++;
        }

        return name;
    }
}

public class TmxImage : IDisposable
{
    public Stream Data;
    public string Format;
    public int Height;
    public string Source;
    public Texture2D Texture;
    public Color Trans;
    public int Width;

    public void Dispose()
    {
        if (Texture != null)
        {
            Texture.Dispose();
            Texture = null;
        }
    }
}

public class TmxBase64Data
{
    public TmxBase64Data(XElement xData)
    {
        if ((string)xData.Attribute("encoding") != "base64")
            throw new Exception("TmxBase64Data: Only Base64-encoded data is supported.");

        var rawData = Convert.FromBase64String(xData.Value);
        Data = new MemoryStream(rawData, false);

        var compression = (string)xData.Attribute("compression");
        if (compression == "gzip")
        {
            Data = new GZipStream(Data, CompressionMode.Decompress);
        }
        else if (compression == "zlib")
        {
            // Strip 2-byte header and 4-byte checksum
            // TODO: Validate header here
            var bodyLength = rawData.Length - 6;
            var bodyData = new byte[bodyLength];
            Array.Copy(rawData, 2, bodyData, 0, bodyLength);

            var bodyStream = new MemoryStream(bodyData, false);
            Data = new DeflateStream(bodyStream, CompressionMode.Decompress);

            // TODO: Validate checksum?
        }
        else if (compression != null)
        {
            throw new Exception("TmxBase64Data: Unknown compression.");
        }
    }

    public Stream Data { get; }
}