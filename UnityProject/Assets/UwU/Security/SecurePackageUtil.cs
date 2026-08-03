using System;
using System.IO;
using System.Text;

namespace UwU.Security
{
    public static class SecurePackageUtil
    {
        public static byte[] Pack(ReadOnlySpan<byte> originalData, string expectedKey, string customHeader = "SECURE_PKG")
        {
            if (originalData == null) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrEmpty(expectedKey)) throw new ArgumentException("Key cannot be null or empty.", nameof(expectedKey));
            if (string.IsNullOrEmpty(customHeader)) throw new ArgumentException("Header cannot be null or empty.", nameof(customHeader));

            var headerBytes = Encoding.UTF8.GetBytes(customHeader);
            var keyBytes = Encoding.UTF8.GetBytes(expectedKey);

            var totalLength = 2 + headerBytes.Length
                            + 2 + keyBytes.Length
                            + 4 + originalData.Length;

            var package = new byte[totalLength];
            var offset = 0;

            BitConverter.TryWriteBytes(package.AsSpan(offset, 2), (ushort)headerBytes.Length);
            offset += 2;
            headerBytes.CopyTo(package.AsSpan(offset));
            offset += headerBytes.Length;

            BitConverter.TryWriteBytes(package.AsSpan(offset, 2), (ushort)keyBytes.Length);
            offset += 2;
            keyBytes.CopyTo(package.AsSpan(offset));
            offset += keyBytes.Length;

            BitConverter.TryWriteBytes(package.AsSpan(offset, 4), originalData.Length);
            offset += 4;
            originalData.CopyTo(package.AsSpan(offset));

            return package;
        }

        public static bool IsPacked(ReadOnlySpan<byte> package, string expectedHeader)
        {
            if (package.Length < 2) return false;
            if (string.IsNullOrEmpty(expectedHeader)) return false;

            var offset = 0;

            try
            {
                var headerLen = BitConverter.ToUInt16(package.Slice(offset, 2));
                offset += 2;

                if (package.Length < offset + headerLen) return false;

                var header = Encoding.UTF8.GetString(package.Slice(offset, headerLen));
                return header == expectedHeader;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPacked(string filePath, string expectedHeader)
        {
            if (!File.Exists(filePath)) return false;
            if (string.IsNullOrEmpty(expectedHeader)) return false;

            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length < 2) return false;

                var lenBuffer = new byte[2];
                if (fs.Read(lenBuffer, 0, 2) != 2) return false;
                var headerLen = BitConverter.ToUInt16(lenBuffer, 0);

                if (fs.Length < 2 + headerLen) return false;

                var headerBuffer = new byte[headerLen];
                if (fs.Read(headerBuffer, 0, headerLen) != headerLen) return false;

                var header = Encoding.UTF8.GetString(headerBuffer);
                return header == expectedHeader;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryUnpack(ReadOnlySpan<byte> package, string inputKey, string expectedHeader, out byte[] extractedData)
        {
            extractedData = null;
            if (package.IsEmpty || string.IsNullOrEmpty(inputKey) || string.IsNullOrEmpty(expectedHeader))
                return false;

            var offset = 0;

            try
            {
                if (package.Length < offset + 2) return false;
                var headerLen = BitConverter.ToUInt16(package.Slice(offset, 2));
                offset += 2;

                if (package.Length < offset + headerLen) return false;
                var header = Encoding.UTF8.GetString(package.Slice(offset, headerLen));
                offset += headerLen;

                if (header != expectedHeader) return false;

                if (package.Length < offset + 2) return false;
                var keyLen = BitConverter.ToUInt16(package.Slice(offset, 2));
                offset += 2;

                if (package.Length < offset + keyLen) return false;
                var key = Encoding.UTF8.GetString(package.Slice(offset, keyLen));
                offset += keyLen;

                if (key != inputKey) return false;

                if (package.Length < offset + 4) return false;
                var dataLen = BitConverter.ToInt32(package.Slice(offset, 4));
                offset += 4;

                if (package.Length < offset + dataLen) return false;

                extractedData = package.Slice(offset, dataLen).ToArray();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}