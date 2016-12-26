using System;
using System.Collections.Generic;
using System.Diagnostics;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Chains
{
    public class NormChain : IChain
    {
        public IChain Next { get; }

        public IChain Fail { get; }

        public Node Left { get; private set; }
        public Node Right { get; private set; }

        public NormChain(IChain next, IChain fail = null)
        {
            Next = next;
            Fail = fail;
        }

        public ChainStatus Run(Node node)
        {
            ChainStatus result;
            try
            {
                Left = (Node) node.Operators.First.Value.Item2;
                Right = (Node) node.Operators.Last.Value.Item2;
                Normalize();
                Canonize();

                node = new Node();
                node.Include(default(SignType), Left);
                node.Include(default(SignType), Right);
                return Next?.Run(node) ?? new ChainStatus();
            }
            catch (Exception e)
            {
                Trace.TraceError($"Normalization error {e} - e.StackTrace");
                result = new ChainStatus("Normalization error " + e.Message);
                Log(result.Message);
            }

            return result;
        }

        public void Log(string error)
        {
            Fail?.Log(error);
        }

        private void Normalize()
        {
            Left = (Node)Left.Mult(new Unary(1));
            Right = (Node)Right.Mult(new Unary(1));
            LinkedList<IOperator> divs;
            do
            {
                divs = new LinkedList<IOperator>();
                FindAllDivs(divs, Left.Operators);
                FindAllDivs(divs, Right.Operators);
                foreach (IOperator oper in divs)
                {
                    Left = (Node) Left.Mult(oper);
                    Right = (Node) Right.Mult(oper);
                }
            } while (divs.Count > 0);
        }

        private void Canonize()
        {
            Left = (Node) Left.Sub(Right);
            Right = new Node();
            Right.Include(SignType.Plus, new Unary(0));
        }

        private void FindAllDivs(LinkedList<IOperator> divs, LinkedList<Tuple<SignType, IOperator>> opers)
        {
            foreach (Tuple<SignType, IOperator> tuple in opers)
            {
                if (tuple.Item1 == SignType.Div)
                {
                    divs.AddLast(tuple.Item2);
                }
                else
                {
                    Node node = tuple.Item2 as Node;
                    if (node != null)
                    {
                        FindAllDivs(divs, node.Operators);
                    }
                }
            }
        }

        public void Dispose()
        {
            Next?.Dispose();
            Fail?.Dispose();
        }
    }
}