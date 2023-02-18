using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet : StocksDataset, IAverageRangeDataSet
    {
        protected int range = 3;
        protected int rangePrevious = 2;
        protected int rangeNext = 1;

        public MovingEnhancedAverageDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public override int Range
        {
            get { return range; }
        }

        public void SetRange(int value)
        {
            range = value;
            rangePrevious = (int)Math.Floor((double)value / 2.0);
            rangeNext = (int)Math.Ceiling((double)value / 2.0);
        }

        /// <summary>
        /// Check that in a for..next cycle from Previous to Next is equal to range
        /// and that pieces are taken aroung a given date (100 in this case)
        /// </summary>
        public List<int> PiecesAround100
        {
            get
            {
                List<int> pieces = new List<int>();
                for (int j = 100 - (rangePrevious); j < 100 + (rangeNext); j++)
                {
                    pieces.Add(j);
                }
                return pieces;
            }
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

        /// <summary>
        /// Returns an array with average centered in the current value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public virtual double[] GetFutureMovingAverage(double[] values, int range)
        {
            SetRange(range);

            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                List<double> pieces = new List<double>();
                //add the previous and future values
                for (int j = i - (range-1); j <= i + (range-1); j++)
                {
                    if ((j >= 0) && (j < values.Length))
                        pieces.Add(values[j]);
                }

                if (pieces.Count == (range*2-1))
                    result[i] = SharpML.Types.Utils.Mean(pieces.ToArray());
                else
                    result[i] = double.NaN;
            }

            return result;
        }

        public double[][] GetMovingEnhancedAverage(double[][] input, int range)
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
    }
}
