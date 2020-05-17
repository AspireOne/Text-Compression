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