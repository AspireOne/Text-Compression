using HuffmanCompression.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HuffmanCompression
{
    internal class Decompressor
    {
        private const char EmptyChar = '\0';

        public static string Decompress(byte[] compressedAsBytes)
        {
            var compressedAsBits = new StringBuilder(IOUtils.GetBits(compressedAsBytes));
            RemoveBeginningZeros(compressedAsBits);

            Console.WriteLine("first 100 of compressed message (in decompressor): ");
            for (int i = 0; i < 100 / 8; ++i)
                Program.PrintBits(compressedAsBytes[i]);
            Console.WriteLine("----");

            var decompressed = new StringBuilder();
            var dict = new Dictionary<string, char>();

            while (compressedAsBits.Length > 1)
            {
                int i = 0;
                Console.WriteLine("Current dictionary: ");
                foreach (var pair in dict)
                    Console.WriteLine(++i + " { '" + (pair.Value == '\n' ? "[new line]" : "" + pair.Value) + "', \"" + pair.Key + "\" },");

                bool codeAlreadyEncountered = char.GetNumericValue(compressedAsBits[0]) == Compressor.CodeAlreadyEncounteredBit;
                compressedAsBits.Remove(0, 1);

                (string code, char character) = codeAlreadyEncountered
                    ? TryRecognizeCode(compressedAsBits, dict)
                    : ExtractInfoCodeNotEncountered(compressedAsBits);

                decompressed.Append(character);

                if (!codeAlreadyEncountered)
                    dict.Add(code, character);
            }

            return decompressed.ToString();
        }

        private static void RemoveBeginningZeros(StringBuilder text)
        {
            for (int i = 0; i < IOUtils.BitsInByte; ++i)
                if (char.GetNumericValue(text[i]) != 0)
                {
                    Console.WriteLine("amount of zeros removed during compression: " + i);
                    text.Remove(0, i);
                    return;
                }
        }

        private static (string code, char character) ExtractInfoCodeNotEncountered(StringBuilder compressed)
        {
            string codeLengthAsBinary = compressed.ToString(0, Compressor.AmountOfBitsToRepresentCodeLength);
            int codeLength = Convert.ToInt32(codeLengthAsBinary, 2);
            compressed.Remove(0, Compressor.AmountOfBitsToRepresentCodeLength);

            string charEncoded = compressed.ToString(0, Compressor.BitsInOneChar);
            compressed.Remove(0, Compressor.BitsInOneChar);

            string code = compressed.ToString(0, codeLength);
            compressed.Remove(0, codeLength);

            char charDecoded = Compressor.TextEncoding.GetString(IOUtils.WriteBits(charEncoded))[0];
            return (code, charDecoded);
        }

        private static (string code, char character) TryRecognizeCode(StringBuilder bits, Dictionary<string, char> dict)
        {
            string unfinishedBits = "";

            for (int i = 0; i < Compressor.MaxCodeLength; ++i)
            {
                dict.TryGetValue(unfinishedBits += bits[i], out char character);

                if (character != default)
                {
                    bits.Remove(0, unfinishedBits.Length);
                    return (unfinishedBits, character);
                }
            }

            throw new Exception($"Method RecognizeCode in class {nameof(Decompressor)} could not recognize code: {bits.ToString(0, Compressor.MaxCodeLength)}");
        }
    }
}