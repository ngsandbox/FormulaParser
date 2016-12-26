using System;

namespace CanonicalizationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
                "The application runs without parameters as console application or can receive file name as an argument");

            var start = args.Length == 0 ? ChainBuilder.GetConsoleChain() : ChainBuilder.GetFileChain(args[0]);

            using (start)
            {
                start.Run(null);
            }
        }
    }
}