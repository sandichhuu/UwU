using UwU.Data;

namespace UwU.Grid
{
    public class GridData
    {
        private const string KEY = "LmT4fVVt2z";

        public int width;
        public int height;
        public float space;
        public float cellSize;
        public int[] obstacles;

        public void Read(byte[] rawBytes)
        {
            using (var bytePackage = new BytePackage(rawBytes, KEY))
            {
                this.width = bytePackage.Read<int>();
                this.height = bytePackage.Read<int>();
                this.space = bytePackage.Read<float>();
                this.cellSize = bytePackage.Read<float>();
                this.obstacles = bytePackage.ReadArray<int>();
                OnDeserialize(bytePackage);
            }
        }

        public void Open(string path)
        {
            using (var bytePackage = new BytePackage(path, KEY))
            {
                this.width = bytePackage.Read<int>();
                this.height = bytePackage.Read<int>();
                this.space = bytePackage.Read<float>();
                this.cellSize = bytePackage.Read<float>();
                this.obstacles = bytePackage.ReadArray<int>();
                OnDeserialize(bytePackage);
            }
        }

        public void Save(string path)
        {
            using (var bytePackage = new BytePackage(KEY))
            {
                bytePackage.Write(this.width);
                bytePackage.Write(this.height);
                bytePackage.Write(this.space);
                bytePackage.Write(this.cellSize);
                bytePackage.WriteArray(this.obstacles);
                OnSerialize(bytePackage);
                bytePackage.Save(path);
            }
        }

        public virtual void OnSerialize(BytePackage bytePackage) { }
        public virtual void OnDeserialize(BytePackage bytePackage) { }

        public static T FromFile<T>(string path) where T : GridData, new()
        {
            var gridData = new T();
            gridData.Open(path);
            return gridData;
        }

        public static T FromBytes<T>(byte[] rawBytes) where T : GridData, new()
        {
            var gridData = new T();
            gridData.Read(rawBytes);
            return gridData;
        }
    }
}