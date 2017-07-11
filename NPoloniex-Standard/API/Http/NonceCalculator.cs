using System;
using System.Globalization;
using System.Numerics;

namespace NPoloniex.API.Http
{
    public static class NonceCalculator
    {
        static readonly DateTime DateTimeUnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        static BigInteger currentHttpPostNonce;

        public static string GetCurrentHttpPostNonce()
        {
            var newHttpPostNonce = new BigInteger(Math.Round(DateTime.UtcNow.Subtract(DateTimeUnixEpochStart).TotalMilliseconds * 1000, MidpointRounding.AwayFromZero));
            if (newHttpPostNonce > currentHttpPostNonce)
            {
                currentHttpPostNonce = newHttpPostNonce;
            }
            else
            {
                currentHttpPostNonce += 1;
            }

            return currentHttpPostNonce.ToString(InvariantCulture);
        }

        internal static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
        {
            return DateTimeUnixEpochStart.AddSeconds(unixTimeStamp);
        }

        internal static ulong DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (ulong)Math.Floor(dateTime.Subtract(DateTimeUnixEpochStart).TotalSeconds);
        }

        internal static DateTime ParseDateTime(string dateTime)
        {
            return DateTime.SpecifyKind(DateTime.ParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", InvariantCulture), DateTimeKind.Utc).ToLocalTime();
        }
    }
}