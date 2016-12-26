using System;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Readers
{
    public class ConsoleReaderChain : IChain
    {
        public ConsoleReaderChain(IChain next)
        {
            Next = next;
        }

        public IChain Next { get; }

        public IChain Fail { get; }

        public void Log(string error)
        {
            Console.WriteLine("Error catched" + error);
        }

        public ChainStatus Run(Node node)
        {
            while (Next != null)
            {
                Next.Run(new Node(ReadFormula()));
            }

            return new ChainStatus();
        }


        private string ReadFormula()
        {
            Console.Write("Enter formula and press enter: ");
            return Console.ReadLine();
        }

        public void Dispose()
        {
            Next?.Dispose();
            Fail?.Dispose();
        }
    }
}