using Microsoft.Xna.Framework.Content;
using MonoGame.Framework.Utilities.Deflate;
using System.IO;
using System.Text;

namespace Nez
{
	/// <summary>
	/// Reads a string encoded in UTF-8, either uncompressed or compressed with GZip.
	/// Useful for loading and managing whole text files via the Content Pipeline.
	/// To decompress the data via GZip, the array length must be negative.
	/// </summary>
	internal class StringContentReader : ContentTypeReader<string>
	{
		protected override string Read(ContentReader input, string existingInstance)
		{
			//Read data from file
			int length = input.ReadInt32();
			bool compressed = length < 0;
			length = System.Math.Abs(length);
			byte[] data = input.ReadBytes(length);
			byte[] decomp = new byte[length];

			if (!compressed) return Encoding.UTF8.GetString(data);

			//Decompress byte stream via gzip
			using (var msi = new MemoryStream(data))
			using (var gs = new GZipStream(msi, CompressionMode.Decompress))
				gs.Read(decomp, 0, data.Length);

			return Encoding.UTF8.GetString(decomp);
		}
	}
}
