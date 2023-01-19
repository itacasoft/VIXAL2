using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public enum TradingStatus
    {
        None = 0,
        Long = 1,
        Short = 2
    }

    public enum Trend
    {
        EOF = 0,
        Down = 1,
        None = 2,
        Up = 3
    }

    public class TradeSimulator2 : BaseTradeSimulator
    {
        private PredictedData PredictedData;
        private List<FinTrade> trades = new List<FinTrade>();
        private double money = 10000;

        public TradeSimulator2(PredictedData predictedData): base()
        {
            PredictedData = predictedData;
        }

        public Trend GetTrend(int index)
        {
            if (index >= PredictedData.OriginalData.Count-1)
                return Trend.EOF;

            var predictedCurve = PredictedData.GetPredictedCurve(index);

            double value0 = predictedCurve.Predicted[0].Value;
            double value1 = predictedCurve.Predicted[1].Value; ;

            if (value1 == value0) return Trend.None;

            if (value1 > value0)
            {
                if (((value1 - value0) / value0) < MinTrend)
                    return Trend.None;
                else
                    return Trend.Up;
            }
            else
            {
                if (((value0 - value1) / value0) < MinTrend)
                    return Trend.None;
                else
                    return Trend.Down;
            }
        }

        private void OpenPosition(TradingPosition tradingPosition, DateTime startDate, double startPrice, double startMoney)
        {
            FinTrade trade = new FinTrade(startDate, startPrice, startMoney, tradingPosition);
            trades.Add(trade);
        }

        private double ClosePosition(DateTime endDate, double endPrice)
        {
            FinTrade trade = trades[trades.Count-1];

            if (trade.IsOpen)
            {
                trade.Close(endDate, endPrice);
            }
            return trade.EndMoney;
        }

        public void Trade(double money)
        {
            TradingStatus status = TradingStatus.None;

            int i = 0;
            List<Trade> trades = new List<Trade>();
            List<DateTime> dates = PredictedData.GetDates();
            var currentMoney = 10000.00;

            while (i < dates.Count)
            {
                var currentDate = dates[i];
                var currentPrice = PredictedData.OriginalData[i].Value;

                //se sono alla fine, chiudo le posizioni pending
                if (i == dates.Count - 1)
                {
                    currentMoney = ClosePosition(currentDate, currentPrice);
                    break;
                }

                if (status == TradingStatus.Long)
                {
                    //se sono lungo, rilevo il trend, se è in ribasso chiudo la posizione  
                    Trend trend = GetTrend(i);
                    if (trend == Trend.Down)
                        currentMoney = ClosePosition(currentDate, currentPrice);
                }
                else if (status == TradingStatus.Short)
                {
                    //se sono cirto, rilevo il trend, se è in rialzo chiudo la posizione  
                    Trend trend = GetTrend(i);
                    if (trend == Trend.Up)
                        currentMoney = ClosePosition(currentDate, currentPrice);
                }

                if (status == TradingStatus.None)
                {
                    //se non ho pisizioni aperte, verifico il trend
                    //e decido se aprire una posizione long o short
                    Trend trend = GetTrend(i);
                    if (trend == Trend.Up)
                        OpenPosition(TradingPosition.Long, currentDate, currentPrice, currentMoney);
                    else if (trend == Trend.Down)
                        OpenPosition(TradingPosition.Short, currentDate, currentPrice, currentMoney);
                }

                i++;
            }
        }
    }
}
