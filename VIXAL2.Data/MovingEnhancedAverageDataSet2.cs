using SharpML.Types;
using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;


namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet2: StocksDataset
    {
        protected int halfRange = 3;

        public MovingEnhancedAverageDataSet2(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public void SetHalfRange(int value)
        {
            halfRange = value;
        }

        public override void Prepare()
        {
            this.Data = GetMovingEnhancedAverage(Data, halfRange);
            RemoveNaNs(dataList, Dates);

            base.Prepare();
        }

        public override string ClassShortName
        {
            get
            {
                return "MovingEnhancedAverageDs2";
            }
        }

        /// <summary>
        /// Returns an array with average centered in the current value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static double[] GetFutureMovingAverage(double[] values, int halfrange)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                List<double> pieces = new List<double>();
                //add the previous and future values
                for (int j = i - (halfrange - 1); j <= i + (halfrange - 1); j++)
                {
                    if ((j >= 0) && (j < values.Length))
                        pieces.Add(values[j]);
                }

                result[i] = SharpML.Types.Utils.Mean(pieces.ToArray());
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
                double[] singleStock = SharpML.Types.Utils.GetVectorFromArray(input, col);
                double[] movingAverage = GetFutureMovingAverage(singleStock, range);

                //apply moving average on data
                for (int row = 0; row < result.Length; row++)
                {
                    result[row][col] = movingAverage[row];
                }
            }
            return result;
        }

        public int HalfRange
        {
            get { return halfRange; }
        }
    }
}



