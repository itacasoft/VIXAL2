using System;
using System.Collections.Generic;
using VIXAL2.Data;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public static class LSTMUtils
    {
        public static void Compare(double[] dataY, double[] dataPredicted, ref List<Performance> performances)
        {
            double predicted0 = dataPredicted[0];
            double future0 = dataY[0];
            if (performances.Count == 0) performances.Add(new Performance());

            for (int row = 1; row < dataY.Length; row++)
            {
                double future1 = dataY[row];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row];
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (performances.Count <= row) performances.Add(new Performance());

                if (predictedPositiveTrend == futurePositiveTrend)
                {
                    performances[row].Guessed++;
                }
                else
                {
                    performances[row].Failed++;
                }

                predicted0 = predicted1;
                future0 = future1;
            }
        }

        public static Tuple<float, float, float> Compare(TimeSerieArray dataY, int IndexColumnToPredict, List<DatedValue> dataPredicted)
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

        public static int GetTrend(double value0, double value1)
        {
            if (value0 == value1) return 0;

            if (value1 > value0)
            {
                if (((value1 - value0) / value0) < 0.05)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (((value0 - value1) / value0) < 0.05)
                    return 0;
                else
                    return -1;
            }
        }


        public static List<Trade> Trade(TimeSerieArray originalData, int IndexColumnToPredict, List<DoubleDatedValue> dataPredicted, double money, double commission)
        {
            int trades = 0;
            int i = 0;
            List<Trade> result = new List<Trade>();

            while (i < dataPredicted.Count)
            {
                DoubleDatedValue predicted0 = dataPredicted[i];
                DoubleDatedValue predicted1;
                if (i + 10 < dataPredicted.Count)
                {
                    predicted1 = dataPredicted[i+10];
                    //check if the predicted trend is positive, negative or flat
                    int predictedTrend = GetTrend(predicted0.Value, predicted1.Value);

                    double value0 = originalData.GetValue(predicted0.Date, IndexColumnToPredict);
                    double value1 = originalData.GetValue(predicted1.Date, IndexColumnToPredict);

                    if (predictedTrend == 1)
                    {
                        //compro
                        var t = new Trade(predicted0.Date, value0, predicted1.Date, value1, 1);
                        trades++;
                        t.StartMoney = money;
                        t.EndMoney = (value1 * t.StartMoney) / value0;
                        //subtract commissions
                        t.EndMoney = t.EndMoney - commission * t.StartMoney - commission * t.EndMoney;
                        money = t.EndMoney;
                        result.Add(t);

                        i += 10;
                    }
                    else if (predictedTrend == -1)
                    {
                        //vendo allo scoperto
                        var t = new Trade(predicted0.Date, value0, predicted1.Date, value1, -1);
                        t.StartMoney = money;
                        t.EndMoney = (value0 * t.StartMoney) / value1;
                        //subtract commissions
                        t.EndMoney = t.EndMoney - commission * t.StartMoney - commission * t.EndMoney;
                        money = t.EndMoney;
                        result.Add(t);

                        i += 10;
                    }
                    else
                    {
                        i++;
                    }
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
