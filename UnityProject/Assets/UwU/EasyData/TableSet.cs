using System;
using System.Collections.Generic;

namespace UwU.EasyData
{
    [Serializable]
    public class TableSet
    {
        public List<Table> tables;

        public TableSet()
        {
            this.tables = new();
        }
    }
}