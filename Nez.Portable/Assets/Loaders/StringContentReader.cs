using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Nez;

/// <summary>
///     Reads a string encoded in UTF-8, either uncompressed or compressed with GZip.
///     Useful for loading and managing whole text files via the Content Pipeline.
///     To decompress the data via GZip, the array length must be negative.
/// </summary>
internal class StringContentReader : ContentTypeReader<string>
{
    protected override string Read(ContentReader input, string existingInstance)
    {
        //Read data from file
        var length = input.ReadInt32();
        var compressed = length < 0;
        length = Math.Abs(length);
        var data = input.ReadBytes(length);
        var decomp = new byte[length];

        if (!compressed) return Encoding.UTF8.GetString(data);

        //Decompress byte stream via gzip
        using (var msi = new MemoryStream(data))
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.ReadExactly(decomp, 0, data.Length);
        }

        return Encoding.UTF8.GetString(decomp);
    }
}