using System;
using System.Text;

namespace HuffmanCompression.Utils
{
    public static class IOUtils
    {
        public const byte BitsInByte = 8;

        public static string GetBits(params byte[] bytes)
        {
            var bits = new StringBuilder();

            for (int byteIndex = 0; byteIndex < bytes.Length; ++byteIndex)
                for (int bitPos = 0; bitPos < 8; ++bitPos)
                    bits.Append((bytes[byteIndex] >> bitPos) & 1U);

            return bits.ToString();
        }

        public static void PrintFirst48Bits(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; ++i)
            {
                PrintBits(bytes[i]);
                if (i == 5)
                    return;
            }
        }

        public static void PrintFirst48Chars(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                Console.Write(str[i]);
                if (i == 47)
                    return;
            }
        }

        public static bool AreIdentical(byte[] one, byte[] two)
        {
            bool haveIdenticalLength = one.Length == two.Length;
            if (haveIdenticalLength)
                for (int i = 0; i < one.Length; ++i)
                    if (one[i] != two[i])
                        return false;
            return haveIdenticalLength;
        }

        public static void PrintBits(byte @byte)
        {
            for (int i = 0; i < 8; ++i)
                Console.Write((byte)((@byte >> i) & 1));
        }

        public static byte[] WriteBits(string bits, bool moveRedundantZerosToFront = false)
        {
            int bytesToUse = bits.Length / BitsInByte + (bits.Length % BitsInByte == 0 ? 0 : 1);
            var byteArr = new byte[bytesToUse];

            byte numOfRedundantZeros = (byte)(bytesToUse * BitsInByte - bits.Length);

            int i = 0;
            for (int byteIndex = 0; byteIndex < bytesToUse; ++byteIndex)
                for (int bitPos = byteIndex == 0 && moveRedundantZerosToFront ? numOfRedundantZeros : 0; bitPos < BitsInByte; ++bitPos)
                    byteArr[byteIndex] |= (byte)((byte)char.GetNumericValue(bits[i++]) << bitPos);

            return byteArr;
        }
    }
}