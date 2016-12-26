using System;
using System.Diagnostics;
using System.IO;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Readers
{
    public class FileReaderChain : IChain
    {
        private bool _disposed;

        private string FileName { get; }

        public FileReaderChain(string fileName, IChain next, IChain fail)
        {
            FileName = fileName;
            Next = next;
            Fail = fail;
        }

        public IChain Next { get; }
        public IChain Fail { get; }

        public void Log(string error)
        {
            Fail?.Log(error);
        }

        public ChainStatus Run(Node node)
        {
            ChainStatus result = new ChainStatus();
            try
            {
                StartRead();
            }
            catch (Exception e)
            {
                Trace.TraceError($"Normalization error {e} - {e.StackTrace}");
                result = new ChainStatus("Normalization error " + e.Message);
                Log(result.Message);
            }

            return result;
        }

        private void StartRead()
        {
            using (StreamReader sr = File.OpenText(FileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Next?.Run(new Node(line));
                }
            }
        }

        public void Dispose()
        {
            Next?.Dispose();
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Fail?.Dispose();
                }

                _disposed = true;
            }
        }

        ~FileReaderChain()
        {
            Dispose(false);
        }
    }
}