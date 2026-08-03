using System;
using System.Collections.Generic;

namespace UwU.EasyData
{
    /// <summary>
    /// File structure:
    /// | Table Name | Row Count  | Column Count |
    /// | Column Header (n-block) | Column Byte Size (n-block) | Chunk Data Type (n-block) |
    /// | Chunk Offset (n-block)  | Chunk Offset Table Size (n-block) |
    /// | Chunk Data (n-block)    |
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

        public void AddColumn(string header, ColumnType columnType)
        {
            this.columns.Add(new Column
            {
                header = header,
                columnType = columnType
            });
        }
    }
}