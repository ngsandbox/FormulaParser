using System;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Readers
{
    public class ConsoleWriteChain : IChain
    {
        public IChain Next { get; }
        public IChain Fail { get; }


        public ChainStatus Run(Node node)
        {
            Write(node);
            return new ChainStatus();
        }

        public void Write(Node node)
        {
            if (node != null)
            {
                IOperator first = node.Operators.First.Value.Item2;
                IOperator last = node.Operators.Count > 1 ? node.Operators.Last.Value.Item2 : null;

                Console.WriteLine($"The result node: {first} = {last}");
            }
            else
            {
                Console.WriteLine("The result node is null");
            }
        }

        public void Dispose()
        {
            Next?.Dispose();
            Fail?.Dispose();
        }

        public void Log(string error)
        {
            Console.WriteLine("Error catched" + error);
        }
    }
}