namespace UwU.EasyData
{
    public static class Extensions
    {
        public static int GetSize(ColumnType columnType)
        {
            switch (columnType)
            {
                case ColumnType.Bool: return sizeof(bool);
                case ColumnType.Int: return sizeof(int);
                case ColumnType.Float: return sizeof(float);
                case ColumnType.String: return -1;
                default: return -1;
            }
        }
    }
}