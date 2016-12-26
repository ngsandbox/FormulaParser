using System;
using System.Diagnostics;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CanonicalizationTest
{
    [TestClass]
    public class MathTest
    {
        ValidatorChain _validator;
        ParserChain _parser;
        NormChain _norm;

        [TestInitialize]
        public void Init()
        {
            _norm = new NormChain(null);
            _parser = new ParserChain(_norm, null);
            _validator = new ValidatorChain(_parser, null);
        }

        [TestMethod]
        public void UnaryMultTest()
        {
            Unary c1 = _parser.ConvertToUnary("3xy^3");
            Unary c2 = _parser.ConvertToUnary("4yb");
            Unary result = (Unary) c1.Mult(c2);
            Trace.WriteLine($"3xy^3 * 4yb = {result}");
            Assert.AreEqual(result.Factor, 12, 0.0001, "Factor should be equals to 12");
            Assert.IsTrue(result.Vars.Count == 3, "Count of variables does not equals to 3");
            Assert.IsTrue(result.Vars['y'] == 4, "Power of y variable should be equal to 4");
        }

        [TestMethod]
        public void UnaryDivTest()
        {
            Unary c1 = _parser.ConvertToUnary("8xy^3");
            Unary c2 = _parser.ConvertToUnary("4ybx");
            Unary result = (Unary) c1.Div(c2);
            Trace.WriteLine($"8xy^3 / 4ybx = {result}");
            Assert.AreEqual(result.Factor, 2, 0.0001, "Factor should be equals to 2");
            Assert.IsTrue(result.Vars.Count == 2, "Count of variables does not equals to 2");
            Assert.IsTrue(result.Vars['y'] == 2, "Power of y variable should be equal to 2");
        }

        [TestMethod]
        public void UnarySubTest()
        {
            Unary c1 = _parser.ConvertToUnary("8xy");
            Unary c2 = _parser.ConvertToUnary("4yx");
            Unary result = (Unary) c1.Sub(c2);
            Trace.WriteLine($"8xy - 4yx = {result}");
            Assert.AreEqual(result.Factor, 4, 0.0001, "Factor should be equals to 4");
            Assert.IsTrue(result.Vars.Count == 2, "Count of variables does not equals to 2");
            Assert.IsTrue(result.Vars['y'] == 1, "Power of y variable should be equal to 1");
        }

        [TestMethod]
        public void UnaryAddTest()
        {
            Unary c1 = _parser.ConvertToUnary("8cd^2b");
            Unary c2 = _parser.ConvertToUnary("4d^2cb");
            Unary result = (Unary) c1.Add(c2);
            Trace.WriteLine($"8cd^2b + 4d^2cb = {result}");
            Assert.AreEqual(result.Factor, 12, 0.0001, "Factor should be equals to 12");
            Assert.IsTrue(result.Vars.Count == 3, "Count of variables does not equals to 3");
            Assert.IsTrue(result.Vars['d'] == 2, "Power of y variable should be equal to 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArithmeticException), "Unary params should be equals for addition")]
        public void UnaryAddException()
        {
            Unary c1 = _parser.ConvertToUnary("8cd^2b");
            Unary c2 = _parser.ConvertToUnary("4dcb");
            Unary result = (Unary) c1.Add(c2);
            Trace.WriteLine($"8cd^2b + 4dcb = {result}");
        }

        [TestMethod]
        [ExpectedException(typeof(ArithmeticException), "Unary params should be equals for subtraction")]
        public void UnarySubException()
        {
            Unary c1 = _parser.ConvertToUnary("8xy^2");
            Unary c2 = _parser.ConvertToUnary("10yx");
            Unary result = (Unary) c1.Sub(c2);
            Trace.WriteLine($"8xy^2 - 10yx = {result}");
        }

        [TestMethod]
        public void NodeMultTest()
        {
            _parser.ProcessFormula("xy(x - y + 5) = 0");
            Node node = _parser.LeftNode;
            Node result = node.Operators.Last.Value.Item2.Mult(node.Operators.First.Value.Item2) as Node;
            Assert.IsNotNull(result, "Expression node result is expected");
            Trace.WriteLine($"xy(x - y + 5) = {result}");
            Assert.IsTrue(result.Operators.Count == 3, "Count of operators should be equal to 3");
        }

        [TestMethod]
        public void NodeDivTest()
        {
            _parser.ProcessFormula("(x - y + 5/x)/xy = 0");
            //===== 1/y-1/x+/x^2y
            Node node = _parser.LeftNode;
            Node result = node.Operators.First.Value.Item2.Div(node.Operators.Last.Value.Item2) as Node;
            Assert.IsNotNull(result, "Expression node result is expected");
            Trace.WriteLine($"(x - y + 5/x)/xy = {result}");
            Assert.IsTrue(result.Operators.Count == 3, "Count of operators should be equal to 3");
        }

        [TestMethod]
        public void NodeComplexMultTest()
        {
            _parser.ProcessFormula("(b + c - (bc + ba / xy))(x + y)= 0");
            //=====         xb+yb+xc+yc-bcx-ba/y-bcy-ba/x
            Node node = _parser.LeftNode;
            Node result = node.Operators.First.Value.Item2.Mult(node.Operators.Last.Value.Item2) as Node;
            Assert.IsNotNull(result, "Expression node result is expected");
            Trace.WriteLine($"(b + c - (bc + ba / xy))(x + y) =  {result}");
            Assert.IsTrue(result.Operators.Count == 8, "Count of operators should be equal to 3");
        }

        [TestMethod]
        public void NodeNormalization()
        {
            string inFormula = "b + c - (bc + ba / xy)= bc ";
            ChainStatus chainStatus = _validator.Run(new Node(inFormula));
            //=====-bx-cx-bcx+bax/xy-by-cy-bcy+bay/xy

            Assert.IsNotNull(chainStatus, "Status is not initialized");
            Assert.IsTrue(chainStatus.IsValid, "Status is not valid: " + chainStatus.Message);
            Trace.WriteLine($"{inFormula}. Result: {_norm.Left}={_norm.Right}");
            Assert.IsInstanceOfType(_norm.Right.Operators.First.Value.Item2, typeof(Unary),
                "The resutl right operator is not Unary");
        }

        [TestMethod]
        public void Norm()
        {
            string inFormula = "x + y - x = x / y";
            ChainStatus chainStatus = _validator.Run(new Node(inFormula));
            //=====-bx-cx-bcx+bax/xy-by-cy-bcy+bay/xy

            Assert.IsNotNull(chainStatus, "Status is not initialized");
            Assert.IsTrue(chainStatus.IsValid, "Status is not valid: " + chainStatus.Message);
            Trace.WriteLine($"{inFormula}. Result: {_norm.Left}={_norm.Right}");
            Assert.IsInstanceOfType(_norm.Right.Operators.First.Value.Item2, typeof(Unary),
                "The resutl right operator is not Unary");
        }

        [TestMethod]
        public void NormTask()
        {
            string inFormula = "x^2 + 3.5xy + y = y^2 - xy + y";
            ChainStatus chainStatus = _validator.Run(new Node(inFormula));
            //=====  x^2 - y^2 + 4.5xy = 0

            Assert.IsNotNull(chainStatus, "Status is not initialized");
            Assert.IsTrue(chainStatus.IsValid, "Status is not valid: " + chainStatus.Message);
            Trace.WriteLine($"{inFormula} = {_norm.Left}={_norm.Right}");
            Assert.AreEqual(_norm.Left.Operators.Count, 3, "The left operators should be equal to 3 ");
        }
    }
}