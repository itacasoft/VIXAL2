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

    public class FinTradeSimulator : BaseTradeSimulator
    {
        private PredictedData PredictedData;
        private List<FinTrade> trades = new List<FinTrade>();
        private bool _applyCommissions;

        public FinTradeSimulator(PredictedData predictedData, bool applyCommissions = true): base()
        {
            PredictedData = predictedData;
            _applyCommissions = applyCommissions;
        }

        public TradingStatus GetTradingStatus()
        {
            if (trades.Count == 0)
                return TradingStatus.None;

            if (trades[trades.Count - 1].TradingPosition == TradingPosition.Long && trades[trades.Count - 1].GetIsOpen())
                return TradingStatus.Long;
            else if (trades[trades.Count - 1].TradingPosition == TradingPosition.Short && trades[trades.Count - 1].GetIsOpen())
                return TradingStatus.Short;
            else
                return TradingStatus.None;
        }

        /// <summary>
        /// Restituisce un valore che indica se il trend è ascendente, discendente o neutro
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Trend GetPredictedTrend(int index)
        {
            if (index >= PredictedData.OriginalData.Count-1)
                return Trend.EOF;

            var predictedCurve = PredictedData.GetPredictedCurve(index);

            double value0 = predictedCurve.Predicted[0].Value;
            double value1 = predictedCurve.Predicted[1].Value; 

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

        /// <summary>
        /// Apre una posizione di trading
        /// </summary>
        /// <param name="tradingPosition">Può essere long (acquisto) o short (vendita allo scoperto)</param>
        /// <param name="startDate"></param>
        /// <param name="startPrice"></param>
        /// <param name="startMoney"></param>
        private void OpenPosition(TradingPosition tradingPosition, DateTime startDate, double startPrice, double startMoney)
        {
            FinTrade trade = new FinTrade(PredictedData.StockName, startDate, startPrice, startMoney, tradingPosition, _applyCommissions);
            trades.Add(trade);
        }

        /// <summary>
        /// Chiude l'ultima posizione eventualmente aperta
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="endPrice"></param>
        /// <returns>Restituisce il denaro della chiusura della posizione</returns>
        private double ClosePosition(DateTime endDate, double endPrice)
        {
            FinTrade trade = trades[trades.Count-1];

            if (trade.GetIsOpen())
            {
                trade.Close(endDate, endPrice);
            }
            return trade.EndMoney;
        }

        /// <summary>
        /// Effettua una serie di operazioni di trade su tutto il periodo considerato
        /// </summary>
        /// <param name="money">Denaro da utilizzare per le operazioni</param>
        /// <returns>Denaro al termine delle operazioni</returns>
        public double Trade(double money)
        {
            int i = 0;
            List<DateTime> dates = PredictedData.GetDates();
            var currentMoney = money;

            while (i < dates.Count)
            {
                var currentDate = dates[i];
                var currentPrice = PredictedData.OriginalData[i].Value;
                var status = GetTradingStatus();

                //se sono alla fine, chiudo le posizioni pending
                if (i == dates.Count - 1)
                {
                    if (trades.Count > 0) 
                        currentMoney = ClosePosition(currentDate, currentPrice);
                    break;
                }

                if (status == TradingStatus.Long)
                {
                    //se sono lungo, rilevo il trend, se è in ribasso chiudo la posizione  
                    Trend trend = GetPredictedTrend(i);
                    if (trend == Trend.Down)
                        currentMoney = ClosePosition(currentDate, currentPrice);
                }
                else if (status == TradingStatus.Short)
                {
                    //se sono cirto, rilevo il trend, se è in rialzo chiudo la posizione  
                    Trend trend = GetPredictedTrend(i);
                    if (trend == Trend.Up)
                        currentMoney = ClosePosition(currentDate, currentPrice);
                }

                status = GetTradingStatus();

                if (status == TradingStatus.None)
                {
                    //se non ho pisizioni aperte, verifico il trend
                    //e decido se aprire una posizione long o short
                    Trend trend = GetPredictedTrend(i);
                    if (trend == Trend.Up)
                        OpenPosition(TradingPosition.Long, currentDate, currentPrice, currentMoney);
                    else if (trend == Trend.Down)
                        OpenPosition(TradingPosition.Short, currentDate, currentPrice, currentMoney);
                }

                i++;
            }

            return currentMoney;
        }

        public int TradesCount
        {
            get
            {
                return trades.Count;
            }
        }

        public int TradesGainCount
        {
            get
            {
                int result = 0;

                for (int i = 0; i < trades.Count; i++)
                {
                    if (trades[i].Gain > 0)
                        result++;
                }
                return result;
            }
        }

        public int TradesLossCount
        {
            get
            {
                int result = 0;

                for (int i = 0; i < trades.Count; i++)
                {
                    if (trades[i].Gain <= 0)
                        result++;
                }
                return result;
            }
        }

        public List<FinTrade> Trades
        {
            get
            {
                return trades;
            }
        }
    }
}
