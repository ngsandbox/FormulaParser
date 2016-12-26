using System;
using System.Diagnostics;
using System.Text;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Chains
{
    public class ParserChain : IChain
    {
        public Node LeftNode { get; private set; }
        public Node RightNode { get; private set; }
        StringBuilder _warnings;

        public IChain Next { get; }
        public IChain Fail { get; }

        public ParserChain(IChain chain, IChain fail)
        {
            Next = chain;
            Fail = fail;
        }

        public ChainStatus Run(Node node)
        {
            try
            {
                _warnings = new StringBuilder();
                LeftNode = null;
                RightNode = null;
                if (!ProcessFormula(node.Formula))
                {
                    ChainStatus status = new ChainStatus(_warnings.ToString());
                    Log("Parser errors: " + status.Message);
                    return status;
                }

                Node result = new Node();
                result.Include(default(SignType), LeftNode);
                result.Include(default(SignType), RightNode);
                return Next?.Run(result) ?? new ChainStatus();
            }
            catch (Exception e)
            {
                ChainStatus status = new ChainStatus("Unexpected error " + e.Message);
                Trace.TraceError(status.Message + e.StackTrace);
                Log(status.Message);
                return status;
            }
        }

        public void Log(string error)
        {
            Fail?.Log(error);
        }


        public bool ProcessFormula(string formula)
        {
            Node expression = new Node();
            string variable = "";
            SignType sign = SignType.Plus;
            char prev = Constants.EMPTY;
            foreach (char ch in formula)
            {
                if (Utils.IsIgnoredChar(ch)) continue;
                switch (ch)
                {
                    case Constants.LB:
                    {
                        Node exp = new Node(expression);
                        if (!string.IsNullOrWhiteSpace(variable) || prev == Constants.RB)
                        {
                            AddUnaryOperator(expression, ref variable, ref sign);
                            expression.Include(SignType.Mult, exp);
                        }
                        else
                        {
                            expression.Include(sign, exp);
                            sign = SignType.Plus;
                        }

                        expression = exp;
                        break;
                    }
                    case Constants.RB:
                        AddUnaryOperator(expression, ref variable, ref sign);
                        sign = SignType.Plus;
                        expression = expression.Parent;
                        break;
                    case Constants.MULT:
                    case Constants.DIV:
                    case Constants.PLUS:
                    case Constants.MINUS:
                        AddUnaryOperator(expression, ref variable, ref sign);
                        sign = Utils.ToSign(ch);
                        break;
                    case Constants.EQ:
                        AddUnaryOperator(expression, ref variable, ref sign);
                        LeftNode = expression;
                        expression = new Node();
                        sign = SignType.Plus;
                        break;
                    default:
                        variable += ch;
                        break;
                }

                prev = ch;
            }

            AddUnaryOperator(expression, ref variable, ref sign);
            RightNode = expression;
            return true;
        }

        private void AddUnaryOperator(Node expression, ref string variable, ref SignType sign)
        {
            if (!string.IsNullOrWhiteSpace(variable))
            {
                expression.Include(sign, ConvertToUnary(variable));
                variable = String.Empty;
                sign = SignType.Plus;
            }
        }

        public Unary ConvertToUnary(string variable)
        {
            Unary oper = new Unary();
            var num = string.Empty;
            bool isPower = false;
            char currentVar = Constants.EMPTY;
            foreach (char ch in variable)
            {
                switch (ch)
                {
                    case Constants.POWER:
                        isPower = true;
                        break;
                    case Constants.POINT:
                        num += ch;
                        break;
                    default:
                        if (char.IsLetter(ch))
                        {
                            AddVariableOrFactor(oper, currentVar, isPower, num);
                            isPower = false;
                            num = string.Empty;
                            currentVar = ch;
                        }

                        if (char.IsDigit(ch))
                        {
                            num += ch;
                        }

                        break;
                }
            }

            AddVariableOrFactor(oper, currentVar, isPower, num);
            return oper;
        }

        private static void AddVariableOrFactor(Unary oper, char currentVar, bool isPower, string num)
        {
            if (currentVar != Constants.EMPTY)
            {
                oper.Vars.Add(currentVar, isPower && !string.IsNullOrWhiteSpace(num) ? int.Parse(num) : 1);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(num))
                {
                    oper.Factor = double.Parse(num);
                }
            }
        }

        public void Dispose()
        {
            Next?.Dispose();
        }
    }
}