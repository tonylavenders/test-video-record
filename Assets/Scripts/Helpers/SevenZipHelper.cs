using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace TVR.Helpers {
	public static class SevenZipHelper {
		static int dictionary = 1 << 23;
		static bool eos = false;
		static CoderPropID[] propIDs = {
			CoderPropID.DictionarySize,
			CoderPropID.PosStateBits,
			CoderPropID.LitContextBits,
			CoderPropID.LitPosBits,
			CoderPropID.Algorithm,
			CoderPropID.NumFastBytes,
			CoderPropID.MatchFinder,
			CoderPropID.EndMarker
		};
		
		// these are the default properties, keeping it simple for now:
		static object[] properties = {
			(Int32)(dictionary),
			(Int32)(2),
			(Int32)(3),
			(Int32)(0),
			(Int32)(2),
			(Int32)(128),
			"bt4",
			eos
		};
		
		public static byte[] Compress(byte[] inputBytes) {
			MemoryStream inStream = new MemoryStream(inputBytes);
			MemoryStream outStream = new MemoryStream();
			SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
			encoder.SetCoderProperties(propIDs, properties);
			encoder.WriteCoderProperties(outStream);
			long fileSize = inStream.Length;
			for (int i = 0; i < 8; i++)
				outStream.WriteByte((Byte)(fileSize >> (8 * i)));
			encoder.Code(inStream, outStream, -1, -1, null);
			return outStream.ToArray();
		}

		public static byte[] Decompress(byte[] inputBytes) {
			MemoryStream newInStream = new MemoryStream(inputBytes);
			SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
			newInStream.Seek(0, 0);
			MemoryStream newOutStream = new MemoryStream();
			byte[] properties2 = new byte[5];
			if (newInStream.Read(properties2, 0, 5) != 5)
				throw (new Exception("input .lzma is too short"));
			long outSize = 0;
			for (int i = 0; i < 8; i++) {
				int v = newInStream.ReadByte();
				if (v < 0)
					throw (new Exception("Can't Read 1"));
				outSize |= ((long)(byte)v) << (8 * i);
			}
			decoder.SetDecoderProperties(properties2);
			long compressedSize = newInStream.Length - newInStream.Position;
			decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);
			return newOutStream.ToArray();
			/*byte[] b = newOutStream.ToArray();
			return b;*/
		}
		
		public static bool Compress(byte[] inputBytes, string FileName, bool Overwrite) {
			if (File.Exists(FileName)) {
				if(Overwrite) {
            		File.Delete(FileName);
				} else {
					return false;
				}
			}
			
			//MemoryStream inStream = new MemoryStream(inputBytes);
			using(FileStream outStream = File.Create(FileName)) {
				MemoryStream inStream = new MemoryStream(inputBytes);
				SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
				encoder.SetCoderProperties(propIDs, properties);
				encoder.WriteCoderProperties(outStream);
				long fileSize = inStream.Length;
				for (int i = 0; i < 8; i++)
					outStream.WriteByte((Byte)(fileSize >> (8 * i)));
				encoder.Code(inStream, outStream, -1, -1, null);
			}
			return true;
		}
		
		public static bool Decompress(string FileName, out  byte[] outputBytes) {
			outputBytes = null;
			if (!File.Exists(FileName))
				return false;
			
			using(FileStream newInStream = File.OpenRead(FileName)) {
				SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
				newInStream.Seek(0, 0);
				MemoryStream newOutStream = new MemoryStream();
				byte[] properties2 = new byte[5];
				if (newInStream.Read(properties2, 0, 5) != 5)
					throw (new Exception("input .lzma is too short"));
				long outSize = 0;
				for (int i = 0; i < 8; i++) {
					int v = newInStream.ReadByte();
					if (v < 0)
						throw (new Exception("Can't Read 1"));
					outSize |= ((long)(byte)v) << (8 * i);
				}
				decoder.SetDecoderProperties(properties2);
				long compressedSize = newInStream.Length - newInStream.Position;
				decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);
				outputBytes = newOutStream.ToArray();
			}
			return true;
		}
	}
}