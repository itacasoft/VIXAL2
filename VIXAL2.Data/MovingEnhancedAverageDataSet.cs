using SharpML.Types;
using System;
using System.Collections.Generic;

namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet : StocksDataset
    {
        protected int range = 20;

        public MovingEnhancedAverageDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public int Range
        {
            get { return range; }
            set { range = value; }
        }

        public override void Prepare()
        {
            this.Data = GetMovingEnhancedAverage(Data, range);
            RemoveNaNs(dataList, Dates);

            base.Prepare();
        }

        public override string ClassShortName
        {
            get
            {
                return "MovingEnhancedAverageDs";
            }
        }

        public static double[] GetFutureMovingAverage(double[] values, int range)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                List<double> pieces = new List<double>();
                //add the previous and future values
                for (int j = i - (range/2); j < i + (range / 2); j++)
                {
                    if ((j >= 0) && (j<values.Length))
                        pieces.Add(values[j]);
                }

                if (pieces.Count == range)
                    result[i] = Utils.Mean(pieces.ToArray());
                else
                    result[i] = double.NaN;
            }

            return result;
        }

        public static double[][] GetMovingEnhancedAverage(double[][] input, int range)
        {
            double[][] result = new double[input.Length][];
            for (int row = 0; row < input.Length; row++)
            {
                result[row] = new double[input[row].Length];
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = Utils.GetVectorFromArray(input, col);
                double[] movingAverage = GetFutureMovingAverage(singleStock, range);

                //apply moving average on data
                for (int row = 0; row < result.Length; row++)
                {
                    result[row][col] = movingAverage[row];
                }
            }
            return result;
        }
    }
}
