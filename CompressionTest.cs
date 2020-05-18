using System.IO;

namespace HuffmanCompression
{
    public static class CompressionTest
    {
        static readonly string testFilesPath = @"C:\Users\matej\Desktop\";
        static readonly string testTextPath = testFilesPath + "test-text.txt";
        static readonly string compressedPath = testFilesPath + "compressed.txt";
        static readonly string decompressedPath = testFilesPath + "decompressed.txt";

        public static string ExecuteCompressionTest()
        {
            // Compress and write text to file.  
            byte[] compressed = Compressor.Compress(File.ReadAllText(testTextPath));
            File.WriteAllBytes(compressedPath, compressed);
            byte[] compressedFileBytes = File.ReadAllBytes(compressedPath);

            // Decompress and write text to file.  
            string decompressed = Decompressor.Decompress(compressedFileBytes);
            File.WriteAllText(decompressedPath, decompressed);
            var decompressedFileBytes = File.ReadAllBytes(decompressedPath);

            // Write info about the compression.  
            var testTextBytes = File.ReadAllBytes(testTextPath);
            return Compressor.GetCompressionSummary(testTextBytes, compressed);
        }
    }
}
