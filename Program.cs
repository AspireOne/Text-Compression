using System;
using System.IO;

namespace HuffmanCompression
{
    public static class Program
    {
        static readonly string testTextPath = @"C:\Users\matej\Desktop\test-text.txt";
        static readonly string outputPath = @"C:\Users\matej\Desktop\compressed.txt";
        static readonly string decompOutputPath = @"C:\Users\matej\Desktop\decompressed.txt";

        private static void Main(string[] args)
        {
            string textToCompress = GetTestText();
            byte[] compressed = Compress(textToCompress);
            WriteToOutput(compressed);

            byte[] output = ReadOutput();
            string decompressed = Decompressor.Decompress(output);
            File.WriteAllText(decompOutputPath, decompressed);

            var decompBytes = File.ReadAllBytes(decompOutputPath);
            var testTextBytes = File.ReadAllBytes(testTextPath);

            WriteCompressionInfo(testTextBytes, decompBytes, compressed);
            Console.ReadKey();
        }

        public static void WriteCompressionInfo(byte[] testTextBytes, byte[] decompBytes, byte[] compBytes)
        {
            bool areIdentical = decompBytes.Length == testTextBytes.Length;
            if (areIdentical)
            {
                for (int i = 0; i < decompBytes.Length; ++i)
                    if (decompBytes[i] != testTextBytes[i])
                    {
                        areIdentical = false;
                        break;
                    }
            }

            Console.WriteLine("\n-------------------\nare identical: " + areIdentical + (areIdentical ? " - Succes!" : ""));
            string originalSize = testTextBytes.Length + " bytes";
            string compressedSize = compBytes.Length + " bytes";

            Console.WriteLine($"original size: {originalSize}.\nCompressed size: {compressedSize}.\n-----------\n reduced by {testTextBytes.Length - compBytes.Length} bytes!");
        }

        public static string Decompress(byte[] text) => Decompressor.Decompress(text);
        public static byte[] Compress(string text) => Compressor.Compress(text);
        public static void WriteToOutput(byte[] text) => File.WriteAllBytes(outputPath, text);
        public static byte[] ReadOutput() => File.ReadAllBytes(outputPath);
        public static string GetTestText() => File.ReadAllText(testTextPath);

        public static void PrintBits(byte @byte)
        {
            for (int i = 0; i < 8; ++i)
                Console.Write((byte)((@byte >> i) & 1));
        }
    }
}