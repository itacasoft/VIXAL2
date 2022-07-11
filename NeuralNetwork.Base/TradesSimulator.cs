using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public class TradesSimulator
    {
        const int MAX_DAYS_FOR_TRADE = 5;
        const int TRADE_LENGHT = 10;
        const double MIN_TREND = 0.04;

        public static int GetTrend(double value0, double value1)
        {
            if (value0 == value1) return 0;

            if (value1 > value0)
            {
                if (((value1 - value0) / value0) < MIN_TREND)
                    return 0;
                else
                    return 1;
            }
            else
            {
                if (((value0 - value1) / value0) < MIN_TREND)
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

            while (i < dataPredicted.Count && i < MAX_DAYS_FOR_TRADE)
            {
                DoubleDatedValue predicted0 = dataPredicted[i];
                DoubleDatedValue predicted1;
                if (i + TRADE_LENGHT < dataPredicted.Count)
                {
                    predicted1 = dataPredicted[i + TRADE_LENGHT];
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

                        i += TRADE_LENGHT;
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

                        i += TRADE_LENGHT;
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
