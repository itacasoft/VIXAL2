using SharpML.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class MovingAverageDataSet : StocksDataset, IAverageRangeDataSet
    {
        protected int range = 20;

        public MovingAverageDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
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

        public override void Prepare()
        {
            this.Data = GetMovingAverage(Data, range);
            RemoveNaNs(dataList, Dates);

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
                    result[i] = SharpML.Types.Utils.Mean(pieces.ToArray());
                else
                    result[i] = double.NaN;
            }

            return result;
        }

        public static double[][] GetMovingAverage(double[][] input, int range)
        {
            double[][] result = new double[input.Length][];
            for (int row = 0; row < input.Length; row++)
            {
                result[row] = new double[input[row].Length];
            }

            for (int col = 0; col < input[0].Length; col++)
            {
                double[] singleStock = SharpML.Types.Utils.GetVectorFromArray(input, col);
                double[] movingAverage = GetMovingAverage(singleStock, range);

                //apply moving average on data
                for (int row = 0; row < result.Length; row++)
                {
                    result[row][col] = movingAverage[row];
                }
            }
            return result;
        }

        public List<DatedValue> GetReverseMovingAverageValues(List<DoubleDatedValue> avgValues)
        {
            var avgValues1 = new List<DatedValue>();
            for (int i=0; i<avgValues.Count; i++)
            {
                avgValues1.Add(avgValues[i]);       
            }

            return GetReverseMovingAverageValues(avgValues1);
        }

        /// <summary>
        /// Restituisce il valore normale data la sua media mobile
        /// </summary>
        /// <param name="date">data del valore</param>
        /// <param name="value">valore della media mobile</param>
        /// <returns></returns>
        public List<DatedValue> GetReverseMovingAverageValues(List<DatedValue> movingAvgValues)
        {
            List<DatedValue> result = new List<DatedValue>();

            DateTime firstDateAvg = movingAvgValues[0].Date;
            DateTime lastDateOriginal = this.OriginalData.MaxDate;

            List<double> originalValues;

            //il calcolo funziona solo se avgValues si sovrappone o è contiguo con i dati originali
            if (Utils.AddBusinessDays(lastDateOriginal, 1) >= firstDateAvg)
            {
                var beforeFirstDateAvg = this.OriginalData.GetPreviousDate(firstDateAvg);
                originalValues = this.OriginalData.GetPreviousValuesFromColumnIncludingCurrent(beforeFirstDateAvg.Value, this.range - 1, FirstColumnToPredict).ToList<double>();
                //aggiusta i valori per rimuovere lo scalino
                var delta = movingAvgValues[0].Value - Utils.Mean(originalValues);

                for (int i=0; i<originalValues.Count; i++)
                {
                    originalValues[i] = originalValues[i] + delta;
                }
            }
            else 
                throw new Exception("Impossible to calculate ReverseMoving from not contiguous arrays");

            for (int i=0; i< movingAvgValues.Count; i++)
            {
                double resultItem = movingAvgValues[i].Value * range - originalValues.Sum(x => x);
                originalValues.Add(resultItem);
                originalValues.RemoveAt(0);
                result.Add(new DatedValue(movingAvgValues[i].Date, resultItem));
            }

            return result;
        }
    }
}
