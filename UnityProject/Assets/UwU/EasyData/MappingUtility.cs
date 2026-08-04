using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UwU.EasyData.Attributes;

namespace UwU.EasyData
{
    public static class EasyDataMappingUtility
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

                if (column.columnType == ColumnType.String)
                {
                    var str = tableIO.GetString(colIndex, rowIndex);
                    if (targetType == typeof(string)) return str;
                    return Convert.ChangeType(str, targetType);
                }

                var span = tableIO.GetCellData(colIndex, rowIndex);
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
    }
}