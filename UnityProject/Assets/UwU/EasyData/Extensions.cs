namespace UwU.EasyData
{
    public static class Extensions
    {
        public static int GetSize(ColumnType columnType)
        {
            switch (columnType)
            {
                case ColumnType.SByte: return sizeof(sbyte);
                case ColumnType.Byte: return sizeof(byte);
                case ColumnType.Short: return sizeof(short);
                case ColumnType.UShort: return sizeof(ushort);
                case ColumnType.Int: return sizeof(int);
                case ColumnType.UInt: return sizeof(uint);
                case ColumnType.Long: return sizeof(long);
                case ColumnType.ULong: return sizeof(ulong);
                case ColumnType.Float: return sizeof(float);
                case ColumnType.Double: return sizeof(double);
                case ColumnType.Decimal: return sizeof(decimal);
                case ColumnType.Bool: return sizeof(bool);
                case ColumnType.Char: return sizeof(char);

                case ColumnType.String: return -1; // Kích thước động
                default: return -1;
            }
        }
    }
}