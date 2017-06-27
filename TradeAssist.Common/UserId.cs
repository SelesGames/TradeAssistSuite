using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace TradeAssist
{
    public struct UserId
    {
        string userId;

        [JsonConstructor]
        private UserId(string userId)
        {
            this.userId = userId;
        }

        public static implicit operator UserId(string userId)
        {
            return new UserId(userId);
        }

        public static implicit operator UserId(Guid userId)
        {
            return new UserId(userId.ToString());
        }

        public static implicit operator UserId(int userId)
        {
            return new UserId(userId.ToString());
        }

        public static implicit operator string(UserId userId)
        {
            return userId.ToString();
        }

        public override string ToString()
        {
            return userId;
        }

        public static bool operator ==(UserId a, UserId b)
        {
            //if (ReferenceEquals(a, b)) return true;
            //if ((object)a == null ^ (object)b == null) return false;

            return a.userId == b.userId;
        }

        public static bool operator !=(UserId a, UserId b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is UserId)
            {
                var b = (UserId)obj;
                return Equals(b);
            }
            return false;
        }

        public bool Equals(UserId b)
        {
            return b.userId == userId;
        }

        public override int GetHashCode()
        {
            return userId.GetHashCode();
        }
    }
}