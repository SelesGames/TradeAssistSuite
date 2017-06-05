using System;

namespace TradeAssistSuite
{
    public static class BitcoinMath
    {
        public static decimal Multiply(decimal a, decimal b)
        {
            return Math.Round(a * b, 8);
        }

        public static decimal Divide(decimal a, decimal b)
        {
            return b == 0 ? 0 : Math.Round(a / b, 8);
        }

        public static decimal Round(decimal a)
        {
            return Math.Round(a, 8);
        }
    }
}