using System;
using System.Collections;
using System.Collections.Generic;
using UwU.Helpers;
using UwU.IO;

namespace UwU.EasyData
{
    public static class EasyDataSetReader
    {
        public static CoroutineHelper.CoroutineTask<List<T>> All<T>(string filePath, IOType ioType = IOType.Persistent) where T : new()
        {
            return CoroutineHelper.Start<List<T>>(Internal());
            IEnumerator Internal()
            {
                var loadBytesTask = CoroutineHelper.Start<byte[]>(ReadBytes(filePath, ioType));
                yield return loadBytesTask;
                yield return loadBytesTask.Result != null ? EasyDataMappingUtility.MapBytesToList<T>(loadBytesTask.Result) : new List<T>();
            }
        }

        public static CoroutineHelper.CoroutineTask<List<T>> Find<T>(string filePath, Func<T, bool> predicate, IOType ioType = IOType.Persistent) where T : new()
        {
            return CoroutineHelper.Start<List<T>>(Internal());
            IEnumerator Internal()
            {
                var task = All<T>(filePath, ioType);
                yield return task;
                var allItems = task.Result;
                var results = new List<T>();
                for (var i = 0; i < allItems.Count; i++)
                {
                    if (predicate(allItems[i]))
                    {
                        results.Add(allItems[i]);
                    }
                }
                yield return results;
            }
        }

        public static CoroutineHelper.CoroutineTask<T> Index<T>(string filePath, int rowIndex, IOType ioType = IOType.Persistent) where T : new()
        {
            return CoroutineHelper.Start<T>(Internal());
            IEnumerator Internal()
            {
                var task = All<T>(filePath, ioType);
                yield return task;
                var allItems = task.Result;
                if (rowIndex >= 0 && rowIndex < allItems.Count)
                {
                    yield return allItems[rowIndex];
                    yield break;
                }
                yield return null;
            }
        }

        private static IEnumerator ReadBytes(string filePath, IOType ioType)
        {
            if (ioType == IOType.Persistent)
            {
                yield return PersistentIO.ReadAll(filePath);
            }
            else if (ioType == IOType.StreamingAssets)
            {
                yield return StreamingAssetsIO.Load(filePath);
            }
            else if (ioType == IOType.Resources)
            {
                yield return ResourcesIO.Load(filePath);
            }
        }
    }
}