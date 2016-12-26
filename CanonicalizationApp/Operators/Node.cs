using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalizationApp.Operators
{
    public enum SignType
    {
        Plus,
        Minus,
        Mult,
        Div
    }

    public class Node : IOperator
    {
        public string Formula { get; }

        public LinkedList<Tuple<SignType, IOperator>> Operators { get; }

        public Node Parent { get; private set; }

        public Node()
        {
            Operators = new LinkedList<Tuple<SignType, IOperator>>();
        }

        public Node(string formula) : this()
        {
            Formula = formula;
        }

        public Node(Node parent) : this()
        {
            Parent = parent;
        }

        public Node(SignType sign, IOperator oper) : this()
        {
            Include(sign, oper);
        }

        public void Include(SignType sign, IOperator oper)
        {
            Operators.AddLast(new Tuple<SignType, IOperator>(sign, oper));
            var node = oper as Node;
            if (node != null)
            {
                node.Parent = this;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (Tuple<SignType, IOperator> pair in Operators)
            {
                int len = result.Length;
                SignType sign = pair.Item1;
                IOperator op = pair.Item2;
                Node opNode = pair.Item2 as Node;

                if (len != 0 || sign != SignType.Plus)
                {
                    result.Append(Utils.ToChar(sign));
                }

                if (opNode != null)
                {
                    result.Append(Constants.LB);
                }

                result.Append(op);

                if (opNode != null)
                {
                    result.Append(Constants.RB);
                }
            }
            return result.ToString();
        }

        public bool IsEqual(IOperator other, bool checkFactor)
        {
            if (this == other) return true;
            bool result = other != null && other.GetType() == typeof(Node);
            result = result && IsOperatorsEquals((Node) other);
            return result;
        }

        /// <summary>
        /// Equality node operators only for normalization process
        /// </summary>
        /// <param name="node">Other node</param>
        /// <returns></returns>
        private bool IsOperatorsEquals(Node node)
        {
            bool result = Operators.Count == node.Operators.Count;
            if (result)
            {
                LinkedListNode<Tuple<SignType, IOperator>> oper = Operators.First;
                LinkedListNode<Tuple<SignType, IOperator>> nodeOper = node.Operators.First;
                while (result && oper != null && nodeOper != null)
                {
                    result = oper.Value.Item1 == nodeOper.Value.Item1 &&
                             oper.Value.Item2.IsEqual(nodeOper.Value.Item2, true);
                    oper = oper.Next;
                    nodeOper = nodeOper.Next;
                }
            }

            return result;
        }

        bool IsAllowedSign(SignType sign)
        {
            return sign == SignType.Minus || sign == SignType.Plus;
        }

        private void CalcUnary(Node node, SignType unSign, Unary un)
        {
            LinkedListNode<Tuple<SignType, IOperator>> subOper = node.Operators.First;
            while (subOper != null)
            {
                var next = subOper.Next;
                IOperator oper = subOper.Value.Item2;
                SignType sign = subOper.Value.Item1;
                if (IsAllowedSign(sign))
                {
                    bool canCalc = next == null || (IsAllowedSign(next.Value.Item1));
                    if (canCalc && oper.IsEqual(un, false))
                    {
                        un = (Unary) oper.Calc(sign == unSign ? SignType.Plus : SignType.Minus, un);
                        node.Operators.Remove(subOper);
                        if (un.Factor.EqualTo(0)) return;
                    }
                }

                subOper = next;
            }

            node.Include(unSign, un.Clone());
        }

        public IOperator Sub(IOperator subtrahend)
        {
            if (IsEqual(subtrahend, true)) return new Unary(0);
            Node result = (Node) Clone();
            var unary = subtrahend as Unary;
            if (unary != null)
            {
                CalcUnary(result, SignType.Minus, unary);
            }
            else
            {
                Node subNode = (Node) subtrahend;
                LinkedListNode<Tuple<SignType, IOperator>> subOperNode = subNode.Operators.First;
                while (subOperNode != null)
                {
                    var next = subOperNode.Next;
                    IOperator subOper = subOperNode.Value.Item2;
                    SignType subSign = subOperNode.Value.Item1;
                    subSign = subSign == SignType.Minus ? SignType.Plus : SignType.Minus;
                    bool canSub = IsAllowedSign(subSign) && (next == null || IsAllowedSign(next.Value.Item1));
                    if (!canSub)
                    {
                        while (next != null && (next.Value.Item1 == SignType.Mult || next.Value.Item1 == SignType.Div))
                        {
                            subOper = subOper.Calc(next.Value);
                            next = next.Next;
                        }
                    }

                    result = (Node) result.Calc(subSign, subOper);
                    subOperNode = next;
                }
            }

            return result;
        }

        public IOperator Add(IOperator summand)
        {
            Node result = (Node) Clone();
            var unary = summand as Unary;
            if (unary != null)
            {
                CalcUnary(result, SignType.Plus, unary);
            }
            else
            {
                Node sumNode = (Node) summand;
                LinkedListNode<Tuple<SignType, IOperator>> sumOperNode = sumNode.Operators.First;
                while (sumOperNode != null)
                {
                    var next = sumOperNode.Next;
                    IOperator sumOper = sumOperNode.Value.Item2;
                    SignType operSign = sumOperNode.Value.Item1;
                    bool canSum = (operSign == SignType.Plus || operSign == SignType.Minus) &&
                                  (next == null || next.Value.Item1 != SignType.Div && next.Value.Item1 != SignType.Mult);
                    if (!canSum)
                    {
                        while (next != null && (next.Value.Item1 == SignType.Mult || next.Value.Item1 == SignType.Div))
                        {
                            sumOper = sumOper.Calc(next.Value);
                            next = next.Next;
                        }
                    }

                    result = (Node) result.Calc(operSign, sumOper);
                    sumOperNode = next;
                }
            }

            return result;
        }

        public IOperator Mult(IOperator operFactor)
        {
            Node result = new Node();
            LinkedListNode<Tuple<SignType, IOperator>> nodeCur = Operators.First;
            while (nodeCur != null)
            {
                IOperator operNode = nodeCur.Value.Item2;
                SignType signNode = nodeCur.Value.Item1;

                var next = nodeCur.Next;
                bool equalFound = false;
                while (next != null && (next.Value.Item1 == SignType.Mult || next.Value.Item1 == SignType.Div))
                {
                    IOperator inOper = next.Value.Item2;
                    SignType inSign = next.Value.Item1;
                    // remove if operators equal to each other
                    if (equalFound || inSign != SignType.Div || !inOper.IsEqual(operFactor, true))
                        operNode = operNode.Calc(inSign, inOper);
                    else
                        equalFound = true;

                    next = next.Next;
                }

                if (!equalFound)
                    operNode = operNode.Calc(SignType.Mult, operFactor);

                result = (Node) result.Calc(signNode, operNode);
                nodeCur = next;
            }


            return result;
        }

        public IOperator Div(IOperator denominator)
        {
            if (denominator == null)
                throw new ArithmeticException("Denominator is not initialized");
            if (denominator is Node)
                throw new ArithmeticException("Only Unary operators allowed as denominators");
            Node result = new Node();
            LinkedListNode<Tuple<SignType, IOperator>> node = Operators.First;
            while (node != null)
            {
                IOperator operNode = node.Value.Item2;
                SignType operSign = node.Value.Item1;

                IOperator divResult = operNode.Div(denominator);
                var inNext = node.Next;
                while (inNext != null && (inNext.Value.Item1 == SignType.Mult || inNext.Value.Item1 == SignType.Div))
                {
                    divResult = divResult.Calc(inNext.Value);
                    inNext = inNext.Next;
                }

                result = (Node) result.Calc(operSign, divResult);
                node = inNext;
            }

            return result;
        }

        public IOperator Clone()
        {
            Node result = new Node();
            foreach (Tuple<SignType, IOperator> tuple in Operators)
            {
                result.Include(tuple.Item1, tuple.Item2.Clone());
            }

            return result;
        }
    }
}