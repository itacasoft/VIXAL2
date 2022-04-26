using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace VIXAL2.Data
{
    public class RsiDataSet : StocksDataset
    {
        private int range = 14;

        public RsiDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
            
        }

        public int Range 
        { 
            get { return range; } 
            set { range = value; }
        }

        public override void Prepare()
        {
            this.allData = GetRsi(allData.ToArray(), range);
            RemoveNaNs(allData, dates);

            base.Prepare();
        }

        public override string ClassShortName
        {
            get
            {
                return "RsiDs";
            }
        }


        public static double SingleRsi(double[] closePrices)
        {
            var prices = closePrices;

            double sumProfit = 0;
            double sumLoss = 0;
            for (int i = 1; i < closePrices.Length; i++)
            {
                var difference = closePrices[i] - closePrices[i - 1];
                if (difference >= 0)
                {
                    sumProfit += difference;
                }
                else
                {
                    sumLoss -= difference;
                }
            }

            if (sumProfit == 0) return 0;
            if (Math.Abs(sumLoss) < 0.000001) return 100;

            var relativeStrength = sumProfit / sumLoss;

            return 100.0 - (100.0 / (1 + relativeStrength));
        }

        public static double[] GetRsi(double[] input, int rsi_period)
        {
            double[] result = new double[input.Length];

            List<double> list = new List<double>();
            for (int i=0; i<input.Length; i++)
            {
                list.Add(input[i]);

                if (list.Count < rsi_period)
                {
                    result[i] = double.NaN;
                }
                else if (list.Count == rsi_period)
                {
                    result[i] = SingleRsi(list.ToArray());
                }
                else if (list.Count > rsi_period)
                {
                    list.RemoveAt(0);
                    result[i] = SingleRsi(list.ToArray());
                }
            }

            return result;
        }

        public static List<double[]> GetRsi(double[][] input, int range)
        {
            List<double[]> result = new List<double[]>();
            for (int row = 0; row < input.Length; row++)
            {
                result.Add(new double[input[row].Length]);
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = Utils.GetVectorFromArray(input, col);
                double[] rsi = GetRsi(singleStock, range);

                //apply RSI on data
                for (int row = 0; row < result.Count; row++)
                {
                    result[row][col] = rsi[row];
                }
            }
            return result;
        }
    }
}
