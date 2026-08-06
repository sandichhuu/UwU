using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UwU.Helpers;
using UwU.IO;

namespace UwU.EasyData
{
    public static class EasyDataSetReader
    {
        /// <summary>
        /// [SLOW_WAY] Take all records.
        /// </summary>
        public static CoroutineHelper.CoroutineTask<List<T>> All<T>(string filePath, IOType ioType = IOType.Persistent) where T : TableDataBase, new()
        {
            return CoroutineHelper.Start<List<T>>(Internal());
            IEnumerator Internal()
            {
                var loadBytesTask = CoroutineHelper.Start<byte[]>(ReadBytes(filePath, ioType));
                yield return loadBytesTask;
                yield return loadBytesTask.Result != null ? MappingUtility.MapBytesToList<T>(loadBytesTask.Result) : new List<T>();
            }
        }

        /// <summary>
        /// [SLOW_WAY] Find records.
        /// </summary>
        public static CoroutineHelper.CoroutineTask<List<T>> Where<T>(string filePath, Func<T, bool> predicate, IOType ioType) where T : TableDataBase, new()
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

        /// <summary>
        /// [SLOW_WAY] Find 1 record.
        /// </summary>
        public static CoroutineHelper.CoroutineTask<T> First<T>(string filePath, Func<T, bool> predicate, IOType ioType) where T : TableDataBase, new()
        {
            return CoroutineHelper.Start<T>(Internal());
            IEnumerator Internal()
            {
                var task = All<T>(filePath, ioType);
                yield return task;
                var allItems = task.Result;
                for (var i = 0; i < allItems.Count; i++)
                {
                    if (predicate(allItems[i]))
                    {
                        yield return allItems[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// [SLOW_WAY] Take 1 record.
        /// </summary>
        public static CoroutineHelper.CoroutineTask<T> Index<T>(string filePath, int rowIndex, IOType ioType) where T : TableDataBase, new()
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
                yield return PersistentIO.ReadAll($"{filePath}.{Config.TABLE_SET_DATA_EXT}");
            }
            else if (ioType == IOType.StreamingAssets)
            {
                yield return StreamingAssetsIO.Load($"{filePath}.{Config.TABLE_SET_DATA_EXT}");
            }
            else if (ioType == IOType.Resources)
            {
                yield return Resources.Load<TableSetAsset>(filePath).bytes;
            }
            else
            {
                yield return null;
            }
        }

        /// <summary>
        /// [FAST_WAY] Find 1 record directly from file, without read all file content.
        /// Warning: This function only working with data file on persistentDataPath
        /// </summary>
        public static CoroutineHelper.CoroutineTask<T> First<T>(
            string filePath,
            Func<T, bool> predicate) where T : TableDataBase, new()
        {
            return CoroutineHelper.Start<T>(Internal());

            IEnumerator Internal()
            {
                var tableName = Table.GetTableNameFromAttribute<T>();
                if (string.IsNullOrEmpty(tableName))
                {
                    Debug.LogError($"[EasyDataSetReader] Type {typeof(T).Name} missing [TableData] attribute.");
                    yield return default(T);
                    yield break;
                }

                var tableIO = LoadSingleTableByName($"{filePath}.{Config.TABLE_SET_DATA_EXT}", tableName);
                if (tableIO == null)
                {
                    yield return default(T);
                    yield break;
                }

                var rowCount = tableIO.Table.rowCount;
                for (var i = 0; i < rowCount; i++)
                {
                    var item = MappingUtility.MapRowToInstance<T>(tableIO, i);
                    if (item != null && predicate(item))
                    {
                        yield return item;
                        break;
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// [FAST_WAY] Find records directly from file, without read all file content.
        /// Warning: This function only working with data file on persistentDataPath
        /// </summary>
        public static CoroutineHelper.CoroutineTask<List<T>> Where<T>(
            string filePath,
            Func<T, bool> predicate) where T : TableDataBase, new()
        {
            return CoroutineHelper.Start<List<T>>(Internal());

            IEnumerator Internal()
            {
                var tableName = Table.GetTableNameFromAttribute<T>();
                if (string.IsNullOrEmpty(tableName))
                {
                    Debug.LogError($"[EasyDataSetReader] Type {typeof(T).Name} missing [TableData] attribute.");
                    yield return default(T);
                    yield break;
                }

                var tableIO = LoadSingleTableByName($"{filePath}.{Config.TABLE_SET_DATA_EXT}", tableName);
                if (tableIO == null)
                {
                    yield return default(T);
                    yield break;
                }

                var results = new List<T>();
                var rowCount = tableIO.Table.rowCount;
                for (var i = 0; i < rowCount; i++)
                {
                    var item = MappingUtility.MapRowToInstance<T>(tableIO, i);
                    if (item != null && predicate(item))
                    {
                        results.Add(item);
                    }
                }

                yield return results;
            }
        }

        /// <summary>
        /// [FAST_WAY] Take 1 record directly from file with index, without read all file content.
        /// Warning: This function only working with data file on persistentDataPath
        /// </summary>
        public static CoroutineHelper.CoroutineTask<T> Index<T>(string filePath, int rowIndex) where T : TableDataBase, new()
        {
            return CoroutineHelper.Start<T>(Internal());
            IEnumerator Internal()
            {
                var tableName = Table.GetTableNameFromAttribute<T>();
                if (string.IsNullOrEmpty(tableName))
                {
                    Debug.LogError($"[EasyDataSetReader] Type {typeof(T).Name} missing [TableData] attribute.");
                    yield return default(T);
                    yield break;
                }

                var tableIO = LoadSingleTableByName($"{filePath}.{Config.TABLE_SET_DATA_EXT}", tableName);
                if (tableIO == null)
                {
                    yield return default(T);
                    yield break;
                }

                var item = MappingUtility.MapRowToInstance<T>(tableIO, rowIndex);
                if (item != null)
                {
                    yield return item;
                }
                else
                {
                    yield return null;
                }
            }
        }

        private static TableIO LoadSingleTableByName(string filePath, string tableName)
        {
            var headerBytes = PersistentIO.ReadSegment(filePath, 0, sizeof(int));
            if (headerBytes == null || headerBytes.Length < sizeof(int))
            {
                Debug.LogError($"[EasyDataSetReader] Cannot read header from {filePath}");
                return null;
            }

            var tableCount = BitConverter.ToInt32(headerBytes, 0);
            long currentOffset = sizeof(int);

            for (var i = 0; i < tableCount; i++)
            {
                var lenBytes = PersistentIO.ReadSegment(filePath, currentOffset, sizeof(int));
                if (lenBytes == null || lenBytes.Length < sizeof(int))
                {
                    Debug.LogError($"[EasyDataSetReader] Cannot read tableBytesLen at offset {currentOffset}");
                    return null;
                }

                var tableBytesLen = BitConverter.ToInt32(lenBytes, 0);
                var dataOffset = currentOffset + sizeof(int);
                var peekSize = Math.Min(tableBytesLen, 256); // 256 bytes đủ cho hầu hết tableName
                var peekBytes = PersistentIO.ReadSegment(filePath, dataOffset, peekSize);
                if (peekBytes == null)
                {
                    Debug.LogError($"[EasyDataSetReader] Cannot peek table data at offset {dataOffset}");
                    return null;
                }

                var peekSpan = new ReadOnlySpan<byte>(peekBytes);
                if (peekSpan.Length >= sizeof(int))
                {
                    var nameLen = BitConverter.ToInt32(peekSpan.Slice(0, sizeof(int)));
                    if (nameLen > 0 && nameLen <= peekSpan.Length - sizeof(int))
                    {
                        var name = System.Text.Encoding.UTF8.GetString(peekSpan.Slice(sizeof(int), nameLen));
                        if (name == tableName)
                        {
                            var tableBytes = PersistentIO.ReadSegment(filePath, dataOffset, tableBytesLen);
                            if (tableBytes == null || tableBytes.Length != tableBytesLen)
                            {
                                Debug.LogError($"[EasyDataSetReader] Short read: got {tableBytes?.Length ?? 0}/{tableBytesLen}");
                                return null;
                            }

                            var tableIO = new TableIO();
                            tableIO.ReadFromBytes(tableBytes);
                            return tableIO;
                        }
                    }
                }

                currentOffset = dataOffset + tableBytesLen;
            }

            Debug.LogWarning($"[EasyDataSetReader] Table '{tableName}' not found in {filePath}");
            return null;
        }
    }
}