using HuffmanCompression.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace HuffmanCompression
{
    internal class Decompressor
    {
        const char EmptyChar = '\0';
        readonly static StringBuilder UnfinishedBits = new StringBuilder(Compressor.MaxCodeLength, Compressor.MaxCodeLength);

        public static string Decompress(byte[] compressedAsBytes)
        {
            string compressedAsBits = IOUtils.GetBits(compressedAsBytes);
            byte amountOfBitsThatStoreCodesLength = GetCodesLength(ref compressedAsBits);
            compressedAsBits = RemoveBeginningZeros(compressedAsBits);

            var decompressed = new StringBuilder();
            var dict = new Dictionary<string, char>();

            for (int i = 0; i < compressedAsBits.Length;)
            {
                bool codeAlreadyEncountered = char.GetNumericValue(compressedAsBits[i++]) == Compressor.CodeAlreadyEncounteredBit;

                (string code, char character) = codeAlreadyEncountered
                    ? GetCode(compressedAsBits, dict, ref i)
                    : ExtractInfoCodeNotEncountered(compressedAsBits, amountOfBitsThatStoreCodesLength, ref i);

                if (!codeAlreadyEncountered)
                    dict.Add(code, character);

                decompressed.Append(character);
            }

            return decompressed.ToString();
        }

        public static void DecompressFile(string path)
        {
            string pathToDecompressedFile = path.Remove(path.LastIndexOf("."));
            string decompressedText = Decompress(File.ReadAllBytes(path));
            File.WriteAllText(pathToDecompressedFile, decompressedText);
        }

        private static byte GetCodesLength(ref string text)
        {
            string info = text.Substring(0, Compressor.CodeLengthLengthInfoBitsAmount);
            text = text.Remove(0, Compressor.CodeLengthLengthInfoBitsAmount);
            foreach (var (amount, _, representationInFile) in Compressor.CodeLengthLengthInfo)
                if (representationInFile == info)
                    return amount;

            throw new NotImplementedException("Could not determine the amount of bits used for writing the codes' lengths.");
        }

        private static string RemoveBeginningZeros(string text)
        {
            int i;
            for (i = 0; text[i] == '0'; ++i) ;

            return text.Remove(0, i);
        }

        private static (string code, char character) ExtractInfoCodeNotEncountered(string compressed, byte amountOfBitsThatStoreCodesLength, ref int index)
        {
            string codeLengthAsBinary = compressed.Substring(index, amountOfBitsThatStoreCodesLength);
            int codeLength = Convert.ToInt32(codeLengthAsBinary, 2);
            index += amountOfBitsThatStoreCodesLength;

            string charEncoded = compressed.Substring(index, Compressor.BitsInOneChar);
            index += Compressor.BitsInOneChar;

            string code = compressed.Substring(index, codeLength);
            index += codeLength;

            char charDecoded = Compressor.TextEncoding.GetString(IOUtils.WriteBits(charEncoded))[0];
            return (code, charDecoded);
        }


        private static (string code, char character) GetCode(string bits, Dictionary<string, char> dict, ref int index)
        {
            UnfinishedBits.Clear();

            for (int i = index; i < index + Compressor.MaxCodeLength; ++i)
            {
                dict.TryGetValue(UnfinishedBits.Append(bits[i]).ToString(), out char character);

                if (character != default)
                {
                    index += UnfinishedBits.Length;
                    return (UnfinishedBits.ToString(), character);
                }
            }

            throw new Exception($"Method RecognizeCode in class {nameof(Decompressor)} could not recognize code: {bits.Substring(index, Compressor.MaxCodeLength)}");
        }
    }
}