using System;
using System.Xml.Serialization;

namespace VIXAL2.Data.Base
{
    public enum TradingPosition
    {
        Long = 1,
        Short = 2
    }

    public class FinTrade
    {
        private DateTime _startDate;
        private DateTime _endDate = DateTime.MinValue;
        private double _currentMoney;
        private bool _applyCommissions = true;
        private double _commissions = 0;
        private bool _isOpen;

        public double StartMoney;
        public double EndMoney;
        public double StartPrice;
        public double EndPrice;
        public string StockName;
        public TradingPosition TradingPosition;


        public FinTrade()
        {

        }

        public FinTrade(string stockName, DateTime startDate, double startPrice, double startMoney, TradingPosition tradingPosition, bool applyCommissions = true)
        {
            StockName = stockName;
            TradingPosition = tradingPosition;
            _startDate = startDate;
            StartPrice = startPrice;
            _applyCommissions = applyCommissions;
            _commissions = CalculateCommission(startMoney);
            StartMoney = startMoney;
            _currentMoney = Math.Round(startMoney - _commissions, 2, MidpointRounding.AwayFromZero);
            _isOpen = true;
        }

        public bool GetIsOpen()
        {
            return _isOpen;
        }

        public double GetCurrentMoney()
        {
            return _currentMoney;
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
            if (!_isOpen)
                throw new InvalidOperationException("Trade is already closed");

            _endDate = endDate;
            EndPrice = endPrice;

            if (TradingPosition == TradingPosition.Long)
                EndMoney = EndPrice * _currentMoney/StartPrice;
            else
                EndMoney = StartPrice * _currentMoney / EndPrice;

            var commission = CalculateCommission(EndMoney);

            EndMoney = Math.Round(EndMoney - commission, 2, MidpointRounding.AwayFromZero); 
            _commissions += commission;
            _isOpen = false;
        }


        public double Gain
        {
            get
            {
                double result = EndMoney - StartMoney;
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }
            set
            {
                EndMoney = value + StartMoney;
            }
        }

        public double GainPerc
        {
            get
            {
                double result = (EndMoney - StartMoney)/StartMoney;
                return Math.Round(result, 4, MidpointRounding.AwayFromZero);
            }
            set
            {
                EndMoney = value * StartMoney + StartMoney;
            }
        }

        public double Commissions
        {
            get
            {
                double result = _commissions;
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }
            set
            {
                _commissions = value;
            }
        }

        public string StartDate
        {
            get
            {
                return _startDate.ToShortDateString();
            }
            set
            {
                _startDate = DateTime.Parse(value);
            }
        }

        public string EndDate
        {
            get
            {
                return _endDate.ToShortDateString();
            }
            set
            {
                _endDate = DateTime.Parse(value);
            }
        }
    }
}
