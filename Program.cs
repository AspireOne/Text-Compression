using System;
using System.IO;
using System.Linq;

namespace HuffmanCompression
{
    public static class Program
    {
        private static readonly string NoArgumentErrorText = "You have not specified any files to compress/decompress.";
        private static readonly string TooManyCharactersExceptionText = "The specified file has too many characters.";
        private static readonly string UnexpectedErrorText = "There was an unexpected error. Skipping.";
        private static readonly string WaitForKeypressText = "Press any button to close...";
        private static readonly string ProcessDoneText = "Done.";

        private static void Main(string[] args)
        {
            if (!args.Any())
                Console.WriteLine(NoArgumentErrorText);
            else
                DoActionsOnFiles(args);

            WaitForKeyPress();
        }

        private static void WaitForKeyPress()
        {
            Console.WriteLine(WaitForKeypressText);
            Console.ReadKey();
        }

        private static void DoActionsOnFiles(string[] paths)
        {
            foreach (string path in paths)
            {
                if (Path.GetExtension(path).Equals(Compressor.CompressedFileExtension))
                {
                    Console.WriteLine($"Decompressing {path}...");
                    try { Decompressor.DecompressFile(path); Console.WriteLine(ProcessDoneText); }
                    catch (Exception e) { Console.WriteLine($"{UnexpectedErrorText} (exception: {e.GetType()})"); }
                }
                else
                {
                    Console.WriteLine($"Compressing {path}...");
                    try { Console.WriteLine($"{ProcessDoneText}. Summary:\n{Compressor.CompressFile(path)}\n"); }
                    catch (TooManyCharactersException) { Console.WriteLine(TooManyCharactersExceptionText); }
                    catch (Exception e) { Console.WriteLine($"{UnexpectedErrorText} (exception: {e.GetType()})"); }
                }
            }
        }
    }
}