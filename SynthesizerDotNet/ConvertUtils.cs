using System;
using System.Linq;

namespace SynthesizerDotNet
{
    internal static class ConvertUtils
    {
        public static byte[] ToLittleEndianBytes(this uint value)
            => value.ToBytes(true);

        public static byte[] ToBytes(this uint value, bool isLittleEndian)
        {
            bool needReverse = isLittleEndian != BitConverter.IsLittleEndian;
            byte[] bytes = BitConverter.GetBytes(value);
            return needReverse ? bytes.Reverse().ToArray() : bytes;
        }

        public static byte[] ToLittleEndianBytes(this ushort value)
            => value.ToBytes(true);

        public static byte[] ToBytes(this ushort value, bool isLittleEndian)
        {
            bool needReverse = isLittleEndian != BitConverter.IsLittleEndian;
            byte[] bytes = BitConverter.GetBytes(value);
            return needReverse ? bytes.Reverse().ToArray() : bytes;
        }

        public static byte[] ToLittleEndianBytes(this short value)
            => value.ToBytes(true);

        public static byte[] ToBytes(this short value, bool isLittleEndian)
        {
            bool needReverse = isLittleEndian != BitConverter.IsLittleEndian;
            byte[] bytes = BitConverter.GetBytes(value);
            return needReverse ? bytes.Reverse().ToArray() : bytes;
        }
    }
}