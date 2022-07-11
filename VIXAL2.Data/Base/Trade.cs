using System;

namespace VIXAL2.Data.Base
{
    public class Trade
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public double StartMoney;
        public double EndMoney;
        public double StartPrice;
        public double EndPrice;
        public int PredictedTrend = 0;

        public Trade(DateTime startDate, double startPrice, DateTime endDate, double endPrice, int predictedTrend)
        {
            PredictedTrend = predictedTrend;
            StartDate = startDate;
            EndDate = endDate;
            StartPrice = startPrice;
            EndPrice = endPrice;
        }

        public double Gain
        {
            get
            {
                return EndMoney - StartMoney;
            }
        }

        public double GainPerc
        {
            get
            {
                return (EndMoney - StartMoney)/StartMoney;
            }
        }
    }
}
