using SharpML.Types;
using System;
using System.Collections.Generic;

namespace VIXAL2.Data
{
    public class MovingAverageDataSet : StocksDataset
    {
        protected int range = 20;

        public MovingAverageDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public int Range
        {
            get { return range; }
            set { range = value; }
        }

        public override void Prepare()
        {
            this.allData = GetMovingAverage(allData.ToArray(), range);
            RemoveNaNs(allData, dates);

            base.Prepare();
        }

        public override string ClassShortName
        {
            get
            {
                return "MovingAverageDs";
            }
        }

        public static double[] GetMovingAverage(double[] values, int range)
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

        public static List<double[]> GetMovingAverage(double[][] input, int range)
        {
            List<double[]> result = new List<double[]>();
            for (int row = 0; row < input.Length; row++)
            {
                result.Add(new double[input[row].Length]);
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = Utils.GetVectorFromArray(input, col);
                double[] movingAverage = GetMovingAverage(singleStock, range);

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
