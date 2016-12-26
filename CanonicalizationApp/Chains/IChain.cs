using System;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Chains
{
    public interface IChain : IDisposable
    {
        IChain Next { get; }

        IChain Fail { get; }

        ChainStatus Run(Node node);

        void Log(string error);
    }
}