using SharpML.Types;
using System;
using System.Collections.Generic;

namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet : StocksDataset
    {
        protected int range = 30;

        public MovingEnhancedAverageDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int predictCount) : base(stockNames, dates, allData, predictCount)
        {
        }

        public int Range
        {
            get { return range; }
            set { range = value; }
        }

        public override void Prepare()
        {
            this.allData = GetMovingEnhancedAverage(allData.ToArray(), range);
            RemoveNaNs(allData, dates);

            base.Prepare();
        }

        public override string ClassShortName
        {
            get
            {
                return "MovingEnhancedAverageDs";
            }
        }

        public static double[] GetMovingEnhancedAverage(double[] values, int range)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                List<double> pieces = new List<double>();
                //add the previous values
                for (int j = i - (range - 1); j < i; j++)
                {
                    if (j >= 0)
                        pieces.Add(values[j]);
                }
                //add the current value
                pieces.Add(values[i]);

                if (pieces.Count == range)
                    result[i] = Utils.Mean(pieces.ToArray());
                else
                    result[i] = double.NaN;
            }

            return result;
        }

        public static List<double[]> GetMovingEnhancedAverage(double[][] input, int range)
        {
            List<double[]> result = new List<double[]>();
            for (int row = 0; row < input.Length; row++)
            {
                result.Add(new double[input[row].Length]);
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = Utils.GetVectorFromArray(input, col);
                double[] movingAverage = GetMovingEnhancedAverage(singleStock, range);

                //apply moving average on data
                for (int row = 0; row < result.Count; row++)
                {
                    result[row][col] = movingAverage[row];
                }
            }
            return result;
        }
    }
}
