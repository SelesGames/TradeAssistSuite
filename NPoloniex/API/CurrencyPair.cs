using System.Globalization;

namespace NPoloniex.API
{
    public class CurrencyPair
    {
        private static readonly CultureInfo culture = new CultureInfo("en-us");

        private const char SeparatorCharacter = '_';

        private string fullTextRep;

        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }

        private CurrencyPair(string stringRep)
        {
            fullTextRep = Clean(stringRep);

            var pairs = stringRep.Split(SeparatorCharacter);
            BaseCurrency = pairs[0];
            QuoteCurrency = pairs[1];

            string Clean(string input)
            {
                return input.ToUpper(culture);
            }
        }

        // User-defined conversion from Digit to double
        public static implicit operator CurrencyPair(string s)
        {
            return new CurrencyPair(s);
        }

        //  User-defined conversion from double to Digit
        public static implicit operator string(CurrencyPair c)
        {
            return c.fullTextRep;
        }

        public CurrencyPair(string baseCurrency, string quoteCurrency)
        {
            fullTextRep = baseCurrency + SeparatorCharacter + quoteCurrency;
            BaseCurrency = baseCurrency;
            QuoteCurrency = quoteCurrency;
        }

        public override string ToString()
        {
            return fullTextRep;
        }

        public static bool operator ==(CurrencyPair a, CurrencyPair b)
        {
            if (ReferenceEquals(a, b)) return true;
            if ((object)a == null ^ (object)b == null) return false;

            return a.fullTextRep == b.fullTextRep;
        }

        public static bool operator !=(CurrencyPair a, CurrencyPair b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var b = obj as CurrencyPair;
            return (object)b != null && Equals(b);
        }

        public bool Equals(CurrencyPair b)
        {
            return b.fullTextRep == fullTextRep;
        }

        public override int GetHashCode()
        {
            return fullTextRep.GetHashCode();
        }

        public static bool IsCurrencyPair(string input)
        {
            return input.Split(SeparatorCharacter).Length == 2;
        }

        public static CurrencyPair PrependBitcoin(string quoteCurrency)
        {
            return new CurrencyPair("BTC", quoteCurrency);
        }
    }
}