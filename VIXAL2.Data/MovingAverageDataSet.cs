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

        public override void Prepare(float trainPercent, float validPercent)
        {
            this.Data = GetMovingAverage(Data, range);
            RemoveNaNs(dataList, Dates);

            base.Prepare(trainPercent, validPercent);
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

        public List<DatedValue> GetReverseMovingAverageValues(List<DoubleDatedValue> avgValues, bool adjustValues = false)
        {
            var avgValues1 = new List<DatedValue>();
            for (int i=0; i<avgValues.Count; i++)
            {
                avgValues1.Add(new DatedValue(avgValues[i].PredictionDate, avgValues[i].Value));
            }

            return GetReverseMovingAverageValues(avgValues1, adjustValues);
        }

        /// <summary>
        /// Restituisce il valore normale data la sua media mobile
        /// </summary>
        /// <param name="date">data del valore</param>
        /// <param name="value">valore della media mobile</param>
        /// <returns></returns>
        public List<DatedValue> GetReverseMovingAverageValues(List<DatedValue> movingAvgValues, bool adjustValues = false)
        {
            List<DatedValue> result = new List<DatedValue>();

            DateTime firstDateAvg = movingAvgValues[0].Date;
            DateTime lastDateOriginal = this.OriginalData.MaxDate;

            List<double> originalValues = new List<double>();
            List<DateTime> originalDates = new List<DateTime>();

            //il calcolo funziona solo se avgValues si sovrappone o è contiguo con i dati originali
            if (Utils.AddBusinessDays(lastDateOriginal, 1) >= firstDateAvg)
            {
                for (int i=range-1; i>0; i--)
                {
                    var myDateOrig = this.OriginalData.GetPreviousDate(firstDateAvg, i);
                    var myValue = this.OriginalData.GetValue(myDateOrig.Value, FirstColumnToPredict);

                    originalValues.Add(myValue);
                    originalDates.Add(myDateOrig.Value);
                }

                //aggiusta i valori per rimuovere lo scalino
                if (adjustValues)
                {
                    var delta = movingAvgValues[0].Value - Utils.Mean(originalValues);

                    for (int i = 0; i < originalValues.Count; i++)
                    {
                        originalValues[i] = originalValues[i] + delta;
                    }
                }
            }
            else 
                throw new Exception("Impossible to calculate ReverseMoving from not contiguous arrays");

            var reverse = MovingAverageDataSet.GetReverseMovingAverage(DatedValue.TakeValues(movingAvgValues), this.range, originalValues);

            for (int i=0; i<originalDates.Count; i++)
            {
                result.Add(new DatedValue(originalDates[i], reverse[i]));
            }

            for (int i=0; i< movingAvgValues.Count; i++)
            {
                result.Add(new DatedValue(movingAvgValues[i].Date, reverse[i+range-1]));
            }

            return result;
        }

        public static double[] GetReverseMovingAverage(IEnumerable<double> movingAvgValues, int range, IEnumerable<double> firstOriginalValues)
        {
            List<double> result = new List<double>();

            //verifico che la lunghezza dell'original sia sufficiente
            if (firstOriginalValues.Count() < range - 1)
                throw new Exception("Impossible to calculate ReverseMoving from not consistent arrays");

            var wrong = movingAvgValues.Where(x => double.IsNaN(x)).ToArray();
            if (wrong.Length > 0)
                throw new Exception("Impossible to calculate ReverseMoving from NaN values");

            List<double> originalValues = firstOriginalValues.Take(range-1).ToList();
            result.AddRange(firstOriginalValues);

            foreach  (var v in movingAvgValues)
            {
                double resultItem = v * range - originalValues.Sum(x => x);
                originalValues.Add(resultItem);
                originalValues.RemoveAt(0);
                result.Add(resultItem);
            }

            return result.ToArray();
        }
    }
}
