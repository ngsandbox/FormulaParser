using System;
using System.Diagnostics;
using System.IO;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Readers
{
    public class FileWriterChain : IChain
    {
        private readonly string _fileName;

        private StreamWriter _file;

        protected StreamWriter File => _file ?? (_file = new StreamWriter(_fileName) {AutoFlush = true});

        public void Dispose()
        {
            _file?.Dispose();
            _file = null;
        }

        public IChain Next { get; }

        public IChain Fail { get; }


        public FileWriterChain(string fileName, IChain fail = null)
        {
            _fileName = fileName;
            Fail = fail;
        }

        public ChainStatus Run(Node node)
        {
            ChainStatus result = new ChainStatus();
            try
            {
                File.WriteLine($"{node.Operators.First.Value.Item2} = 0");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Saving result error {e} ");
                result = new ChainStatus("Saving result error " + e.Message);
                Log(result.Message);
            }

            return result;
        }


        public void Log(string error)
        {
            try
            {
                File.WriteLine($"Error process formula: {error}");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Saving error failed {e} ");
            }

            Fail?.Log(error);
        }
    }
}