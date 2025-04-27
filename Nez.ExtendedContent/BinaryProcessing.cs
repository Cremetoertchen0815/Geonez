using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Nez.ExtendedContent;

[ContentProcessor(DisplayName = "BinaryGZipProcessor")]
public class BinaryGZipProcessor : ContentProcessor<byte[], ProcessedBinaryData>
{
    public override ProcessedBinaryData Process(byte[] input, ContentProcessorContext context)
    {
        using (var msi = new MemoryStream(input))
        using (var mso = new MemoryStream())
        {
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return new ProcessedBinaryData { Compressed = true, Data = mso.ToArray() };
        }
    }
}

[ContentProcessor(DisplayName = "BinaryUncompressedProcessor")]
public class BinaryUncompressedProcessor : ContentProcessor<byte[], ProcessedBinaryData>
{
    public override ProcessedBinaryData Process(byte[] input, ContentProcessorContext context)
    {
        return new ProcessedBinaryData { Compressed = false, Data = input };
    }
}

public class ProcessedBinaryData
{
    public bool Compressed;
    public byte[] Data;
}

[ContentTypeWriter]
public class BinaryContentWriter : ContentTypeWriter<ProcessedBinaryData>
{
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return "Nez.BinaryContentReader, Nez";
    }

    protected override void Write(ContentWriter output, ProcessedBinaryData value)
    {
        output.Write((value.Compressed ? -1 : 1) * value.Data.Length);
        output.Write(value.Data);
    }
}