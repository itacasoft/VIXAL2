namespace VIXAL2.Data.Base
{
    public class Trade
    {
        public int StartIndex;
        public int EndIndex;
        public double StartMoney;
        public double EndMoney;
        public int Trend = 0;

        public Trade(int trend)
        {
            Trend = trend;
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
