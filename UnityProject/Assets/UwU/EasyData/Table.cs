using System;
using System.Collections.Generic;

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
    }
}