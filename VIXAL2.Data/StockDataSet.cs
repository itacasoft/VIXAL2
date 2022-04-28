using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class StocksDataset : TimeSerieDataSet
    {
        List<double> predictionResults;
        TimeSerieArray predicted;

        public StocksDataset(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) :
            base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
            predictionResults = new List<double>();

            if (allData.Length != dates.Length)
                throw new ArgumentException("Date lenght is not the same of Data lenght");

            if (allData[0].Length != stockNames.Length)
                throw new ArgumentException("Names lenght is not the same of DataRow lenght");

            if (predictCount > allData[0].Length)
                throw new ArgumentException("Columns to predict is larger than input columns");
        }

        public string[] StockNames
        {
            get
            {
                return colNames;
            }
        }

        public override string ClassShortName
        {
            get
            {
                return "StockDs";
            }
        }



        public double GetPredictionResult(int col)
        {
            return predictionResults[col];
        }

        public void PredictedSetValue(int row, int col, DateTime date, double value)
        {
            predicted.SetValue(row, col, date, value);
        }

        public double PredictedGetValue(int row, int col)
        {
            return predicted.Values[row][col];
        }

        public double PredictedGetValue(DateTime date, int col)
        {
            return predicted.GetValue(date, col);
        }

        public void PredictedAlloc(int rows)
        {
            predicted = new TimeSerieArray(rows, testCount);// Training[0].Steps[0].TargetOutput.Rows);
        }

        public void PredictedDecode()
        {
            predicted.DecodeValues(normalizer);
        }

        public TimeSerieArray PredictedGetVector(int col)
        {
            TimeSerieArray result = new TimeSerieArray(predicted.Length, 1);

            for (int row = 0; row < result.Length; row++)
            {
                result.SetValue(row, 0, predicted.GetDate(row), predicted.Values[row][col]);
            }
            return result;
        }

        public virtual double[] CompareAgainstPredicted(TimeSerieArray future)
        {
            for (int col = 0; col < Math.Min(columnsToPredict, StockNames.Length); col++)
            {
                double guessed = 0, failed = 0;
                double predicted0 = predicted.Values[0][col];
                double future0 = future.GetValue(predicted.GetDate(0));

                for (int row = 1; row < future.Length; row++)
                {
                    //get the date
                    DateTime mydate = future.GetDate(row);
                    //get the real value
                    double future1 = future.Values[row][col];
                    bool futurePositiveTrend = (future1 > future0);

                    double predicted1 = predicted.GetValue(mydate, col);
                    bool predictedPositiveTrend = (predicted1 > predicted0);

                    if (predictedPositiveTrend == futurePositiveTrend)
                        guessed++;
                    else
                        failed++;

                    predicted0 = predicted1;
                    future0 = future1;
                }

                predictionResults.Add(guessed / (guessed + failed));
            }

            return predictionResults.ToArray();
        }
    }
}
