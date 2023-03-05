using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class RsiDataSet : StocksDataset, IAverageRangeDataSet
    {
        private int range = 14;

        public RsiDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
            
        }

        public override int Range
        {
            get { return range; }
        }

        public void SetRange(int value)
        {
            range = value;
        }

        public override void Prepare(float trainPercent, float validPercent)
        {
            ReloadFromOriginal();
            this._data = GetRsi(_originalData, range).ToList();
            RemoveNaNs(_data, Dates);

            base.Prepare(trainPercent, validPercent);
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

        public static double[][] GetRsi(double[][] input, int range)
        {
            double[][] result = new double[input.Length][];
            for (int row = 0; row < input.Length; row++)
            {
                result[row] = new double[input[row].Length];
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = SharpML.Types.Utils.GetVectorFromArray(input, col);
                double[] rsi = GetRsi(singleStock, range);

                //apply RSI on data
                for (int row = 0; row < result.Length; row++)
                {
                    result[row][col] = rsi[row];
                }
            }
            return result;
        }
    }
}
