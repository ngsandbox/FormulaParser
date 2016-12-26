using System;

namespace CanonicalizationApp.Operators
{
    public interface IOperator
    {
        IOperator Sub(IOperator subtrahend);

        IOperator Add(IOperator summand);

        IOperator Mult(IOperator operFactor);

        IOperator Div(IOperator denominator);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other">Other operator</param>
        /// <param name="checkFactor">If true then check equality of operators and factors</param>
        /// <returns></returns>
        bool IsEqual(IOperator other, bool checkFactor);

        IOperator Clone();
    }
}