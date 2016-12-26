using System.Diagnostics;
using CanonicalizationApp.Chains;
using CanonicalizationApp.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CanonicalizationTest
{
    [TestClass]
    public class ParserTest
    {
        private ValidatorChain _validator;
        ParserChain _parser;

        [TestInitialize]
        public void Init()
        {
            _parser = new ParserChain(null, null);
            _validator = new ValidatorChain(_parser, null);
        }


        [TestMethod]
        public void UnaryWithVars()
        {
            Unary oper = _parser.ConvertToUnary("3.5xy");
            Trace.WriteLine("Result: " + oper);
            Assert.AreEqual(oper.Factor, 3.5, 0.001, "Factor is not equal to 3.5");
            Assert.IsTrue(oper.Vars.ContainsKey('x'), "x variable not found");
            Assert.IsTrue(oper.Vars.ContainsKey('y'), "y variable not found");
        }

        [TestMethod]
        public void UnaryWithPower()
        {
            Unary oper = _parser.ConvertToUnary("3x^2y");
            Trace.WriteLine("Result: " + oper);
            Assert.AreEqual(oper.Factor, 3, 0.001, "Factor is not equal to 3");
            Assert.IsTrue(oper.Vars.ContainsKey('x'), "x variable not found");
            Assert.AreEqual(oper.Vars['x'], 2, "Power of X is not equal to 2");
            Assert.IsTrue(oper.Vars.ContainsKey('y'), "y variable not found");
        }

        [TestMethod]
        public void ExpressionsNotNull()
        {
            _validator.Run(new Node("x^2 + 3.5xy + y = y^2 - (xy + y)"));
            Node leftNode = _parser.LeftNode;
            Node rightNode = _parser.RightNode;
            Assert.IsNotNull(leftNode, "Left node is not initialized");
            Assert.IsNotNull(rightNode, "Left node is not initialized");
        }

        [TestMethod]
        public void ValidCompexExpression()
        {
            var status = _validator.Run(new Node("ab^2 + bc^2 - ( 5 + d) = d +(d+c)+5"));
            Node leftNode = _parser.LeftNode;
            Node rightNode = _parser.RightNode;
            Assert.IsTrue(status.IsValid, "Formula should be valid! " + status.Message);
            Assert.IsNotNull(leftNode, "Left node is not initialized");
            Assert.IsNotNull(rightNode, "Left node is not initialized");
        }

        [TestMethod]
        public void ExpressionOperatorsValid()
        {
            var status = _validator.Run(new Node("x^2 + 3.5xy + y + 5 = y^2 - (xy + y)"));
            Node leftNode = _parser.LeftNode;
            Node rightNode = _parser.RightNode;
            Assert.AreEqual(leftNode.Operators.Count, 4, "Count of left operators does not equal to 4");
            Assert.AreEqual(rightNode.Operators.Count, 2, "Count of right operators does not equal to 2");
            Assert.IsInstanceOfType(rightNode.Operators.Last.Value.Item2, typeof(Node),
                "The type of the last item right expression should be Node!");
            Assert.AreEqual(((Node) rightNode.Operators.Last.Value.Item2).Operators.Count, 2,
                "Count of the last item right operators does not equal to 2");
        }
    }
}