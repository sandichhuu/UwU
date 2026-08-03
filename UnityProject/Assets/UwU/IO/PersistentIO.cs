using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UwU.IO
{
    public static class PersistentIO
    {
        private static string GetFullPath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);

        public static byte[] ReadSegment(string fileName, long offset, int length)
        {
            string path = GetFullPath(fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"[PersistentIO] File not found: {path}");
                return null;
            }

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (offset < 0 || offset + length > fs.Length)
            {
                Debug.LogError($"[PersistentIO] Segment out of range! offset={offset}, len={length}, fileSize={fs.Length}");
                return null;
            }

            fs.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[length];
            int bytesRead = fs.Read(buffer, 0, length);

            if (bytesRead != length)
            {
                Debug.LogWarning($"[PersistentIO] Short read: {bytesRead}/{length}");
                Array.Resize(ref buffer, bytesRead);
            }

            return buffer;
        }

        public static Task<byte[]> ReadSegmentAsync(string fileName, long offset, int length) => Task.Run(() => ReadSegment(fileName, offset, length));

        public static byte[] ReadAll(string fileName)
        {
            string path = GetFullPath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"[PersistentIO] File not found: {path}");
                return null;
            }
            return File.ReadAllBytes(path);
        }

        public static bool Write(string fileName, byte[] data)
        {
            try
            {
                string path = GetFullPath(fileName);
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentIO] Write error: {ex.Message}");
                return false;
            }
        }

        public static bool Exists(string fileName) => File.Exists(GetFullPath(fileName));

        public static long GetFileSize(string fileName)
        {
            string path = GetFullPath(fileName);
            return File.Exists(path) ? new FileInfo(path).Length : -1;
        }

        public static bool Delete(string fileName)
        {
            try
            {
                string path = GetFullPath(fileName);
                if (File.Exists(path)) File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentIO] Delete error: {ex.Message}");
                return false;
            }
        }
    }
}