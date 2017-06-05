using NPoloniex.API.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using static TradeAssistSuite.BitcoinMath;

namespace TradeAssistSuite
{
    public static class PositionCalculator
    {
        public static PositionHistory ExtractPositions(decimal currentBalance, decimal currentPrice, IEnumerable<Trade> trades)
        {
            decimal size = 0m - currentBalance;
            bool hasOpenPosition = currentBalance > 0m;

            var positionHistory = new PositionHistory();

            var positionSensor = new List<Trade>();

            foreach (var trade in trades)
            {
                positionSensor.Add(trade);

                if (trade.Type == OrderType.Buy)
                {
                    var fee = Round(trade.Fee * trade.Amount);
                    size += trade.Amount - fee;
                }

                else if (trade.Type == OrderType.Sell)
                {
                    size -= trade.Amount;
                }

                if (Math.Round(size, 4) == 0)
                {
                    if (hasOpenPosition)
                    {
                        hasOpenPosition = false;
                        var openPosition = CalculateOpenPosition(currentBalance, currentPrice, positionSensor);
                        positionHistory.Open = openPosition;
                    }
                    else
                    {
                        var closedPosition = CalculateClosedPosition(positionSensor);
                        positionHistory.Closed.Add(closedPosition);
                    }
                    positionSensor.Clear();
                }
            }

            return positionHistory;
        }




        static ClosedPosition CalculateClosedPosition(List<Trade> trades)
        {
            decimal
                numberOfPurchasedUnits = 0m,
                totalPurchasePrice = 0m,
                numberOfSoldUnits = 0m,
                totalSalePrice = 0m;

            foreach (var trade in trades)
            {
                if (trade.Type == OrderType.Buy)
                {
                    // Poloniex-specific calculation.  Poloniex takes the fee out of the number of units you get for a purchase.  So as an example, if you purchase 1BTC of a coin, you will only spend 1BTC but you will get less than 1BTC worth of that coin
                    var tradeTotalFee = Round(trade.Fee * trade.Amount);
                    numberOfPurchasedUnits += trade.Amount - tradeTotalFee;
                    totalPurchasePrice += trade.Total;
                }
                else if (trade.Type == OrderType.Sell)
                {
                    // Poloniex-specific calculation.  Poloniex reduces the amount of BTC received from the purchase by the fee percentage
                    numberOfSoldUnits += trade.Amount;
                    var tradeTotalFee = Round(trade.Fee * trade.Total);
                    totalSalePrice += trade.Total - tradeTotalFee;
                }
            }

            var averagePurchasePrice = Divide(totalPurchasePrice, numberOfPurchasedUnits);
            var averageSalePrice = Divide(totalSalePrice, numberOfSoldUnits);

            var profit = totalSalePrice - totalPurchasePrice;

            var firstTrade = trades.Last();
            var lastTrade = trades.First();

            return new ClosedPosition
            {
                Began = firstTrade.Date,
                Closed = lastTrade.Date,
                FirstTrade = firstTrade,
                LastTrade = lastTrade,
                AveragePurchasePrice = averagePurchasePrice,
                AverageSalePrice = averageSalePrice,
                Size = numberOfPurchasedUnits,
                Profit = profit,
            };
        }




        static OpenPosition CalculateOpenPosition(decimal currentBalance, decimal currentPrice, List<Trade> trades)
        {
            decimal
                numberOfPurchasedUnits = 0m,
                totalPurchasePrice = 0m,
                numberOfSoldUnits = 0m,
                totalSalePrice = 0m;

            foreach (var trade in trades)
            {
                if (trade.Type == OrderType.Buy)
                {
                    // Poloniex-specific calculation.  Poloniex takes the fee out of the number of units you get for a purchase.  So as an example, if you purchase 1BTC of a coin, you will only spend 1BTC but you will get less than 1BTC worth of that coin
                    var tradeTotalFee = Round(trade.Fee * trade.Amount);
                    numberOfPurchasedUnits += trade.Amount - tradeTotalFee;
                    totalPurchasePrice += trade.Total;
                }
                else if (trade.Type == OrderType.Sell)
                {
                    // Poloniex-specific calculation.  Poloniex reduces the amount of BTC received from the purchase by the fee percentage
                    numberOfSoldUnits += trade.Amount;
                    var tradeTotalFee = Round(trade.Fee * trade.Total);
                    totalSalePrice += trade.Total - tradeTotalFee;
                }
            }

            var averagePurchasePrice = Divide(totalPurchasePrice, numberOfPurchasedUnits);
            var averageSalePrice = Divide(totalSalePrice, numberOfSoldUnits);

            // calculate unrealized profit for units the user still holds
            //var calculatedBalance = numberOfPurchasedUnits - numberOfSoldUnits;
            var priceDifferential = currentPrice - averagePurchasePrice;
            var unrealizedProfit = Round(priceDifferential * currentBalance);
            //var costBasisForUnitsKept = averagePurchasePrice * currentBalance;
            //var currentValueOfBalance = currentPrice * currentBalance;
            //var unrealizedProfit = currentValueOfBalance - costBasisForUnitsKept;

            // calculate realized profit for units already sold
            var costBasisForUnitsSold = Round(averagePurchasePrice * numberOfSoldUnits);
            var realizedProfit = totalSalePrice - costBasisForUnitsSold;

            var currentTotalProfit = realizedProfit + unrealizedProfit;

            var firstTrade = trades.Last();

            return new OpenPosition
            {
                Began = firstTrade.Date,
                FirstTrade = firstTrade,
                AveragePurchasePrice = Math.Round(averagePurchasePrice, 8),
                AverageSalePrice = Math.Round(averageSalePrice, 8),
                Size = numberOfPurchasedUnits,
                CurrentTotalProfit = currentTotalProfit,
                CurrentBalance = currentBalance,
                RealizedProfit = realizedProfit,
                UnrealizedProfit = unrealizedProfit,
                LastPrice = currentPrice,
            };
        }
    }
}