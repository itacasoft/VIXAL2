namespace NeuralNetwork.Base
{
    public abstract class BaseTradeSimulator
    {
        protected double MinTrend = 0.02;

        public BaseTradeSimulator(double minTrend)
        {
            MinTrend = minTrend;
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
