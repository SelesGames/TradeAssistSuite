using System;
using System.Globalization;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

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

    public class Authenticator
    {
        static readonly Encoding Encoding = Encoding.ASCII;
        static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        readonly HMACSHA512 encryptor;
        readonly string publicKey, privateKey;

        public Authenticator(string publicKey, string privateKey)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
            encryptor = new HMACSHA512();
            encryptor.Key = Encoding.GetBytes(privateKey);
        }

        public void SignRequest(HttpRequestMessage request)
        {

        }

        public void SignRequest(HttpRequestMessage request, byte[] contentBytes)
        {
            request.Headers.Add("Key", publicKey);

            var signedContent = encryptor.ComputeHash(contentBytes);
            var signedContentHex = ToStringHex(signedContent);
            request.Headers.Add("Sign", signedContentHex);
        }

        static string ToStringHex(byte[] value)
        {
            var output = string.Empty;
            for (var i = 0; i < value.Length; i++)
            {
                output += value[i].ToString("x2", InvariantCulture);
            }

            return (output);
        }
    }
}