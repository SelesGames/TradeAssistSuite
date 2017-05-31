using System.Collections.Generic;

namespace Jojatekok.PoloniexAPI
{
    static class OrderTypeHelpers
    {
        private static Dictionary<OrderType, string> orderTypeToString = new Dictionary<OrderType, string>
        {
            { OrderType.Buy, "buy" },
            { OrderType.Sell, "sell" }
        };

        private static Dictionary<string, OrderType> stringToOrderType = new Dictionary<string, OrderType>
        {
            { "buy", OrderType.Buy },
            { "sell", OrderType.Sell }
        };

        public static OrderType ToOrderType(string value)
        {
            return stringToOrderType[value];
        }

        public static string ToString(OrderType type)
        {
            return orderTypeToString[type];
        }
    }

    /// <summary>Represents the type of an order.</summary>
    public enum OrderType
    {
        Buy,
        Sell
    }
}
