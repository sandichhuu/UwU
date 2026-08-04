using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static UnityEngine.Rendering.DebugUI;

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
    public class TableIO
    {
        private readonly Table table;
        private readonly List<ArrayBufferWriter<byte>> columnBuffers;
        private readonly List<int> columnCellSizes;

        public Table Table => this.table;
        public IReadOnlyList<ArrayBufferWriter<byte>> ColumnBuffers => this.columnBuffers;

        public TableIO()
        {
            this.table = new Table();
            this.columnBuffers = new List<ArrayBufferWriter<byte>>();
            this.columnCellSizes = new List<int>();
        }

        public void ReadFromBytes(ReadOnlySpan<byte> bytes)
        {
            var pos = 0;

            var nameLen = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
            this.table.tableName = Encoding.UTF8.GetString(bytes.Slice(pos, nameLen)); pos += nameLen;
            this.table.rowCount = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
            var colCount = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);

            this.table.columns.Clear();
            this.columnBuffers.Clear();

            for (var i = 0; i < colCount; i++)
            {
                var col = new Column();

                var hLen = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
                col.header = Encoding.UTF8.GetString(bytes.Slice(pos, hLen)); pos += hLen;

                col.byteSize = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
                col.columnType = (ColumnType)BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);

                var offCount = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
                col.offsets = new List<uint>(offCount);
                for (var j = 0; j < offCount; j++)
                {
                    col.offsets.Add(BitConverter.ToUInt32(bytes.Slice(pos, sizeof(int)))); pos += sizeof(int);
                }

                this.table.columns.Add(col);
            }

            this.columnCellSizes.Clear();

            for (var i = 0; i < colCount; i++)
            {
                var dataLen = BitConverter.ToInt32(bytes.Slice(pos, sizeof(int))); pos += sizeof(int);
                var writer = new ArrayBufferWriter<byte>();
                bytes.Slice(pos, dataLen).CopyTo(writer.GetSpan(dataLen));
                writer.Advance(dataLen);
                pos += dataLen;

                this.columnBuffers.Add(writer);

                if (this.table.columns[i].columnType == ColumnType.String)
                {
                    this.columnCellSizes.Add(0);
                }
                else
                {
                    if (this.table.rowCount <= 0 || dataLen % this.table.rowCount != 0)
                        throw new InvalidDataException($"Invalid fixed-size column data length: {dataLen}.");

                    this.columnCellSizes.Add(dataLen / this.table.rowCount);
                }

                this.table.columns[i].byteSize = dataLen;
            }
        }

        public ReadOnlySpan<byte> GetCellData(int columnIndex, int rowIndex)
        {
            ValidateColumnIndex(columnIndex);

            var column = this.table.columns[columnIndex];
            var span = this.columnBuffers[columnIndex].WrittenSpan;

            int offset;
            if (column.columnType == ColumnType.String)
            {
                ValidateRowIndex(column, rowIndex);
                offset = (int)column.offsets[rowIndex];
                var end = rowIndex + 1 < column.offsets.Count
                    ? (int)column.offsets[rowIndex + 1]
                    : span.Length;

                return span[offset..end];
            }

            ValidateRowIndex(rowIndex);

            var cellSize = GetCellSize(columnIndex);
            offset = rowIndex * cellSize;

            return span.Slice(offset, cellSize);
        }

        public ReadOnlySpan<byte> GetCellData(string header, int rowIndex) =>
            GetCellData(FindColumnIndex(header), rowIndex);

        public string GetString(int columnIndex, int rowIndex) =>
            Encoding.UTF8.GetString(GetCellData(columnIndex, rowIndex));

        public string GetString(string header, int rowIndex) =>
            GetString(FindColumnIndex(header), rowIndex);

        public void SetCellData(int columnIndex, int rowIndex, string value)
        {
            ValidateColumnIndex(columnIndex);
            var column = this.table.columns[columnIndex];

            if (column.columnType != ColumnType.String)
                throw new InvalidOperationException($"Column '{column.header}' is not a String column.");

            ValidateRowIndex(column, rowIndex);
            value ??= string.Empty;

            var newData = Encoding.UTF8.GetBytes(value);
            var oldBuffer = this.columnBuffers[columnIndex].WrittenSpan;

            var oldStart = (int)column.offsets[rowIndex];
            var oldEnd = rowIndex + 1 < column.offsets.Count
                ? (int)column.offsets[rowIndex + 1]
                : oldBuffer.Length;

            var oldLength = oldEnd - oldStart;
            var newLength = oldBuffer.Length - oldLength + newData.Length;
            var newWriter = new ArrayBufferWriter<byte>(newLength);

            oldBuffer[..oldStart].CopyTo(newWriter.GetSpan(oldStart));
            newWriter.Advance(oldStart);

            newData.CopyTo(newWriter.GetSpan(newData.Length));
            newWriter.Advance(newData.Length);

            var suffix = oldBuffer[oldEnd..];
            suffix.CopyTo(newWriter.GetSpan(suffix.Length));
            newWriter.Advance(suffix.Length);

            var delta = newData.Length - oldLength;
            column.byteSize = newWriter.WrittenCount;

            if (delta != 0)
            {
                for (var i = rowIndex + 1; i < column.offsets.Count; i++)
                {
                    column.offsets[i] = (uint)((int)column.offsets[i] + delta);
                }
            }

            this.columnBuffers[columnIndex] = newWriter;
        }

        public void SetCellData(int columnIndex, int rowIndex, ReadOnlySpan<byte> data)
        {
            ValidateColumnIndex(columnIndex);
            var column = this.table.columns[columnIndex];

            if (column.columnType == ColumnType.String)
            {
                SetCellData(columnIndex, rowIndex, Encoding.UTF8.GetString(data));
                return;
            }

            ValidateRowIndex(rowIndex);

            var cellSize = GetCellSize(columnIndex);

            if (data.Length != cellSize)
                throw new ArgumentException($"Expected {cellSize} bytes but got {data.Length}.", nameof(data));

            var offset = rowIndex * cellSize;
            var mutableMemory = MemoryMarshal.AsMemory(this.columnBuffers[columnIndex].WrittenMemory);
            data.CopyTo(mutableMemory.Span.Slice(offset, cellSize));
        }

        public void SetCellData(string header, int rowIndex, string value) =>
            SetCellData(FindColumnIndex(header), rowIndex, value);

        public void SetCellData(string header, int rowIndex, ReadOnlySpan<byte> data) =>
            SetCellData(FindColumnIndex(header), rowIndex, data);

        public void Append()
        {
            for (var i = 0; i < this.table.columns.Count; i++)
            {
                var column = this.table.columns[i];
                var writer = this.columnBuffers[i];

                if (column.columnType == ColumnType.String)
                {
                    column.offsets.Add((uint)writer.WrittenCount);
                    column.byteSize = writer.WrittenCount;
                    continue;
                }

                var cellSize = GetCellSize(i);
                writer.GetSpan(cellSize)[..cellSize].Clear();
                writer.Advance(cellSize);
                column.byteSize = writer.WrittenCount;
            }

            this.table.rowCount++;
        }

        public void RenameColumn(int columnIndex, string newHeader)
        {
            ValidateColumnIndex(columnIndex);
            this.table.columns[columnIndex].header = newHeader;
        }

        public byte[] WriteToBytes()
        {
            int totalSize = CalculateTotalSize();
            var writer = new ArrayBufferWriter<byte>(totalSize);

            byte[] nameBytes = Encoding.UTF8.GetBytes(this.table.tableName ?? string.Empty);
            WriteInt32(ref writer, nameBytes.Length);
            WriteBytes(ref writer, nameBytes);
            WriteInt32(ref writer, this.table.rowCount);
            WriteInt32(ref writer, this.table.columns.Count);

            foreach (var col in this.table.columns)
            {
                byte[] hBytes = Encoding.UTF8.GetBytes(col.header ?? string.Empty);
                WriteInt32(ref writer, hBytes.Length);
                WriteBytes(ref writer, hBytes);
                WriteInt32(ref writer, col.byteSize);
                WriteInt32(ref writer, (int)col.columnType);

                int offCount = col.columnType == ColumnType.String ? col.offsets.Count : 0;
                WriteInt32(ref writer, offCount);
                for (int i = 0; i < offCount; i++)
                    WriteUInt32(ref writer, col.offsets[i]);
            }

            for (int i = 0; i < this.table.columns.Count; i++)
            {
                ReadOnlySpan<byte> data = this.columnBuffers[i].WrittenSpan;
                WriteInt32(ref writer, data.Length);
                WriteBytes(ref writer, data);
            }

            return writer.WrittenSpan.ToArray();
        }

        private void ValidateColumnIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.table.columns.Count)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }

        private void ValidateRowIndex(Column column, int rowIndex)
        {
            if (column.columnType == ColumnType.String)
            {
                if (rowIndex < 0 || rowIndex >= column.offsets.Count)
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));

                return;
            }

            if (rowIndex < 0 || rowIndex >= this.table.rowCount)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        private void ValidateRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= this.table.rowCount)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        private int GetCellSize(int columnIndex)
        {
            ValidateColumnIndex(columnIndex);

            var cellSize = this.columnCellSizes[columnIndex];
            if (cellSize <= 0)
                throw new InvalidOperationException($"Column '{this.table.columns[columnIndex].header}' does not have a valid fixed cell size.");

            return cellSize;
        }

        private int FindColumnIndex(string header)
        {
            for (int i = 0; i < this.table.columns.Count; i++)
            {
                if (this.table.columns[i].header == header)
                    return i;
            }
            throw new KeyNotFoundException($"Không tìm thấy column '{header}'");
        }

        private static void WriteInt32(ref ArrayBufferWriter<byte> w, int value)
        {
            var s = w.GetSpan(4);
            BitConverter.TryWriteBytes(s, value);
            w.Advance(4);
        }

        private static void WriteUInt32(ref ArrayBufferWriter<byte> w, uint value)
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

        private int CalculateTotalSize()
        {
            var size = sizeof(int) + Encoding.UTF8.GetByteCount(this.table.tableName ?? string.Empty);
            size += sizeof(int) + sizeof(int);

            foreach (var col in this.table.columns)
            {
                size += 4 + Encoding.UTF8.GetByteCount(col.header ?? string.Empty);
                size += 4 + 4 + 4;
                if (col.columnType == ColumnType.String)
                    size += col.offsets.Count * sizeof(int);
            }

            for (var i = 0; i < this.columnBuffers.Count; i++)
            {
                size += sizeof(int) + this.columnBuffers[i].WrittenMemory.Length;
            }

            return size;
        }

        public void AddColumn(string header, ColumnType columnType)
        {
            var column = new Column()
            {
                header = header,
                columnType = columnType
            };
            var writer = new ArrayBufferWriter<byte>();
            var cellSize = Extensions.GetSize(columnType);

            this.table.columns.Add(column);
            this.columnBuffers.Add(writer);
            this.columnCellSizes.Add(cellSize);

            for (var i = 0; i < this.table.rowCount; i++)
            {
                if (column.columnType == ColumnType.String)
                {
                    column.offsets.Add((uint)writer.WrittenCount);
                    column.byteSize = writer.WrittenCount;
                    continue;
                }

                writer.GetSpan(cellSize)[..cellSize].Clear();
                writer.Advance(cellSize);
                column.byteSize = writer.WrittenCount;
            }
        }

        public void RemoveRow(int rowIndex)
        {
            ValidateRowIndex(rowIndex);

            for (var i = 0; i < this.table.columns.Count; i++)
            {
                var column = this.table.columns[i];
                var oldBuffer = this.columnBuffers[i].WrittenSpan;

                if (column.columnType == ColumnType.String)
                {
                    var oldStart = (int)column.offsets[rowIndex];
                    var oldEnd = rowIndex + 1 < column.offsets.Count
                        ? (int)column.offsets[rowIndex + 1]
                        : oldBuffer.Length;
                    var removedLength = oldEnd - oldStart;
                    var newLength = oldBuffer.Length - removedLength;
                    var newWriter = new ArrayBufferWriter<byte>(newLength);

                    if (oldStart > 0)
                    {
                        oldBuffer[..oldStart].CopyTo(newWriter.GetSpan(oldStart));
                        newWriter.Advance(oldStart);
                    }

                    if (oldEnd < oldBuffer.Length)
                    {
                        var suffix = oldBuffer[oldEnd..];
                        suffix.CopyTo(newWriter.GetSpan(suffix.Length));
                        newWriter.Advance(suffix.Length);
                    }

                    column.offsets.RemoveAt(rowIndex);
                    for (var j = rowIndex; j < column.offsets.Count; j++)
                    {
                        column.offsets[j] = (uint)((int)column.offsets[j] - removedLength);
                    }

                    column.byteSize = newWriter.WrittenCount;
                    this.columnBuffers[i] = newWriter;
                }
                else
                {
                    var cellSize = GetCellSize(i);
                    var removeStart = rowIndex * cellSize;
                    var removeEnd = removeStart + cellSize;
                    var newLength = oldBuffer.Length - cellSize;
                    var newWriter = new ArrayBufferWriter<byte>(newLength);

                    if (removeStart > 0)
                    {
                        oldBuffer[..removeStart].CopyTo(newWriter.GetSpan(removeStart));
                        newWriter.Advance(removeStart);
                    }

                    if (removeEnd < oldBuffer.Length)
                    {
                        var suffix = oldBuffer[removeEnd..];
                        suffix.CopyTo(newWriter.GetSpan(suffix.Length));
                        newWriter.Advance(suffix.Length);
                    }

                    column.byteSize = newWriter.WrittenCount;
                    this.columnBuffers[i] = newWriter;
                }
            }

            this.table.rowCount--;
        }

        public void RemoveColumn(int columnIndex)
        {
            ValidateColumnIndex(columnIndex);

            this.table.columns.RemoveAt(columnIndex);
            this.columnBuffers.RemoveAt(columnIndex);
            this.columnCellSizes.RemoveAt(columnIndex);
        }
    }
}