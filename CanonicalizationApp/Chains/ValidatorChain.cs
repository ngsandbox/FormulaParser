using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp.Chains
{
    public class ValidatorChain : IChain
    {
        public IChain Next { get; }
        public IChain Fail { get; }

        private readonly StringBuilder _warnings = new StringBuilder();

        public ValidatorChain(IChain next, IChain fail)
        {
            Next = next;
            Fail = fail;
        }

        public ChainStatus Run(Node node)
        {
            try
            {
                if (!IsValid(node))
                {
                    var status = new ChainStatus(_warnings.ToString());
                    Log(status.Message);
                    return status;
                }

                return Next?.Run(node) ?? new ChainStatus();
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

        bool IsValid(Node node)
        {
            string formula = node.Formula;
            if (IsValid(!string.IsNullOrWhiteSpace(formula), "Formula is empty!"))
            {
                char prev = Constants.EMPTY;
                bool result = true;
                int eqCount = 0;
                int openedBrackets = 0;
                for (int i = 0; i < formula.Length; i++)
                {
                    char curr = formula[i];
                    if (Utils.IsIgnoredChar(curr)) continue;

                    char next = Utils.FindNextChar(formula, i);
                    switch (curr)
                    {
                        case Constants.POINT:
                            result = IsValid(IsPointValid(prev, next), GetSignWarn(curr, i)) && result;
                            break;
                        case Constants.LB:
                            result = IsValid(IsLeftBracketValid(prev, next), GetSignWarn(curr, i)) && result;
                            openedBrackets++;
                            break;
                        case Constants.RB:
                            result = IsValid(IsRightBracketValid(prev, next), GetSignWarn(curr, i)) && result;
                            openedBrackets--;
                            break;
                        case Constants.EQ:
                            eqCount++;
                            result = eqCount == 1 && IsValid(IsEqualsValid(prev, next), GetSignWarn(curr, i)) && result;
                            result = IsValid(openedBrackets == 0, "Not all brackets are closed!") && result;
                            openedBrackets = 0;
                            break;
                        case Constants.MINUS:
                        case Constants.PLUS:
                            result = IsValid(IsMinusPlusValid(prev, next), GetSignWarn(curr, i)) && result;
                            break;
                        case Constants.MULT:
                        case Constants.DIV:
                            result = IsValid(IsDivMulValid(prev, next), GetSignWarn(curr, i)) && result;
                            break;
                        case Constants.POWER:
                            result = IsValid(IsPowerValid(prev, next), GetSignWarn(curr, i)) && result;
                            break;
                        default:
                            if (!IsValid(char.IsLetterOrDigit(curr), GetSignWarn(curr, i)))
                            {
                                result = false;
                                continue;
                            }
                            break;
                    }

                    prev = curr;
                }

                result = IsValid(eqCount == 1, "Too many equals sings!") && result;
                result = IsValid(openedBrackets == 0, "Not all brackets are closed!") && result;

                return result;
            }

            return false;
        }

        private bool IsPointValid(char prev, char next)
        {
            return char.IsDigit(prev) && char.IsDigit(next);
        }

        private bool IsRightBracketValid(char prev, char next)
        {
            return prev != Constants.EMPTY && (char.IsLetterOrDigit(prev) || prev == Constants.RB) &&
                   (next == Constants.EMPTY || next == Constants.EQ || char.IsLetterOrDigit(next) ||
                    Constants.MATHS.Contains(next));
        }

        private bool IsLeftBracketValid(char prev, char next)
        {
            return (prev == Constants.EMPTY || prev == Constants.EQ || Constants.MATHS.Contains(prev) ||
                    char.IsLetterOrDigit(prev)) &&
                   (char.IsLetterOrDigit(next) || next == Constants.LB || Constants.PLUS_MINUS.Contains(next));
        }

        string GetSignWarn(char ch, int pos)
        {
            return $"'{ch}': {pos}!";
        }

        private bool IsEqualsValid(char prev, char next)
        {
            return prev != Constants.EMPTY && (char.IsLetterOrDigit(prev) || prev == Constants.RB) &&
                   next != Constants.EMPTY && (char.IsLetterOrDigit(next) || next == Constants.LB);
        }

        private bool IsPowerValid(char prev, char next)
        {
            return (prev == Constants.RB || char.IsLetterOrDigit(prev)) && char.IsDigit(next);
        }

        private bool IsDivMulValid(char prev, char next)
        {
            return prev != Constants.EMPTY && (char.IsLetterOrDigit(prev) || prev == Constants.RB) &&
                   next != Constants.EMPTY && (char.IsLetterOrDigit(next) || next == Constants.LB);
        }

        /// <summary>
        /// Checking that Minus or Plus position is valid
        /// </summary>
        /// <param name="prev">Previous character</param>
        /// <param name="next">Next character</param>
        private static bool IsMinusPlusValid(char prev, char next)
        {
            return (prev == char.MaxValue || char.IsLetterOrDigit(prev) || prev == Constants.RB) &&
                   next != char.MaxValue && (char.IsLetterOrDigit(next) || next == Constants.LB);
        }


        bool IsValid(bool exp, string warning)
        {
            if (!exp) _warnings.Append("  ").Append(warning);
            return exp;
        }

        public void Dispose()
        {
            Next?.Dispose();
            Fail?.Dispose();
        }
    }
}