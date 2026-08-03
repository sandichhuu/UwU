using System.Collections.Generic;

namespace UwU.EasyData
{
    public class Column
    {
        public string header;
        public int byteSize;
        public ColumnType columnType;
        public List<uint> offsets;

        public Column()
        {
            this.offsets = new();
        }
    }
}