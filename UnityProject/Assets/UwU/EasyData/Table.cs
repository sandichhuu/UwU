using System;
using System.Collections.Generic;

namespace UwU.EasyData
{
    [Serializable]
    public class Table
    {
        public readonly List<Column> columns;
        public string tableName;
        public int rowCount;

        public Table()
        {
            this.columns = new List<Column>();
            this.rowCount = 0;
        }
    }
}