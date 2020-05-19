using HuffmanCompression.Extensions;
using HuffmanCompression.TreeElements;
using HuffmanCompression.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HuffmanCompression
{
    public class Compressor
    {
        public static readonly string CompressedFileExtension = ".comp";
        public const byte CodeAlreadyEncounteredBit = 0;
        public const byte CodeNotAlreadyEncounteredBit = 1;
        public static readonly byte BitsInOneChar = 8;
        public static readonly byte AmountOfBitsToRepresentCodeLength = 4;
        public static readonly byte MaxCodeLength = ComputeMaxCodeLength();
        public static readonly Encoding TextEncoding = Encoding.GetEncoding("ISO-8859-2");

        private static byte ComputeMaxCodeLength()
        {
            byte prevMultiplier = 1;
            byte theNum = 1;

            for (byte i = 0; i < AmountOfBitsToRepresentCodeLength - 1; ++i)
                theNum += prevMultiplier *= 2;

            return theNum;
        }

        public static byte[] Compress(string text)
        {
            (int occurence, char character)[] charactersOccurence = GetCharactersOccurence(text);
            Dictionary<char, string> charCodeDict = CreateCodesDictionary(charactersOccurence);

            if (CodesAreTooLong(charCodeDict))
                throw new TooManyCharactersException("Input has too many different characters.");

            string compressed = AddTreeInfoAndReplaceCharsWithCode(text, charCodeDict);
            byte[] compressedAsBytes = IOUtils.WriteBits(compressed, true);
            //WriteCompressionInfo(compressed, compressedAsBytes, charCodeDict);

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
            float b = sizeDiff / originalFileSize;
            int percentage = (int)Math.Round(b * 100);

            return $"\noriginal size: {originalFileSize} bytes.\nCompressed size: {compressedFileSize} bytes.\nReduced by {percentage}%";
        }

        private static bool CodesAreTooLong(Dictionary<char, string> codes)
        {
            foreach (var pair in codes)
                if (pair.Value.Length > MaxCodeLength)
                    return true;
            return false;
        }

        // [1][length][as encoded bits][code] OR [0][code]
        private static string AddTreeInfoAndReplaceCharsWithCode(string text, Dictionary<char, string> dict)
        {
            var textCopy = new StringBuilder(text);
            var usedChars = new List<char>();

            int i = 0;
            while (true)
            {
                char character = textCopy[i];
                bool charAlreadyUsed = usedChars.Contains(character);

                dict.TryGetValue(character, out string code);

                string toInsert = (charAlreadyUsed ? CodeAlreadyEncounteredBit : CodeNotAlreadyEncounteredBit) + "";

                if (!charAlreadyUsed)
                {
                    usedChars.Add(character);

                    string charAsEncodedBits = IOUtils.GetBits(EncodeCharacter(character));
                    string codeLengthAsBinary = CorrectCodeLength(Convert.ToString(code.Length, 2));

                    toInsert = string.Concat(toInsert, codeLengthAsBinary, charAsEncodedBits);

                    if (codeLengthAsBinary.Length != AmountOfBitsToRepresentCodeLength)
                        throw new Exception($"The number representing code length had a wrong length. Should be {AmountOfBitsToRepresentCodeLength}, but was {codeLengthAsBinary.Length}");
                }

                textCopy.Remove(i, 1);
                textCopy.Insert(i, toInsert + code);

                if ((i += toInsert.Length + code.Length) > textCopy.Length - 1)
                    break;
            }

            return textCopy.ToString();
        }

        private static string CorrectCodeLength(string codeLength)
        {
            int difference = AmountOfBitsToRepresentCodeLength - codeLength.Length;

            if (difference != 0)
                for (int i = 0; i < difference; ++i)
                    codeLength = codeLength.Insert(0, "0");

            return codeLength;
        }

        private static byte EncodeCharacter(char character)
        {
            var bytes = TextEncoding.GetBytes(new char[] { character });

            if (bytes.Length > 1)
                throw new Exception($"It took more than 1 byte ({bytes.Length}) to encode character {character}");

            return bytes[0];
        }

        // Character | the character's representation code.
        private static Dictionary<char, string> CreateCodesDictionary((int occurence, char character)[] occurences)
        {
            var codes = new Dictionary<char, string>();
            BoxCharacters(occurences).ForEach(x => codes.Add(x.Character, FindCharacterCode(x)));

            return codes;
        }

        private static string FindCharacterCode(CharacterBox box)
        {
            string code = "";

            Box temp = box;
            do
                code += temp.Number;
            while ((temp = temp.Parent)?.Parent != null);

            return code.Reverse();
        }

        // Correct.
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

                tree.Remove(right);
                tree.Remove(left);

                InsertAtAppropriatePlace(tree, newBox);
            }

            return CharacterList;
        }

        // Correct.
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

        // Correct.
        private static (int occurence, char character)[] GetCharactersOccurence(string text)
        {
            var set = new HashSet<(int occurence, char character)>();
            foreach (char ch in text)
                set.Add((text.Length - text.Replace(ch.ToString(), "").Length, ch));

            return set.ToArray();
        }
    }
}