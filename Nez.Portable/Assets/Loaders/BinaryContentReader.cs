using Microsoft.Xna.Framework.Content;
using System.IO;
using System.IO.Compression;

namespace Nez
{
	/// <summary>
	/// Reads a byte array, either uncompressed or compressed with GZip.
	/// Useful for loading and managing whole binary files via the Content Pipeline.
	/// To decompress the data via GZip, the array length must be negative.
	/// </summary>
	internal class BinaryContentReader : ContentTypeReader<byte[]>
	{
		protected override byte[] Read(ContentReader input, byte[] existingInstance)
		{
			//Read data from file
			int length = input.ReadInt32();
			bool compressed = length < 0;
			length = System.Math.Abs(length);
			byte[] data = input.ReadBytes(length);
			byte[] decomp = new byte[length];

			if (!compressed) return data;

			//Decompress byte stream via gzip
			using (var msi = new MemoryStream(data))
			using (var gs = new GZipStream(msi, CompressionMode.Decompress))
				gs.Read(decomp, 0, data.Length);

			return decomp;
		}
	}
}
