using System;

namespace VIXAL2.Data.Base
{
    public enum TradingPosition
    {
        Long = 1,
        Short = 2
    }

    public class FinTrade
    {
        public DateTime StartDate;
        public DateTime EndDate = DateTime.MinValue;
        public double StartMoney;
        public double CurrentMoney;
        public double EndMoney;
        public double StartPrice;
        public double EndPrice;
        public TradingPosition TradingPosition;
        public bool IsOpen;
        private bool _applyCommissions = true;
        private double _commissions = 0;

        public FinTrade(DateTime startDate, double startPrice, double startMoney, TradingPosition tradingPosition, bool applyCommissions = true)
        {
            TradingPosition = tradingPosition;
            StartDate = startDate;
            StartPrice = startPrice;
            _applyCommissions = applyCommissions;
            _commissions = CalculateCommission(startMoney);
            StartMoney = startMoney;
            CurrentMoney = Math.Round(startMoney - _commissions, 2, MidpointRounding.AwayFromZero);
            IsOpen = true;
        }

        private double CalculateCommission(double money)
        {
            if (!_applyCommissions) return 0;

            double result = 0.0019 * money;

            if (result < 2.95) result = 2.95F;
            if (result > 19) result = 19F;
            return result;
        }

        public void Close(DateTime endDate, double endPrice)
        {
            if (!IsOpen)
                throw new InvalidOperationException("Trade is already closed");

            EndDate = endDate;
            EndPrice = endPrice;

            if (TradingPosition == TradingPosition.Long)
                EndMoney = EndPrice * CurrentMoney/StartPrice;
            else
                EndMoney = StartPrice * CurrentMoney / EndPrice;

            var commission = CalculateCommission(EndMoney);

            EndMoney = Math.Round(EndMoney - commission, 2, MidpointRounding.AwayFromZero); 
            _commissions += commission;
            IsOpen = false;
        }


        public double Gain
        {
            get
            {
                double result = EndMoney - StartMoney;
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }
        }

        public double GainPerc
        {
            get
            {
                double result = (EndMoney - StartMoney)/StartMoney;
                return Math.Round(result, 4, MidpointRounding.AwayFromZero);
            }
        }

        public double Commissions
        {
            get
            {
                double result = _commissions;
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}
