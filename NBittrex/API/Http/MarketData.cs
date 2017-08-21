using System;
using System.Collections.Generic;

namespace NBittrex.API.Http
{
    public class ApiCallResponse<T>
    {
        public string Success { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }

    public class GetMarketsResult
    {
        public string Success { get; set; }
        public string Message { get; set; }
        public List<MarketData> Result { get; set; }
    }

    public class MarketData
    {
        public string MarketCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public string MarketCurrencyLong { get; set; }
        public string BaseCurrencyLong { get; set; }
        public string MarketName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }

    public class GetBalancesResponse : List<AccountBalance>
    {
    }
    public class AccountBalance
    {
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal Available { get; set; }
        public decimal Pending { get; set; }
        public string CryptoAddress { get; set; }
        public bool Requested { get; set; }
        public string Uuid { get; set; }
    }

    public class GetMarketHistoryResponse : List<MarketTrade>
    {
    }
    public class MarketTrade
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public FillType FillType { get; set; }
        public OrderType OrderType { get; set; }
    }

    public class GetMarketSummaryResponse
    {
        public string MarketName { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public decimal Last { get; set; }
        public decimal BaseVolume { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public int OpenBuyOrders { get; set; }
        public int OpenSellOrders { get; set; }
        public decimal PrevDay { get; set; }
        public DateTime Created { get; set; }
        public string DisplayMarketName { get; set; }
    }

    public class GetOpenOrdersResponse : List<OpenOrder>
    {
    }
    public class OpenOrder
    {
        public string Uuid { get; set; }
        public string OrderUuid { get; set; }
        public string Exchange { get; set; }
        public OpenOrderType OrderType { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal Limit { get; set; }
        public decimal CommissionPaid { get; set; }
        public decimal Price { get; set; }
        //public decimal? PricePerUnit{get;set;}
        public DateTime Opened { get; set; }
        //public string Closed" : null,
        public bool CancelInitiated { get; set; }
        public bool ImmediateOrCancel { get; set; }
        public bool IsConditional { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }
    }

    public class GetOrderBookResponse
    {
        public List<OrderEntry> Buy { get; set; }
        public List<OrderEntry> Sell { get; set; }
    }
    public class OrderEntry
    {
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }

    public class OrderResponse
    {
        public string Uuid { get; set; }
    }

    public class GetOrderHistoryResponse : List<CompletedOrder>
    {
    }
    public class CompletedOrder
    {
        public string OrderUuid { get; set; }
        public string Exchange { get; set; }
        public DateTime TimeStamp { get; set; }
        public OpenOrderType OrderType { get; set; }
        public decimal Limit { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal Commission { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsConditional { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }
        public bool ImmediateOrCancel { get; set; }
    }
}