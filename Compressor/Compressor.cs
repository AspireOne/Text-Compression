using HuffmanCompression.Extensions;
using HuffmanCompression.TreeElements;
using HuffmanCompression.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HuffmanCompression
{
    public class Compressor
    {
        public const byte CodeAlreadyEncounteredBit = 0;
        public const byte CodeNotAlreadyEncounteredBit = 1;
        public static readonly string CompressedFileExtension = ".comp";
        public static readonly byte BitsInOneChar = 8;
        public static readonly Encoding TextEncoding = Encoding.GetEncoding("ISO-8859-2");

        public static readonly (byte amountOfBits, byte maxValueInDecimal, string representationInFile)[] CodeLengthLengthInfo =
        {
            (3, GetBinaryNumberMaxValue(3), "00"),
            (4, GetBinaryNumberMaxValue(4), "01"),
            (5, GetBinaryNumberMaxValue(5), "10"),
            (6, GetBinaryNumberMaxValue(6), "11"),
        };

        public static readonly byte MaxCodeLength = CodeLengthLengthInfo.Last().maxValueInDecimal;
        public static readonly byte CodeLengthLengthInfoBitsAmount = (byte)((CodeLengthLengthInfo.Length / 2f) + 0.5f);

        private static byte GetBinaryNumberMaxValue(byte amountOfPlaces)
        {
            byte prevMultiplier = 1;
            byte theNum = 1;

            for (byte i = 0; i < amountOfPlaces - 1; ++i)
                theNum += prevMultiplier *= 2;

            return theNum;
        }

        public static byte[] Compress(string text)
        {
            (int occurence, char character)[] charactersOccurence = GetCharactersOccurence(text);

            // This is the bottleneck.
            Dictionary<char, string> charCodeDict = CreateCodesDictionary(charactersOccurence);

            if (AnyCodeIsTooLong(charCodeDict))
                throw new TooManyCharactersException("Input has too many different characters.");

            // This is bottleneck.
            string compressed = AddTreeInfoAndReplaceCharsWithCode(text, charCodeDict);
            byte[] compressedAsBytes = IOUtils.WriteBits(compressed, true, CodeLengthLengthInfoBitsAmount);
            return compressedAsBytes;
        }

        public static string CompressFile(string path)
        {
            byte[] compressed = Compress(File.ReadAllText(path));

            int lastIndexOfBackSlash = path.LastIndexOf(@"\");
            string containingFolder = path.Substring(0, lastIndexOfBackSlash + 1);

            string filename = path.Substring(lastIndexOfBackSlash + 1);
            string newFilename = filename + CompressedFileExtension;
            string pathToCompressedFile = containingFolder + newFilename;

            File.WriteAllBytes(pathToCompressedFile, compressed);

            return GetCompressionSummary(new FileInfo(path).Length, new FileInfo(pathToCompressedFile).Length);
        }

        public static string GetCompressionSummary(byte[] original, byte[] compressed) => GetCompressionSummary(original.Length, compressed);
        public static string GetCompressionSummary(long originalFileSize, byte[] compressed) => GetCompressionSummary(originalFileSize, compressed.Length);
        public static string GetCompressionSummary(byte[] original, long compressedFileSize) => GetCompressionSummary(original, compressedFileSize);
        public static string GetCompressionSummary(long originalFileSize, long compressedFileSize)
        {
            float sizeDiff = originalFileSize - compressedFileSize;
            float x = sizeDiff / originalFileSize;
            int percentage = (int)Math.Round(x * 100);

            return $"\noriginal size: {originalFileSize} bytes.\nCompressed size: {compressedFileSize} bytes.\nReduced by {percentage}%";
        }

        private static bool AnyCodeIsTooLong(Dictionary<char, string> codes)
        {
            foreach (var pair in codes)
                if (pair.Value.Length > CodeLengthLengthInfo.Last().maxValueInDecimal)
                    return true;
            return false;
        }

        // [1][length][as encoded bits][code] OR [0][code]
        private static string AddTreeInfoAndReplaceCharsWithCode(string text, Dictionary<char, string> dict)
        {
            var usedChars = new HashSet<char>();
            (byte codeLengthLength, string codeLengthLengthBits) = GetAmountOfBitsNeededToRepresentCodeLength(dict);

            StringBuilder encoded = new StringBuilder();

            foreach (char character in text)
            {
                string code = dict[character];

                if (!usedChars.Contains(character))
                {
                    usedChars.Add(character);

                    string charAsEncodedBits = IOUtils.GetBits(EncodeCharacter(character));
                    string codeLengthAsBinary = FillCodeLength(Convert.ToString(code.Length, 2), codeLengthLength);

                    encoded.Append(CodeNotAlreadyEncounteredBit).Append(codeLengthAsBinary).Append(charAsEncodedBits);
                }
                else
                    encoded.Append(CodeAlreadyEncounteredBit);

                encoded.Append(code);
            }

            return encoded.Insert(0, codeLengthLengthBits).ToString();
        }

        private static (byte amount, string representation) GetAmountOfBitsNeededToRepresentCodeLength(Dictionary<char, string> dict)
        {
            byte longestCodeLength = GetLongestCodeLength(dict);

            foreach (var (amount, maxCodeLengthDecimal, representationInFile) in CodeLengthLengthInfo)
                if (longestCodeLength <= maxCodeLengthDecimal)
                    return (amount, representationInFile);

            // This exception should never be thrown, because a check for codes lengths is made before calling this method.
            throw new NotImplementedException("at least one character's code exceeded the maximal code length limit.");
        }

        private static byte GetLongestCodeLength(Dictionary<char, string> dict)
        {
            byte highest = 0;

            foreach (var pair in dict)
                if (pair.Value.Length > highest)
                    highest = (byte)pair.Value.Length;

            return highest;
        }

        // Fills unused places with zeros.
        private static string FillCodeLength(string charCode, byte amountOfBitsToRepresentCodeLength)
        {
            int difference = amountOfBitsToRepresentCodeLength - charCode.Length;

            for (byte i = 0; i < difference; ++i)
                charCode = charCode.Insert(0, "0");

            return charCode;
        }

        private static byte EncodeCharacter(char character) => TextEncoding.GetBytes(new char[] { character })[0];

        private static Dictionary<char, string> CreateCodesDictionary((int occurence, char character)[] occurences)
        {
            var codes = new Dictionary<char, string>();
            BoxCharacters(occurences).ForEach(x => codes.Add(x.Character, FindCharacterCode(x)));
            return codes;
        }

        private static string FindCharacterCode(CharacterBox box)
        {
            string code = "";

            Box currBox = box;
            do
                code += currBox.Number;
            while ((currBox = currBox.Parent)?.Parent != null);

            return code.Reverse();
        }

        private static List<CharacterBox> BoxCharacters((int occurence, char character)[] occurences)
        {
            var CharacterList = new List<CharacterBox>();
            var tree = new List<Box>();

            Array.Sort(occurences);
            foreach (var occurence in occurences)
            {
                var box = new CharacterBox(occurence.character, occurence.occurence);
                CharacterList.Add(box);
                tree.Add(box);
            }

            while (tree.Count > 1)
            {
                Box right = tree[BoxesBox.RightIndex];
                Box left = tree[BoxesBox.LeftIndex];

                Box newBox = new BoxesBox(left, right);

                tree.RemoveAt(BoxesBox.RightIndex);
                tree.RemoveAt(BoxesBox.LeftIndex);

                InsertAtAppropriatePlace(tree, newBox);
            }

            return CharacterList;
        }

        private static void InsertAtAppropriatePlace(List<Box> tree, Box boxToInsert)
        {
            if (tree.Count == 0 || boxToInsert.Sum > tree.Last().Sum || (boxToInsert.Sum == tree.Last().Sum && tree.Last().Depth < boxToInsert.Depth))
            {
                tree.Add(boxToInsert);
                return;
            }

            for (int i = 0; i < tree.Count; ++i)
            {
                Box boxAtIndex = tree[i];

                if (boxAtIndex.Sum > boxToInsert.Sum || (boxAtIndex.Sum == boxToInsert.Sum && boxAtIndex.Depth >= boxToInsert.Depth))
                {
                    tree.Insert(i, boxToInsert);
                    return;
                }
            }

            throw new Exception($"Could not insert box with sum {boxToInsert.Sum} to a tree with count {tree.Count} and highest sum {tree.Last().Sum}.");
        }

        // This is the bottleneck
        private static (int occurence, char character)[] GetCharactersOccurence(string text)
        {
            StringBuilder sb = new StringBuilder(text);
            var stack = new Stack<(int occurence, char character)>();

            int lengthBeforeReplacement;
            while ((lengthBeforeReplacement = sb.Length) > 0)
            {
                char character = sb[0];
                sb.Replace(character.ToString(), "");
                stack.Push((lengthBeforeReplacement - sb.Length, character));
            }
            return stack.ToArray();
        }
    }
}