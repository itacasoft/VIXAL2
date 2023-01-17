using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public class TradeSimulator2 : BaseTradeSimulator
    {
        public TradeSimulator2(double minTrend) : base(minTrend)
        {
        }

        public List<Trade> Trade(PredictedData predictedData, double money, double commission)
        {
            int trades = 0;
            int i = 0;
            List<Trade> result = new List<Trade>();
            List<DateTime> dates = predictedData.GetDates();

            while (i < predictedData.PredictedStack.Count)
            {
                DatedValue predicted0 = predictedData.PredictedStack[i].Predicted[0];
                DatedValue predicted1;

                if (i < predictedData.PredictedStack.Count - 1)
                {
                    predicted1 = predictedData.PredictedStack[i].Predicted[1];
                    //check if the predicted trend is positive, negative or flat
                    int predictedTrend = GetTrend(predicted0.Value, predicted1.Value);
/*
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

                        i += TradeLenght;
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

                        i += TradeLenght;
                    }
                    else
                    {
                        i++;
                    }
*/
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
