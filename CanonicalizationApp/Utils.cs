using System;
using CanonicalizationApp.Operators;

namespace CanonicalizationApp
{
    public static class Utils
    {
        /// <summary>
        /// Find next char in formula excluding ignorable 
        /// </summary>
        /// <param name="formula">Expression </param>
        /// <param name="curr">Current position</param>
        public static char FindNextChar(string formula, int curr)
        {
            for (int i = curr + 1; i < formula.Length; i++)
            {
                char next = formula[i];
                if (!IsIgnoredChar(next)) return next;
            }

            return Constants.EMPTY;
        }

        /// <summary>
        /// Checking that a char should be ignored
        /// </summary>
        /// <param name="ch">Current char</param>
        /// <returns>Ignorable char</returns>
        public static bool IsIgnoredChar(char ch)
        {
            return char.IsWhiteSpace(ch);
        }

        /// <summary>
        /// Convert sign enum to character
        /// </summary>
        /// <param name="sign">Enum sign</param>
        /// <returns>Appropriate sign</returns>
        public static char ToChar(SignType sign)
        {
            return sign == SignType.Plus
                ? Constants.PLUS
                : sign == SignType.Minus
                    ? Constants.MINUS
                    : sign == SignType.Mult
                        ? Constants.MULT
                        : Constants.DIV;
        }

        /// <summary>
        /// Convert sign enum to character
        /// </summary>
        /// <param name="ch">Character sign</param>
        /// <returns>Appropriate sign</returns>
        public static SignType ToSign(char ch)
        {
            return ch == Constants.PLUS
                ? SignType.Plus
                : ch == Constants.MINUS
                    ? SignType.Minus
                    : ch == Constants.MULT
                        ? SignType.Mult
                        : SignType.Div;
        }

        public static bool EqualTo(this double value, double val)
        {
            return Math.Abs(value - val) < Constants.FLOAT_TOLERANCE;
        }

        public static IOperator Calc(this IOperator op1, Tuple<SignType, IOperator> tuple)
        {
            return op1.Calc(tuple.Item1, tuple.Item2);
        }

        public static IOperator Calc(this IOperator op1, SignType sign, IOperator op2)
        {
            switch (sign)
            {
                case SignType.Div:
                    if (op2 is Unary)
                        return op1.Div(op2);
                    throw new ArithmeticException("The denominator should be unary");
                case SignType.Minus:
                    return op1.Sub(op2);
                case SignType.Mult:
                    return op1.Mult(op2);
                case SignType.Plus:
                    return op1.Add(op2);
            }

            throw new NotImplementedException(sign.ToString());
        }
    }
}