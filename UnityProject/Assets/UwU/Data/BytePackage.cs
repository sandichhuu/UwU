namespace UwU.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class BytePackage : IDisposable
    {
        private readonly MemoryStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private readonly string password;

        // Traditional array initialization for older C# versions
        private static readonly byte[] Salt = new byte[] { 0x42, 0x79, 0x74, 0x65, 0x50, 0x61, 0x63, 0x6B };
        private static readonly byte[] FixedIv = new byte[] { 0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

        // Constructor for WRITING
        public BytePackage(string password = null)
        {
            this.stream = new MemoryStream();
            this.writer = new BinaryWriter(this.stream, Encoding.UTF8);
            this.password = password;
        }

        // Constructor for READING from file path
        public BytePackage(string path, string password = null)
        {
            byte[] rawBytes = File.ReadAllBytes(path);
            if (!string.IsNullOrEmpty(password))
            {
                rawBytes = Decrypt(rawBytes, password);
            }
            this.stream = new MemoryStream(rawBytes);
            this.reader = new BinaryReader(this.stream, Encoding.UTF8);
        }

        public BytePackage(byte[] rawBytes, string password = null)
        {
            if (!string.IsNullOrEmpty(password))
            {
                rawBytes = Decrypt(rawBytes, password);
            }
            this.stream = new MemoryStream(rawBytes);
            this.reader = new BinaryReader(this.stream, Encoding.UTF8);
        }

        // ----------------------------------------------------
        // WRITE SINGLE VALUES (Unity/IL2CPP Safe)
        // ----------------------------------------------------
        public void Write<T>(T value) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            Type t = typeof(T);
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

        // ----------------------------------------------------
        // WRITE ARRAYS 
        // ----------------------------------------------------
        public void WriteArray<T>(T[] array) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");
            this.writer.Write(array != null ? array.Length : -1);
            if (array == null) return;

            Type t = typeof(T);
            if (t == typeof(int)) { int[] arr = (int[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(float)) { float[] arr = (float[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(double)) { double[] arr = (double[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(bool)) { bool[] arr = (bool[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(byte)) { this.writer.Write((byte[])(object)array); }
            else if (t == typeof(long)) { long[] arr = (long[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else if (t == typeof(short)) { short[] arr = (short[])(object)array; for (int i = 0; i < arr.Length; i++) this.writer.Write(arr[i]); }
            else throw new NotSupportedException("Array type " + t.Name + "[] is not supported.");
        }

        // ----------------------------------------------------
        // WRITE LISTS
        // ----------------------------------------------------
        public void WriteList<T>(List<T> list) where T : struct
        {
            if (this.writer == null) throw new InvalidOperationException("Package is in read-only mode.");

            if (list == null)
            {
                this.writer.Write(-1);
                return;
            }

            this.writer.Write(list.Count);

            Type t = typeof(T);
            if (t == typeof(int)) { for (int i = 0; i < list.Count; i++) this.writer.Write((int)(object)list[i]); }
            else if (t == typeof(float)) { for (int i = 0; i < list.Count; i++) this.writer.Write((float)(object)list[i]); }
            else if (t == typeof(double)) { for (int i = 0; i < list.Count; i++) this.writer.Write((double)(object)list[i]); }
            else if (t == typeof(bool)) { for (int i = 0; i < list.Count; i++) this.writer.Write((bool)(object)list[i]); }
            else if (t == typeof(byte))
            {
                byte[] bytes = new byte[list.Count];
                for (int i = 0; i < list.Count; i++) bytes[i] = (byte)(object)list[i];
                this.writer.Write(bytes);
            }
            else if (t == typeof(long)) { for (int i = 0; i < list.Count; i++) this.writer.Write((long)(object)list[i]); }
            else if (t == typeof(short)) { for (int i = 0; i < list.Count; i++) this.writer.Write((short)(object)list[i]); }
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
            for (int i = 0; i < list.Count; i++)
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
            for (int i = 0; i < array.Length; i++)
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
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Serialize(this);
            }
        }

        // Gets final byte stream
        public byte[] Bytes
        {
            get
            {
                if (this.writer != null) this.writer.Flush();
                byte[] data = this.stream.ToArray();
                if (!string.IsNullOrEmpty(this.password))
                {
                    data = Encrypt(data, this.password);
                }
                return data;
            }
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, this.Bytes);
        }

        // ----------------------------------------------------
        // READ SINGLE VALUES
        // ----------------------------------------------------
        public T Read<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            Type t = typeof(T);
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

        // ----------------------------------------------------
        // READ ARRAYS
        // ----------------------------------------------------
        public T[] ReadArray<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            int length = this.reader.ReadInt32();
            if (length == -1) return null;

            Type t = typeof(T);
            if (t == typeof(int))
            {
                int[] arr = new int[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadInt32();
                return (T[])(object)arr;
            }
            if (t == typeof(float))
            {
                float[] arr = new float[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadSingle();
                return (T[])(object)arr;
            }
            if (t == typeof(double))
            {
                double[] arr = new double[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadDouble();
                return (T[])(object)arr;
            }
            if (t == typeof(bool))
            {
                bool[] arr = new bool[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadBoolean();
                return (T[])(object)arr;
            }
            if (t == typeof(byte))
            {
                return (T[])(object)this.reader.ReadBytes(length);
            }
            if (t == typeof(long))
            {
                long[] arr = new long[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadInt64();
                return (T[])(object)arr;
            }
            if (t == typeof(short))
            {
                short[] arr = new short[length];
                for (int i = 0; i < length; i++) arr[i] = this.reader.ReadInt16();
                return (T[])(object)arr;
            }
            throw new NotSupportedException("Array type " + t.Name + "[] is not supported.");
        }

        // ----------------------------------------------------
        // READ LISTS
        // ----------------------------------------------------
        public List<T> ReadList<T>() where T : struct
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            int length = this.reader.ReadInt32();
            if (length == -1) return null;

            List<T> list = new List<T>(length);
            Type t = typeof(T);

            if (t == typeof(int))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt32());
            }
            else if (t == typeof(float))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadSingle());
            }
            else if (t == typeof(double))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadDouble());
            }
            else if (t == typeof(bool))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadBoolean());
            }
            else if (t == typeof(byte))
            {
                byte[] bytes = this.reader.ReadBytes(length);
                for (int i = 0; i < bytes.Length; i++) list.Add((T)(object)bytes[i]);
            }
            else if (t == typeof(long))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt64());
            }
            else if (t == typeof(short))
            {
                for (int i = 0; i < length; i++) list.Add((T)(object)this.reader.ReadInt16());
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

            int length = this.reader.ReadInt32();
            if (length == -1) return null;

            List<string> list = new List<string>(length);
            for (int i = 0; i < length; i++)
            {
                list.Add(this.reader.ReadString());
            }
            return list;
        }

        public T[] ReadDataArray<T>() where T : IByteData, new()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            int length = this.reader.ReadInt32();
            if (length == -1) return null;

            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
                array[i].Deserialize(this);
            }
            return array;
        }

        public List<T> ReadDataList<T>() where T : IByteData, new()
        {
            if (this.reader == null) throw new InvalidOperationException("Package is in write-only mode.");

            int length = this.reader.ReadInt32();
            if (length == -1) return null;

            List<T> list = new List<T>(length);
            for (int i = 0; i < length; i++)
            {
                T item = new T();
                item.Deserialize(this);
                list.Add(item);
            }
            return list;
        }

        // ----------------------------------------------------
        // CRYPTOGRAPHY HELPERS (Compatible with standard Unity Mono)
        // ----------------------------------------------------
        private static byte[] Encrypt(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Salt, 1000))
                {
                    aes.Key = deriveBytes.GetBytes(32); // AES-256
                    aes.IV = FixedIv;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private static byte[] Decrypt(byte[] encryptedData, string password)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Salt, 1000))
                    {
                        aes.Key = deriveBytes.GetBytes(32);
                        aes.IV = FixedIv;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(encryptedData, 0, encryptedData.Length);
                                cs.FlushFinalBlock();
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                throw new UnauthorizedAccessException("Invalid password or corrupted package file.");
            }
        }

        public void Dispose()
        {
            if (this.writer != null) { this.writer.Close(); this.writer = null; }
            if (this.reader != null) { this.reader.Close(); this.reader = null; }
            if (this.stream != null) { this.stream.Close(); }
        }
    }
}