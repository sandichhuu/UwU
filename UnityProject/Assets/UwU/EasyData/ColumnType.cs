namespace UwU.EasyData
{
    public enum ColumnType
    {
        // Integer types
        SByte,
        Byte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,

        // Floating-point types
        Float,
        Double,
        Decimal,

        // Boolean & Character
        Bool,
        Char,

        // String (Reference type / variable size)
        String,

        // Array types
        IntArray,
        LongArray,
        FloatArray,
        DoubleArray,
        BoolArray,
        StringArray
    }
}