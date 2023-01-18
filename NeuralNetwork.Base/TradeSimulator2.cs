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

        public TradeSimulator2(PredictedData predictedData, double minTrend) : base(minTrend)
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

        public List<Trade> Trade(double money, double commission)
        {
            TradingStatus status = TradingStatus.None;

            int trades = 0;
            int i = 0;
            List<Trade> result = new List<Trade>();
            List<DateTime> dates = PredictedData.GetDates();

            while (i < PredictedData.PredictedStack.Count)
            {
                if (status == TradingStatus.None)
                {
                    //se non sono in trading, verifico il trend e decido se acquistare o
                    //vendere allo scoperto

                    Trend trend = GetTrend(i);
                }
                else
                {
                    i++;
                }
            }
            return result;
        }
    }
}
