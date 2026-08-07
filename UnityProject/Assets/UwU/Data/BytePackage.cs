namespace UwU.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class BytePackage : IDisposable
    {
        private readonly MemoryStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private readonly string password;

        public BytePackage(string password = null)
        {
            this.stream = new MemoryStream();
            this.writer = new BinaryWriter(this.stream, Encoding.UTF8);
            this.password = password;
        }

        public BytePackage(string path, string password = null)
        {
            var rawBytes = File.ReadAllBytes(path);
            if (!string.IsNullOrEmpty(password))
            {
                rawBytes = DataHelper.Decrypt(rawBytes, password);
            }
            this.stream = new MemoryStream(rawBytes);
            this.reader = new BinaryReader(this.stream, Encoding.UTF8);
        }

        public BytePackage(byte[] rawBytes, string password = null)
        {
            if (!string.IsNullOrEmpty(password))
            {
                rawBytes = DataHelper.Decrypt(rawBytes, password);
            }
            this.stream = new MemoryStream(rawBytes);
            this.reader = new BinaryReader(this.stream, Encoding.UTF8);
        }

        public void Write<T>(T value) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            var t = typeof(T);
            if (t == typeof(int)) this.writer.Write((int)(object)value);
            else if (t == typeof(float)) this.writer.Write((float)(object)value);
            else if (t == typeof(double)) this.writer.Write((double)(object)value);
            else if (t == typeof(bool)) this.writer.Write((bool)(object)value);
            else if (t == typeof(byte)) this.writer.Write((byte)(object)value);
            else if (t == typeof(long)) this.writer.Write((long)(object)value);
            else if (t == typeof(short)) this.writer.Write((short)(object)value);
            else throw new NotSupportedException("Type " + t.Name + " is not supported.");
        }

        public void WriteString(string value)
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");
            this.writer.Write(value ?? string.Empty);
        }

        public void WriteByteData<T>(T value) where T : IByteData
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");
            value.Serialize(this);
        }

        public void WriteArray<T>(T[] array) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");
            this.writer.Write(array != null ? array.Length : -1);
            if (array == null) return;

            var t = typeof(T);
            if (t == typeof(int)) { var arr = (int[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(float)) { var arr = (float[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(double)) { var arr = (double[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(bool)) { var arr = (bool[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(byte)) { this.writer.Write((byte[])(object)array); }
            else if (t == typeof(long)) { var arr = (long[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(short)) { var arr = (short[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else throw new NotSupportedException("Array type " + t.Name + "[] is not supported.");
        }

        public void WriteList<T>(List<T> list) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            if (list == null)
            {
                this.writer.Write(-1);
                return;
            }

            this.writer.Write(list.Count);

            var t = typeof(T);
            if (t == typeof(int)) { for (var i = 0; i < list.Count; i++) this.writer.Write((int)(object)list[i]); }
            else if (t == typeof(float)) { for (var i = 0; i < list.Count; i++) this.writer.Write((float)(object)list[i]); }
            else if (t == typeof(double)) { for (var i = 0; i < list.Count; i++) this.writer.Write((double)(object)list[i]); }
            else if (t == typeof(bool)) { for (var i = 0; i < list.Count; i++) this.writer.Write((bool)(object)list[i]); }
            else if (t == typeof(byte))
            {
                var bytes = new byte[list.Count];
                for (var i = 0; i < list.Count; i++) bytes[i] = (byte)(object)list[i];
                this.writer.Write(bytes);
            }
            else if (t == typeof(long)) { for (var i = 0; i < list.Count; i++) this.writer.Write((long)(object)list[i]); }
            else if (t == typeof(short)) { for (var i = 0; i < list.Count; i++) this.writer.Write((short)(object)list[i]); }
            else throw new NotSupportedException("List type " + t.Name + " is not supported.");
        }

        public void WriteStringList(List<string> list)
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            if (list == null)
            {
                this.writer.Write(-1);
                return;
            }

            this.writer.Write(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                this.writer.Write(list[i] ?? string.Empty);
            }
        }

        public void WriteDataArray<T>(T[] array) where T : IByteData
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            if (array == null)
            {
                this.writer.Write(-1);
                return;
            }

            this.writer.Write(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                array[i].Serialize(this);
            }
        }

        public void WriteDataList<T>(List<T> list) where T : IByteData
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            if (list == null)
            {
                this.writer.Write(-1);
                return;
            }

            this.writer.Write(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                list[i].Serialize(this);
            }
        }

        public byte[] Bytes
        {
            get
            {
                this.writer?.Flush();
                var data = this.stream.ToArray();
                if (!string.IsNullOrEmpty(this.password))
                {
                    data = DataHelper.Encrypt(data, this.password);
                }
                return data;
            }
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, this.Bytes);
        }

        public T Read<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var t = typeof(T);
            if (t == typeof(int)) return (T)(object)this.reader.ReadInt32();
            if (t == typeof(float)) return (T)(object)this.reader.ReadSingle();
            if (t == typeof(double)) return (T)(object)this.reader.ReadDouble();
            if (t == typeof(bool)) return (T)(object)this.reader.ReadBoolean();
            if (t == typeof(byte)) return (T)(object)this.reader.ReadByte();
            if (t == typeof(long)) return (T)(object)this.reader.ReadInt64();
            if (t == typeof(short)) return (T)(object)this.reader.ReadInt16();
            throw new NotSupportedException("Type " + t.Name + " is not supported.");
        }

        public string ReadString()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");
            return this.reader.ReadString();
        }

        public T ReadData<T>() where T : IByteData, new()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");
            var value = new T();
            value.Deserialize(this);
            return value;
        }

        public T[] ReadArray<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var length = this.reader.ReadInt32();
            if (length == -1) return null;

            var t = typeof(T);
            if (t == typeof(int))
            {
                var arr = new int[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadInt32();
                return (T[])(object)arr;
            }
            if (t == typeof(float))
            {
                var arr = new float[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadSingle();
                return (T[])(object)arr;
            }
            if (t == typeof(double))
            {
                var arr = new double[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadDouble();
                return (T[])(object)arr;
            }
            if (t == typeof(bool))
            {
                var arr = new bool[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadBoolean();
                return (T[])(object)arr;
            }
            if (t == typeof(byte))
            {
                return (T[])(object)this.reader.ReadBytes(length);
            }
            if (t == typeof(long))
            {
                var arr = new long[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadInt64();
                return (T[])(object)arr;
            }
            if (t == typeof(short))
            {
                var arr = new short[length];
                for (var i = 0; i < length; i++) arr[i] = this.reader.ReadInt16();
                return (T[])(object)arr;
            }
            throw new NotSupportedException("Array type " + t.Name + "[] is not supported.");
        }

        public List<T> ReadList<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var length = this.reader.ReadInt32();
            if (length == -1) return null;

            var list = new List<T>(length);
            var t = typeof(T);

            if (t == typeof(int))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt32());
            }
            else if (t == typeof(float))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadSingle());
            }
            else if (t == typeof(double))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadDouble());
            }
            else if (t == typeof(bool))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadBoolean());
            }
            else if (t == typeof(byte))
            {
                var bytes = this.reader.ReadBytes(length);
                for (var i = 0; i < bytes.Length; i++) list.Add((T)(object)bytes[i]);
            }
            else if (t == typeof(long))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt64());
            }
            else if (t == typeof(short))
            {
                for (var i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt16());
            }
            else
            {
                throw new NotSupportedException("List type " + t.Name + " is not supported.");
            }

            return list;
        }

        public List<string> ReadStringList()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var length = this.reader.ReadInt32();
            if (length == -1) return null;

            var list = new List<string>(length);
            for (var i = 0; i < length; i++)
            {
                list.Add(this.reader.ReadString());
            }
            return list;
        }

        public T[] ReadDataArray<T>() where T : IByteData, new()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var length = this.reader.ReadInt32();
            if (length == -1) return null;

            var array = new T[length];
            for (var i = 0; i < length; i++)
            {
                array[i] = new T();
                array[i].Deserialize(this);
            }
            return array;
        }

        public List<T> ReadDataList<T>() where T : IByteData, new()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            var length = this.reader.ReadInt32();
            if (length == -1) return null;

            var list = new List<T>(length);
            for (var i = 0; i < length; i++)
            {
                var item = new T();
                item.Deserialize(this);
                list.Add(item);
            }
            return list;
        }

        public void Dispose()
        {
            this.writer?.Close();
            this.writer = null;
            this.reader?.Close();
            this.reader = null; this.stream?.Close();
        }
    }
}