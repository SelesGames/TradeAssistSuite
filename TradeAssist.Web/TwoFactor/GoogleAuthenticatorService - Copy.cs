using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Web.TwoFactor
{
    public class GoogleAuthenticatorService2<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
    {
        static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return manager.GetTwoFactorEnabledAsync(user);
        }

        public string GenerateSecret()
        {
            byte[] buffer = new byte[9];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            return Convert.ToBase64String(buffer).Substring(0, 10).Replace('/', '0').Replace('+', '1');
        }

        public string GetCode(string secret, long counter)
        {
            return GeneratePassword(secret, counter);
        }

        public string GetCode(string secret)
        {
            return GetCode(secret, GetCurrentCounter());
        }

        public long GetCurrentCounter()
        {
            return GetCurrentCounter(DateTime.UtcNow, UNIX_EPOCH, 30);
        }

        public bool IsValid(string secret, string code, int checkAdjacentIntervals = 1)
        {
            if (code == GetCode(secret))
                return true;

            for (int i = 1; i <= checkAdjacentIntervals; i++)
            {
                if (code == GetCode(secret, GetCurrentCounter() + i))
                    return true;

                if (code == GetCode(secret, GetCurrentCounter() - i))
                    return true;
            }

            return false;
        }

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            return IsValid(await manager.GetSecurityStampAsync(user), token);
        }

        string GeneratePassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);

            byte[] key = Encoding.ASCII.GetBytes(secret);
            byte[] hash;

            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                hash = hmac.ComputeHash(counter);
            }

            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, digits);

            return password.ToString(new string('0', digits));
        }

        long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
        {
            return (long)(now - epoch).TotalSeconds / timeStep;
        }
    }
}
