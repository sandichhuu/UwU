using System;
using System.Buffers;
using System.Collections.Generic;

namespace UwU.EasyData
{
    public class TableSetIO
    {
        private readonly TableSet tableSet;
        private readonly List<TableIO> tableIOs;

        public TableSet TableSet => this.tableSet;
        public IReadOnlyList<TableIO> TableIOs => this.tableIOs;

        public TableSetIO()
        {
            this.tableSet = new TableSet();
            this.tableIOs = new List<TableIO>();
        }

        public void ReadFromBytes(ReadOnlySpan<byte> bytes)
        {
            var pos = 0;
            var tableCount = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int)));
            pos += sizeof(int);

            this.tableSet.tables.Clear();
            this.tableIOs.Clear();

            for (var i = 0; i < tableCount; i++)
            {
                var tableBytesLen = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int)));
                pos += sizeof(int);

                var tableBytes = bytes.Slice(pos, tableBytesLen);
                pos += tableBytesLen;

                var tableIO = new TableIO();
                tableIO.ReadFromBytes(tableBytes);

                this.tableSet.tables.Add(tableIO.Table);
                this.tableIOs.Add(tableIO);
            }
        }

        public byte[] WriteToBytes()
        {
            var writer = new ArrayBufferWriter<byte>();
            WriteInt32(ref writer, this.tableSet.tables.Count);

            for (var i = 0; i < this.tableIOs.Count; i++)
            {
                var tableBytes = this.tableIOs[i].WriteToBytes();
                WriteInt32(ref writer, tableBytes.Length);
                WriteBytes(ref writer, tableBytes);
            }

            return writer.WrittenSpan.ToArray();
        }

        public void AddTable(string tableName)
        {
            var tableIO = new TableIO();
            tableIO.Table.tableName = tableName;
            tableIO.AddColumn("Id", ColumnType.Int);
            tableIO.AddColumn("Name", ColumnType.String);

            this.tableSet.tables.Add(tableIO.Table);
            this.tableIOs.Add(tableIO);
        }

        public void RemoveTable(int index)
        {
            if (index < 0 || index >= this.tableSet.tables.Count) return;
            this.tableSet.tables.RemoveAt(index);
            this.tableIOs.RemoveAt(index);
        }

        private static void WriteInt32(ref ArrayBufferWriter<byte> w, int value)
        {
            var s = w.GetSpan(4);
            BitConverter.TryWriteBytes(s, value);
            w.Advance(4);
        }

        private static void WriteBytes(ref ArrayBufferWriter<byte> w, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0) return;
            var s = w.GetSpan(data.Length);
            data.CopyTo(s);
            w.Advance(data.Length);
        }
    }
}