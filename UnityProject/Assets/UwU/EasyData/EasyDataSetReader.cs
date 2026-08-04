using System;
using System.Collections.Generic;
using UwU.Common;
using UwU.IO;

namespace UwU.EasyData
{
    public static class EasyDataSetReader
    {
        public static List<T> All<T>(string filePath, IOType ioType = IOType.Persistent) where T : new()
        {
            var bytes = ReadBytes(filePath, ioType);
            return bytes != null ? EasyDataMappingUtility.MapBytesToList<T>(bytes) : new List<T>();
        }

        public static List<T> Find<T>(string filePath, Func<T, bool> predicate, IOType ioType = IOType.Persistent) where T : new()
        {
            var allItems = All<T>(filePath, ioType);
            var results = new List<T>();

            for (var i = 0; i < allItems.Count; i++)
            {
                if (predicate(allItems[i]))
                {
                    results.Add(allItems[i]);
                }
            }

            return results;
        }

        public static T Index<T>(string filePath, int rowIndex, IOType ioType = IOType.Persistent) where T : new()
        {
            var allItems = All<T>(filePath, ioType);
            if (rowIndex >= 0 && rowIndex < allItems.Count)
            {
                return allItems[rowIndex];
            }
            return default;
        }

        private static byte[] ReadBytes(string filePath, IOType ioType)
        {
            if (ioType == IOType.Persistent)
            {
                return PersistentIO.ReadAll(filePath);
            }

            var bytes = default(byte[]);

            if (ioType == IOType.StreamingAssets)
            {
                CoroutineUtility.StartCoroutineStatic(StreamingAssetsIO.Load(filePath, b => bytes = b));
            }
            else if (ioType == IOType.Resources)
            {
                CoroutineUtility.StartCoroutineStatic(ResourcesIO.Load(filePath, b => bytes = b));
            }

            return bytes;
        }
    }
}