using System;

namespace UwU.Data
{
    public class EnigmaProcessor
    {
        private readonly byte[] wheel;
        private int wheelIndex = 0;

        public EnigmaProcessor(Span<byte> wheel)
        {
            this.wheel = wheel.ToArray(); // Clone new one
        }

        public byte Process(byte input)
        {
            var currentOffset = this.wheel[this.wheelIndex];
            var output = (byte)(input ^ currentOffset);
            this.wheel[this.wheelIndex] = (byte)((currentOffset + input + 13) % 256);
            this.wheelIndex = (this.wheelIndex + 1) % this.wheel.Length;
            return output;
        }

        public byte[] Process(Span<byte> input)
        {
            var length = input.Length;
            var result = new byte[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = Process(input[i]);
            }
            return result;
        }
    }
}