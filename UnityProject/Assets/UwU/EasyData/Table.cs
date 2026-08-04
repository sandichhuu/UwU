using System;
using System.Collections.Generic;
using System.Reflection;
using UwU.EasyData.Attributes;

namespace UwU.EasyData
{
    /// <summary>
    /// Binary format (tuần tự theo cột):
    ///   [tableNameLen:i32][tableName:utf8]
    ///   [rowCount:i32]
    ///   [columnCount:i32]
    ///   × columnCount:
    ///     [headerLen:i32][header:utf8]
    ///     [byteSize:i32]
    ///     [columnType:i32]
    ///     [offsetCount:i32]
    ///     × offsetCount: [offset:u32]
    ///   × columnCount:
    ///     [dataLen:i32][data:bytes]
    /// </summary>
    [Serializable]
    public class Table
    {
        public string tableName;
        public int rowCount;
        public readonly List<Column> columns;

        public Table()
        {
            this.columns = new();
        }

        private static readonly Dictionary<Type, string> TableNameCache = new();
        public static string GetTableNameFromAttribute<T>()
        {
            var type = typeof(T);
            if (TableNameCache.TryGetValue(type, out var cached))
                return cached;

            var attr = type.GetCustomAttribute<TableDataAttribute>();
            var name = attr?.TableName;
            TableNameCache[type] = name;
            return name;
        }
    }
}