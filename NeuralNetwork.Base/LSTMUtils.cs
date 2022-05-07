using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public static class LSTMUtils
    {
        public static float Compare(double[] dataY, double[] dataPredicted)
        {
            float result;

            float guessed = 0, failed = 0;
            double predicted0 = dataPredicted[0];
            double future0 = dataY[0];

            for (int row = 1; row < dataY.Length; row++)
            {
                double future1 = dataY[row];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row];
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (predictedPositiveTrend == futurePositiveTrend)
                    guessed++;
                else
                    failed++;

                predicted0 = predicted1;
                future0 = future1;
            }

            result = guessed / (guessed + failed);

            return result;
        }

        public static float Compare2(TimeSerieArray dataY, List<Tuple<DateTime, float>> dataPredicted)
        {
            float result;

            float guessed = 0, failed = 0;
            double predicted0 = dataPredicted[0].Item2;
            double future0 = dataY.Values[0][0];

            for (int row = 1; row < dataPredicted.Count; row++)
            {
                double future1 = dataY.Values[row][0];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row].Item2;
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (predictedPositiveTrend == futurePositiveTrend)
                    guessed++;
                else
                    failed++;

                predicted0 = predicted1;
                future0 = future1;
            }

            result = guessed / (guessed + failed);

            return result;
        }

    }
}
