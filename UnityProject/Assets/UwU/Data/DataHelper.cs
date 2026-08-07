using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace UwU.Data
{
    public static class DataHelper
    {
        private static readonly byte[] Salt = new byte[] { 0x42, 0x79, 0x74, 0x65, 0x50, 0x61, 0x63, 0x6B };
        private static readonly byte[] FixedIv = new byte[] { 0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

        public static byte[] Encrypt(byte[] data, string password)
        {
            using (var aes = Aes.Create())
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 1000))
                {
                    aes.Key = deriveBytes.GetBytes(32); // AES-256
                    aes.IV = FixedIv;
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] encryptedData, string password)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    using (var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 1000))
                    {
                        aes.Key = deriveBytes.GetBytes(32);
                        aes.IV = FixedIv;
                        using (var ms = new MemoryStream())
                        {
                            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedData, 0, encryptedData.Length);
                                cs.FlushFinalBlock();
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                throw new UnauthorizedAccessException("Invalid password or corrupted package file.");
            }
        }

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