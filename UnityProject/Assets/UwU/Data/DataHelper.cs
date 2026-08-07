using System;
using System.IO;
using System.IO.Compression;

namespace UwU.Data
{
    public static class DataHelper
    {
        public static uint Checksum(Span<byte> data, Span<byte> keys)
        {
            uint hash = 2166136261u;

            for (int i = 0; i < keys.Length; i++)
            {
                hash ^= keys[i];
                hash *= 16777619u;
            }

            for (int i = 0; i < data.Length; i++)
            {
                hash ^= (uint)(data[i] + (i * keys.Length));
                hash *= 16777619u;
            }

            hash ^= hash >> 16;
            hash *= 0x85ebca6bu;
            hash ^= hash >> 13;

            return hash;
        }

        public static byte[] Compress(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gZipStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using (var inputStream = new MemoryStream(compressedData))
            {
                using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        gZipStream.CopyTo(outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}