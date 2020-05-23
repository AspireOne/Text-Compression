using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace HuffmanCompression
{
    public static class Program
    {
        private static readonly string NoArgumentErrorText = "You have not specified any files to compress/decompress.";
        private static readonly string TooManyCharactersExceptionText = "The specified file has too many characters.";
        private static readonly string UnexpectedErrorText = "There was an unexpected error. Skipping.";
        private static readonly string WaitForKeypressText = "\nPress any button to close...";

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
            var runningThreads = new List<Thread>();

            foreach (string path in paths)
            {
                PerformFileAction fileAction = new PerformFileAction
                    (path.EndsWith(Compressor.CompressedFileExtension)
                    ? (PerformFileAction)DecompressFile
                    : (PerformFileAction)CompressFile);

                //TODO: Limit number of threads.  
                var thread = new Thread(() => fileAction.Invoke(path));
                thread.Start();
                runningThreads.Add(thread);
            }

            foreach (Thread thread in runningThreads)
                if (thread.IsAlive)
                    thread.Join();
        }

        private delegate void PerformFileAction(string path);

        private static void CompressFile(string path)
        {
            string filename = Path.GetFileName(path);
            Console.WriteLine($"Compressing {filename}");
            try { Console.WriteLine($"Done compressing {filename}. Summary:\n{Compressor.CompressFile(path)}"); ; }
            catch (TooManyCharactersException) { Console.WriteLine(TooManyCharactersExceptionText); }
            catch (Exception e) { Console.WriteLine($"{UnexpectedErrorText} (exception: {e.GetType()})"); }
        }

        private static void DecompressFile(string path)
        {
            string filename = Path.GetFileName(path);
            Console.WriteLine($"Decompressing {filename}");
            try { Decompressor.DecompressFile(path); Console.WriteLine($"Done decompressing {filename}"); ; }
            catch (Exception e) { Console.WriteLine($"{UnexpectedErrorText} (exception: {e.GetType()})"); }
        }
    }
}