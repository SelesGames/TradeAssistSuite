using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace NPoloniex.API.Http
{
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