using SharpML.Types;
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

        public static Tuple<float,float,float> Compare(TimeSerieArray dataY, int IndexColumnToPredict, List<DatedValueF> dataPredicted)
        {
            float guessed = 0, failed = 0;
            double predicted0 = dataPredicted[0].Value;
            double future0 = dataY.Values[0][IndexColumnToPredict];

            for (int row = 1; row < dataPredicted.Count; row++)
            {
                double future1 = dataY.Values[row][IndexColumnToPredict];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row].Value;
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (predictedPositiveTrend == futurePositiveTrend)
                    guessed++;
                else
                    failed++;

                predicted0 = predicted1;
                future0 = future1;
            }

            float result = guessed / (guessed + failed);

            return Tuple.Create<float, float, float>(guessed, guessed+failed, result);
        }

        public static float Trade(StocksDataset ds, int IndexColumnToPredict, List<DatedValueF> dataPredicted)
        {
            float guessed = 0, failed = 0;
            DatedValueF predicted0 = dataPredicted[0];

            for (int row = 1; row < dataPredicted.Count; row++)
            {
                //assess if the predicted trend is positive or not
                var predicted1 = dataPredicted[row];
                bool predictedPositiveTrend = (predicted1.Value > predicted0.Value);

                double valueToday = ds.OriginalData.GetValue(predicted0.Date, IndexColumnToPredict);
                double valueTomorrow = ds.OriginalData.GetValue(predicted1.Date, IndexColumnToPredict);


                if (predictedPositiveTrend == true)
                    guessed++;
                else
                    failed++;

                predicted0 = predicted1;
                //                future0 = future1;
            }

            float result = guessed / (guessed + failed);

            return result;
        }
    }
}
