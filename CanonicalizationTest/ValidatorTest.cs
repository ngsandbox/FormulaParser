using System.Diagnostics;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CanonicalizationTest
{
    [TestClass]
    public class ValidatorTest
    {
        IChain _parser;

        [TestInitialize]
        public void Init()
        {
            _parser = new ValidatorChain(null, null);;
        }

        [TestMethod]
        public void ValidSimpleFormula()
        {
            ChainStatus result = _parser.Run(new Node("a+b"));
            Assert.IsNotNull(result, "Validator result is not initialized!");
        }

        [TestMethod]
        public void ValidContainsEq()
        {
            ChainStatus result = _parser.Run(new Node("a+b=c"));
            Trace.WriteLine(result.Message);
            Assert.IsTrue(result.IsValid, result.Message);
        }

        [TestMethod]
        public void ValidComplexWithBrackets()
        {
            ChainStatus result = _parser.Run(new Node("-(a + b)x + b=c + m"));
            Trace.WriteLine(result.Message);
            Assert.IsTrue(result.IsValid, result.Message);
        }

        [TestMethod]
        public void ValidComplexWithPower()
        {
            ChainStatus result = _parser.Run(new Node("+1.1(a + b)x ^ 3 + b = c^7 + l"));
            Trace.WriteLine(result.Message);
            Assert.IsTrue(result.IsValid, result.Message);
        }

        [TestMethod]
        public void InvalidContainsTwoEq()
        {
            ChainStatus result = _parser.Run(new Node("a+b=c=b"));
            Trace.WriteLine(result.Message);
            Assert.IsFalse(result.IsValid, result.Message);
        }

        [TestMethod]
        public void InvalidBrackets()
        {
            ChainStatus result = _parser.Run(new Node("a + (b - v)) = c"));
            Trace.WriteLine(result.Message);
            Assert.IsFalse(result.IsValid, result.Message);
        }

        [TestMethod]
        public void InvalidStartDiv()
        {
            ChainStatus result = _parser.Run(new Node("/a + (b - v) = c"));
            Trace.WriteLine(result.Message);
            Assert.IsFalse(result.IsValid, result.Message);
        }

        [TestMethod]
        public void InvalidPoint()
        {
            ChainStatus result = _parser.Run(new Node("-1..234a + (b - v) = c"));
            Trace.WriteLine(result.Message);
            Assert.IsFalse(result.IsValid, result.Message);
        }
    }
}