using CanonicalizationApp.Chains;
using CanonicalizationApp.Readers;

namespace CanonicalizationApp
{
    public static class ChainBuilder
    {
        public static IChain GetConsoleChain()
        {
            IChain writer = new ConsoleWriteChain();
            return
                new ConsoleReaderChain(new ValidatorChain(new ParserChain(new NormChain(writer, writer), writer), writer));
        }

        public static IChain GetFileChain(string fileName)
        {
            IChain writer = new ConsoleWriteChain();
            IChain fileWriter = new FileWriterChain(fileName + ".out", writer);
            return new FileReaderChain(fileName,
                new ValidatorChain(new ParserChain(new NormChain(fileWriter, fileWriter), fileWriter), fileWriter), fileWriter);
        }
    }
}