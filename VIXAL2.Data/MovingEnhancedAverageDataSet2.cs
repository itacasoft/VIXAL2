using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class MovingEnhancedAverageDataSet2: MovingEnhancedAverageDataSet
    {
        public MovingEnhancedAverageDataSet2(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
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
        public override double[] GetFutureMovingAverage(double[] values, int range)
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

                result[i] = SharpML.Types.Utils.Mean(pieces.ToArray());
            }

            return result;
        }
    }
}



