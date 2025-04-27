using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Nez.ExtendedContent;

[ContentImporter(".tmx", DisplayName = "Tiled Level Importer", DefaultProcessor = "BinaryGZipProcessor")]
public class TiledImporter : ContentImporter<byte[]>
{
    public override byte[] Import(string filename, ContentImporterContext context)
    {
        return Encoding.UTF8.GetBytes(File.ReadAllText(filename).Replace(".png", "").Replace(".tsx", ""));
    }
}