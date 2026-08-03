using System.Buffers;
using System.Collections.Generic;

namespace UwU.EasyData.Controllers
{
    public class TableWriter
    {
        private readonly Table table;
        private readonly List<ArrayBufferWriter<byte>> buffers;

        public TableWriter(Table table)
        {
            this.table = table;
            this.buffers = new();
        }

        public void AddColumn(Column column)
        {
            this.table.columns.Add(column);
            this.buffers.Add(new ArrayBufferWriter<byte>());
        }

        public void InsertColumnAtRight(int columnIndex, Column column)
        {
            this.table.columns.Insert(columnIndex + 1, column);
            this.buffers.Insert(columnIndex + 1, new ArrayBufferWriter<byte>());
        }

        public void InsertColumnAtLeft(int columnIndex, Column column)
        {
            this.table.columns.Insert(columnIndex, column);
            this.buffers.Insert(columnIndex, new ArrayBufferWriter<byte>());
        }

        public void RemoveColumn(int columnIndex)
        {
            this.table.columns.RemoveAt(columnIndex);
            this.buffers.RemoveAt(columnIndex);
        }

        public ArrayBufferWriter<byte> GetBuffer(int columnIndex)
        {
            return this.buffers[columnIndex];
        }
    }
}