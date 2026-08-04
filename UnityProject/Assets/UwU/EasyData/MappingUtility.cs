using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UwU.EasyData.Attributes;

namespace UwU.EasyData
{
    public static class MappingUtility
    {
        public static T MapRowToInstance<T>(TableIO tableIO, int rowIndex) where T : new()
        {
            var tableType = typeof(T);
            var attr = tableType.GetCustomAttribute<TableDataAttribute>();
            var table = tableIO.Table;
            var fieldMappings = GetFieldMappings(tableType, table);
            var instance = new T();

            foreach (var mapping in fieldMappings)
            {
                var val = ConvertCellToObject(tableIO, mapping.columnIndex, rowIndex, mapping.memberType);
                if (val != null)
                {
                    mapping.SetMember(instance, val);
                }
            }

            return instance;
        }

        public static List<T> MapBytesToList<T>(byte[] bytes) where T : new()
        {
            var tableType = typeof(T);
            var attr = tableType.GetCustomAttribute<TableDataAttribute>();
            var targetTableName = attr != null ? attr.TableName : tableType.Name;

            var tableIO = FindTableIO(bytes, targetTableName);
            if (tableIO == null)
            {
                Debug.LogError($"[EasyData] Could not find table '{targetTableName}' in data source.");
                return new List<T>();
            }

            var results = new List<T>();
            var table = tableIO.Table;

            var fieldMappings = GetFieldMappings(tableType, table);

            for (var r = 0; r < table.rowCount; r++)
            {
                var instance = new T();

                foreach (var mapping in fieldMappings)
                {
                    var val = ConvertCellToObject(tableIO, mapping.columnIndex, r, mapping.memberType);
                    if (val != null)
                    {
                        mapping.SetMember(instance, val);
                    }
                }

                results.Add(instance);
            }

            return results;
        }

        private static TableIO FindTableIO(byte[] bytes, string tableName)
        {
            try
            {
                var tableSetIO = new TableSetIO();
                tableSetIO.ReadFromBytes(bytes);

                foreach (var io in tableSetIO.TableIOs)
                {
                    if (io.Table.tableName == tableName)
                        return io;
                }
            }
            catch
            {
                try
                {
                    var singleTableIO = new TableIO();
                    singleTableIO.ReadFromBytes(bytes);
                    if (singleTableIO.Table.tableName == tableName || string.IsNullOrEmpty(tableName))
                        return singleTableIO;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EasyData] Failed to parse data bytes: {ex.Message}");
                }
            }

            return null;
        }

        private static List<MemberMapping> GetFieldMappings(Type type, Table table)
        {
            var mappings = new List<MemberMapping>();
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    var fieldAttr = field.GetCustomAttribute<TableFieldAttribute>();
                    var colName = fieldAttr != null ? fieldAttr.ColumnName : field.Name;
                    var colIndex = FindColumnIndex(table, colName);

                    if (colIndex != -1)
                    {
                        mappings.Add(new MemberMapping
                        {
                            memberType = field.FieldType,
                            columnIndex = colIndex,
                            SetMember = (obj, val) => field.SetValue(obj, val)
                        });
                    }
                }
                else if (member is PropertyInfo prop && prop.CanWrite)
                {
                    var propAttr = prop.GetCustomAttribute<TableFieldAttribute>();
                    var colName = propAttr != null ? propAttr.ColumnName : prop.Name;
                    var colIndex = FindColumnIndex(table, colName);

                    if (colIndex != -1)
                    {
                        mappings.Add(new MemberMapping
                        {
                            memberType = prop.PropertyType,
                            columnIndex = colIndex,
                            SetMember = (obj, val) => prop.SetValue(obj, val)
                        });
                    }
                }
            }

            return mappings;
        }

        private static int FindColumnIndex(Table table, string columnName)
        {
            for (var i = 0; i < table.columns.Count; i++)
            {
                if (table.columns[i].header == columnName)
                    return i;
            }
            return -1;
        }

        private static object ConvertCellToObject(TableIO tableIO, int colIndex, int rowIndex, Type targetType)
        {
            try
            {
                var column = tableIO.Table.columns[colIndex];
                var span = tableIO.GetCellData(colIndex, rowIndex);

                if (column.columnType == ColumnType.String)
                {
                    var str = Encoding.UTF8.GetString(span);
                    if (targetType == typeof(string)) return str;
                    return Convert.ChangeType(str, targetType);
                }

                if (Extensions.IsArrayType(column.columnType))
                {
                    if (span.Length == 0)
                    {
                        return column.columnType switch
                        {
                            ColumnType.IntArray => Array.Empty<int>(),
                            ColumnType.LongArray => Array.Empty<long>(),
                            ColumnType.FloatArray => Array.Empty<float>(),
                            ColumnType.DoubleArray => Array.Empty<double>(),
                            ColumnType.BoolArray => Array.Empty<bool>(),
                            _ => null
                        };
                    }

                    switch (column.columnType)
                    {
                        case ColumnType.IntArray:
                            {
                                var count = span.Length / sizeof(int);
                                var arr = new int[count];
                                Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                                return arr;
                            }
                        case ColumnType.LongArray:
                            {
                                var count = span.Length / sizeof(long);
                                var arr = new long[count];
                                Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                                return arr;
                            }
                        case ColumnType.FloatArray:
                            {
                                var count = span.Length / sizeof(float);
                                var arr = new float[count];
                                Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                                return arr;
                            }
                        case ColumnType.DoubleArray:
                            {
                                var count = span.Length / sizeof(double);
                                var arr = new double[count];
                                Buffer.BlockCopy(span.ToArray(), 0, arr, 0, span.Length);
                                return arr;
                            }
                        case ColumnType.BoolArray:
                            {
                                var arr = new bool[span.Length];
                                for (int i = 0; i < span.Length; i++)
                                    arr[i] = span[i] != 0;
                                return arr;
                            }
                        case ColumnType.StringArray:
                            {
                                var arr = DeserializeStringArray(span);
                                return targetType.IsAssignableFrom(typeof(string[])) ? arr : Convert.ChangeType(arr, targetType);
                            }
                    }
                }

                if (span.Length == 0) return null;

                switch (column.columnType)
                {
                    case ColumnType.SByte:
                        var sbyteVal = (sbyte)span[0];
                        return Convert.ChangeType(sbyteVal, targetType);

                    case ColumnType.Byte:
                        var byteVal = span[0];
                        return Convert.ChangeType(byteVal, targetType);

                    case ColumnType.Short:
                        var shortVal = BitConverter.ToInt16(span);
                        return Convert.ChangeType(shortVal, targetType);

                    case ColumnType.UShort:
                        var ushortVal = BitConverter.ToUInt16(span);
                        return Convert.ChangeType(ushortVal, targetType);

                    case ColumnType.Int:
                        var intVal = BitConverter.ToInt32(span);
                        return Convert.ChangeType(intVal, targetType);

                    case ColumnType.UInt:
                        var uintVal = BitConverter.ToUInt32(span);
                        return Convert.ChangeType(uintVal, targetType);

                    case ColumnType.Long:
                        var longVal = BitConverter.ToInt64(span);
                        return Convert.ChangeType(longVal, targetType);

                    case ColumnType.ULong:
                        var ulongVal = BitConverter.ToUInt64(span);
                        return Convert.ChangeType(ulongVal, targetType);

                    case ColumnType.Float:
                        var floatVal = BitConverter.ToSingle(span);
                        return Convert.ChangeType(floatVal, targetType);

                    case ColumnType.Double:
                        var doubleVal = BitConverter.ToDouble(span);
                        return Convert.ChangeType(doubleVal, targetType);

                    case ColumnType.Bool:
                        var boolVal = BitConverter.ToBoolean(span);
                        return Convert.ChangeType(boolVal, targetType);

                    case ColumnType.Char:
                        var charVal = BitConverter.ToChar(span);
                        return Convert.ChangeType(charVal, targetType);

                    case ColumnType.Decimal:
                        if (span.Length >= 16)
                        {
                            var bits = new int[4];
                            Buffer.BlockCopy(span.ToArray(), 0, bits, 0, 16);
                            var decVal = new decimal(bits);
                            return Convert.ChangeType(decVal, targetType);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EasyData] Failed to convert cell [{rowIndex},{colIndex}] to {targetType.Name}: {ex.Message}");
            }

            return null;
        }

        private class MemberMapping
        {
            public Type memberType;
            public int columnIndex;
            public Action<object, object> SetMember;
        }

        public static byte[] SerializeStringArray(string[] values)
        {
            if (values == null || values.Length == 0)
                return BitConverter.GetBytes(0); // elemCount = 0

            var encoding = Encoding.UTF8;
            var elemCount = values.Length;

            var stringBytes = new byte[elemCount][];
            for (int i = 0; i < elemCount; i++)
                stringBytes[i] = encoding.GetBytes(values[i] ?? string.Empty);

            var headerSize = sizeof(int) + elemCount * sizeof(uint);
            var totalDataSize = 0;
            for (int i = 0; i < elemCount; i++)
                totalDataSize += stringBytes[i].Length;

            var result = new byte[headerSize + totalDataSize];

            BitConverter.TryWriteBytes(result.AsSpan(0, 4), elemCount);

            var currentOffset = headerSize;
            for (var i = 0; i < elemCount; i++)
            {
                BitConverter.TryWriteBytes(result.AsSpan(4 + i * 4, 4), (uint)currentOffset);
                currentOffset += stringBytes[i].Length;
            }

            var writePos = headerSize;
            for (var i = 0; i < elemCount; i++)
            {
                stringBytes[i].CopyTo(result, writePos);
                writePos += stringBytes[i].Length;
            }

            return result;
        }

        public static string[] DeserializeStringArray(ReadOnlySpan<byte> span)
        {
            if (span.Length < sizeof(int))
                return Array.Empty<string>();

            var elemCount = BitConverter.ToInt32(span[..4]);
            if (elemCount == 0)
                return Array.Empty<string>();

            var minHeaderSize = sizeof(int) + elemCount * sizeof(uint);
            if (span.Length < minHeaderSize)
                throw new InvalidDataException($"StringArray data too short: expected at least {minHeaderSize} bytes, got {span.Length}");

            var result = new string[elemCount];
            var encoding = Encoding.UTF8;

            for (int i = 0; i < elemCount; i++)
            {
                var strStart = (int)BitConverter.ToUInt32(span.Slice(4 + i * 4, 4));
                var strEnd = i + 1 < elemCount
                    ? (int)BitConverter.ToUInt32(span.Slice(4 + (i + 1) * 4, 4))
                    : span.Length;

                if (strStart > strEnd || strEnd > span.Length)
                    throw new InvalidDataException($"Invalid StringArray element offset: [{strStart}, {strEnd}) in span of length {span.Length}");

                result[i] = encoding.GetString(span[strStart..strEnd]);
            }

            return result;
        }
    }
}