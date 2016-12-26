using System;

namespace CanonicalizationApp
{
    public class Constants
    {
        public const char POINT = '.';
        public const char EQ = '=';
        public const char LB = '(';
        public const char RB = ')';
        public const char PLUS = '+';
        public const char MINUS = '-';
        public const char POWER = '^';
        public const char DIV = '/';
        public const char MULT = '*';
        public const char EMPTY = Char.MaxValue;
        public static readonly char[] MATHS = {PLUS, MINUS, DIV, MULT};
        public static readonly char[] PLUS_MINUS = {PLUS, MINUS};
        public static readonly double FLOAT_TOLERANCE = 0.0001;

    }
}