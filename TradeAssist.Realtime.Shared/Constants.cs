using System.Collections.Generic;
using System.Globalization;

namespace TradeAssist.Realtime
{
    internal static class Constants
    {
        public static readonly string Poloniex = "poloniex";
        public static readonly string Bittrex = "bittrex";

        static readonly CultureInfo culture = new CultureInfo("en-us");

        static readonly char CanonicalSeparator = '-';

        public static readonly char BittrexSeparator = '-';
        public static readonly char PoloniexSeparator = '_';

        public static string Canonical(string input, char separator, bool reverse = false)
        {
            var pairs = input.Split(separator);

            if (pairs.Length != 2)
                return null;

            if (reverse)
            {
                return join(pairs[1], pairs[0]);
            }
            else
            {
                return join(pairs[0], pairs[1]);
            }

            string join(string a, string b)
            {
                return $"{a.ToUpper(culture)}{CanonicalSeparator}{b.ToUpper(culture)}";
            }
        }

        public static string Canonical(string input)
        {
            return
                Canonical(input, BittrexSeparator) ??
                Canonical(input, PoloniexSeparator);
        }

        public static List<string> Exchanges()
        {
            return new List<string> { Bittrex, Poloniex };
        }
    }
}