using HuffmanCompression.Extensions;
using HuffmanCompression.TreeElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace HuffmanCompression
{
    public class Compressor
    {
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
                theNum += (prevMultiplier *= 2);

            return theNum;
        }

        public static byte[] Compress(string text)
        {
            (int occurence, char character)[] charactersOccurence = GetCharactersOccurence(text);

            Dictionary<char, string> charCodeDict = CreateCodesDictionary(charactersOccurence);

            if (CodesAreTooLong(charCodeDict))
                throw new TooManyCharactersException("Input has too many different characters.");

            string textToCompress = AddTreeInfo(text, charCodeDict);

            string textCompressed = ReplaceTextWithCodes(textToCompress, charCodeDict);
            Console.WriteLine("Amount of zeros to add: " + textCompressed.Length % 8);
            Console.WriteLine("first 100 of compressed message: ");
            for (int i = 0; i < 100; ++i)
                Console.Write(textCompressed[i]);
            Console.WriteLine("----");

            return Utils.IOUtils.WriteBits(textCompressed, true);
        }

        private static bool CodesAreTooLong(Dictionary<char, string> codes)
        {
            foreach (var pair in codes)
                if (pair.Value.Length > MaxCodeLength)
                    return true;
            return false;
        }

        // [1][length][as encoded bits][code] OR [0][code]
        private static string AddTreeInfo(string text, Dictionary<char, string> dict)
        {
            var textCopy = new StringBuilder(text);
            var usedChars = new List<char>();

            int i = 0;
            while (true)
            {
                char character = textCopy[i];
                bool charAlreadyUsed = usedChars.Contains(character);

                string toInsert = (charAlreadyUsed ? CodeAlreadyEncounteredBit : CodeNotAlreadyEncounteredBit) + "";

                if (!charAlreadyUsed)
                {
                    usedChars.Add(character);

                    string charAsEncodedBits = Utils.IOUtils.GetBits(EncodeCharacter(character));

                    dict.TryGetValue(character, out string code);

                    if (code is null || code.Equals(string.Empty))
                        throw new Exception($"Could not find the code associated with the character {character}.");

                    string codeLengthAsBinary = CorrectCodeLength(Convert.ToString(code.Length, 2));

                    toInsert += codeLengthAsBinary + charAsEncodedBits;

                    if (codeLengthAsBinary.Length != 4)
                        throw new Exception($"The number representing code length had a wrong length. Should be {AmountOfBitsToRepresentCodeLength}, but was {codeLengthAsBinary.Length}");
                }

                textCopy.Insert(i, toInsert);
                if ((i += toInsert.Length + 1) > textCopy.Length - 1)
                    break;
            }

            return textCopy.ToString();
        }

        private static string CorrectCodeLength(string codeLength)
        {
            int difference = 4 - codeLength.Length;
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

        private static string ReplaceTextWithCodes(string text, Dictionary<char, string> codes)
        {
            var textEncoded = new StringBuilder(text);

            foreach (var pair in codes)
                textEncoded.Replace(pair.Key.ToString(), pair.Value);

            return textEncoded.ToString();
        }

        // Character | the character's representation code.
        private static Dictionary<char, string> CreateCodesDictionary((int occurence, char character)[] occurences)
        {
            var codes = new Dictionary<char, string>();
            BoxCharacters(occurences).ForEach(x => codes.Add(x.Character, FindCharacterCode(x)));

            // For debugging purposes.
            int i = 0;
            foreach (var pair in codes)
                Console.WriteLine(++i + " { '" + (pair.Key == '\n' ? "[new line]" : "" + pair.Key) + "', \"" + pair.Value + "\" },");

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