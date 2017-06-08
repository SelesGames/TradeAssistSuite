using System;

namespace TradeAssistSuite
{
    public struct UserId
    {
        string userId;

        public static implicit operator UserId(string userId)
        {
            var o = new UserId();
            o.userId = userId;
            return o;
        }

        public static implicit operator UserId(Guid userId)
        {
            var o = new UserId();
            o.userId = userId.ToString();
            return o;
        }

        public static implicit operator UserId(int userId)
        {
            var o = new UserId();
            o.userId = userId.ToString();
            return o;
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