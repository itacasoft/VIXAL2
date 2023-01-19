namespace NeuralNetwork.Base
{
    public abstract class BaseTradeSimulator
    {
        const double DEFAULT_MIN_TREND = 0.02;

        private double minTrend = DEFAULT_MIN_TREND;

        public BaseTradeSimulator()
        {

        }

        public double MinTrend
        {
            get { return minTrend; }
            set { minTrend = value; }
        }

        public int GetTrend(double value0, double value1)
        {
            if (value0 == value1) return 0;

            if (value1 > value0)
            {
                if (((value1 - value0) / value0) < MinTrend)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (((value0 - value1) / value0) < MinTrend)
                    return 0;
                else
                    return -1;
            }
        }

    }
}
