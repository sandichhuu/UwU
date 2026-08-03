namespace UwU.Data
{
    public interface IByteData
    {
        void Serialize(BytePackage package);
        void Deserialize(BytePackage package);
    }
}