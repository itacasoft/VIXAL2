using SharpML.Types;
using System;
using System.Collections.Generic;

namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet2: MovingEnhancedAverageDataSet
    {
        public MovingEnhancedAverageDataSet2(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public MovingEnhancedAverageDataSet2(string stockName, DateTime[] dates, double[] singleStockData) : this(Utils.StringToStrings(stockName), dates, Utils.VectorToArray(singleStockData), 0, 1)
        {
        }

        public override string ClassShortName
        {
            get
            {
                return "MovAvgEnh2";
            }
        }

        /// <summary>
        /// Returns an array with average centered in the current value
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override double[] GetFutureMovingAverage(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                List<double> pieces = new List<double>();
                //add the previous and future values
                for (int j = i - (rangePrevious); j < i + (rangeNext); j++)
                {
                    if ((j >= 0) && (j < values.Length))
                        pieces.Add(values[j]);
                }

                result[i] = SharpML.Types.Utils.Mean(pieces.ToArray());
            }

            return result;
        }
    }
}



