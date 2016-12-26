using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanonicalizationApp.Operators
{
    public class Unary : IOperator
    {
        public double Factor { get; set; }

        /// <summary>
        /// List of variables with
        /// </summary>
        public Dictionary<char, int> Vars { get; }


        public Unary(double factor)
        {
            Factor = factor;
            Vars = new Dictionary<char, int>();
        }

        public Unary(Dictionary<char, int> vars) : this(1)
        {
            Vars = vars;
        }

        public Unary() : this(new Dictionary<char, int>())
        {
        }

        public IOperator Sub(IOperator denominator)
        {
            if (IsEqual(denominator, false))
            {
                return new Unary(new Dictionary<char, int>(Vars)) {Factor = Factor - ((Unary) denominator).Factor};
            }

            throw new ArithmeticException(
                "The type and size of Unary operators should be equals to each other for subtraction");
        }

        public IOperator Add(IOperator summand)
        {
            if (IsEqual(summand, false))
            {
                return new Unary(new Dictionary<char, int>(Vars)) {Factor = Factor + ((Unary) summand).Factor};
            }

            throw new ArithmeticException(
                "The type and size of Unary operators should be equals to each other for addition");
        }

        public IOperator Mult(IOperator operFactor)
        {
            if (operFactor is Node)
            {
                return operFactor.Mult(this);
            }

            Unary c2 = operFactor as Unary;
            Unary result = (Unary) Clone();
            result.Factor = Factor * c2.Factor;
            foreach (KeyValuePair<char, int> c2Var in c2.Vars)
            {
                if (result.Vars.ContainsKey(c2Var.Key))
                {
                    var resVar = result.Vars[c2Var.Key] + c2Var.Value ;
                    if (resVar != 0)
                        result.Vars[c2Var.Key] = resVar;
                    else
                        result.Vars.Remove(c2Var.Key);
                }
                else
                {
                    result.Vars.Add(c2Var.Key, c2Var.Value);
                }
            }

            return result;
        }

        public IOperator Div(IOperator denominator)
        {
            if (denominator == null)
                throw new ArithmeticException("The denominator is not specified");
            if (IsEqual(denominator, true))
                return new Unary(1);

            Unary c2 = denominator as Unary;
            if (c2 == null)
                throw new ArithmeticException("The denominator should be unary for calculation");

            Unary result = (Unary) Clone();
            foreach (KeyValuePair<char, int> c2Var in c2.Vars)
            {
                if (result.Vars.ContainsKey(c2Var.Key))
                {
                    var resVar = result.Vars[c2Var.Key] - c2Var.Value;
                    if (resVar != 0)
                        result.Vars[c2Var.Key] = resVar;
                    else
                        result.Vars.Remove(c2Var.Key);
                }
                else
                {
                    result.Vars.Add(c2Var.Key, -1 * c2Var.Value);
                }
            }

            result.Factor = Factor / c2.Factor;
            return result;
        }


        public static Unary operator /(Unary c1, Unary c2)
        {
            if (c1 == null) throw new ArgumentNullException(nameof(c1));
            return (Unary) c1.Div(c2);
        }

        public static Unary operator *(Unary c1, Unary c2)
        {
            if (c1 == null) throw new ArgumentNullException(nameof(c1));
            return (Unary) c1.Mult(c2);
        }

        public static Unary operator +(Unary c1, Unary c2)
        {
            if (c1 == null) throw new ArgumentNullException(nameof(c1));
            return (Unary) c1.Add(c2);
        }

        public static Unary operator -(Unary c1, Unary c2)
        {
            if (c1 == null) throw new ArgumentNullException(nameof(c1));
            return (Unary) c1.Sub(c2);
        }

        /// <summary>
        /// Checking that variables with degree of the current unary operator is equal to another operator 
        /// </summary>
        /// <param name="other">Another operator</param>
        /// <returns>Equality of operators</returns>
        public bool IsVarsEquals(Unary other)
        {
            if (other == null || Vars.Count != other.Vars.Count) return false;
            return Vars.All(v => other.Vars.ContainsKey(v.Key) && other.Vars[v.Key] == Vars[v.Key]);
        }

        public bool IsEqual(IOperator other, bool checkFactor)
        {
            if (this == other) return true;
            Unary othOp = other as Unary;
            bool result = IsVarsEquals(othOp);
            if (result && checkFactor)
                result = Math.Abs(othOp.Factor - Factor) < 0.001;

            return result;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            StringBuilder numer = new StringBuilder();
            StringBuilder denom = new StringBuilder();
            foreach (KeyValuePair<char, int> pair in Vars)
            {
                StringBuilder builder = pair.Value > 0 ? numer : denom;
                builder.Append(pair.Key);
                int power = Math.Abs(pair.Value);
                if (power != 1)
                {
                    builder.Append("^" + power);
                }
            }

            if ((numer.Length == 0 && denom.Length > 0) || !Factor.EqualTo(1))
                result.AppendFormat("{0:0.##}", Factor);

            result.Append(numer);
            if (denom.Length > 0)
            {
                result.Append("/");
                result.Append(denom);
            }

            return result.ToString();
        }

        public IOperator Clone()
        {
            return new Unary(new Dictionary<char, int>(Vars)) {Factor = Factor};
        }
    }
}