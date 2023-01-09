using System;
using System.Collections.Generic;
using SharpML.Types;
using Accord.Math;
using Accord.Statistics;
using System.Data;
using System.Linq;
using SharpML.Types.Normalization;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public static class DatasetFactory
    {
        private static void ProcessData(string[][] stocks, out List<string> stockNames, out List<DateTime> stockDates, out double[][] stocksData)
        {
            //first row is header (symbols), exluding the first field (date)
            stockNames = new List<string>();
            for (int i = 1; i < stocks[0].Length; i++)
            {
                stockNames.Add(stocks[0][i]);
            }

            stockDates = new List<DateTime>();

            stocksData = new double[stocks.Length - 1][];
            for (int row = 1; row < stocks.Length; row++)
            {
                stocksData[row - 1] = new double[stocks[row].Length - 1]; //first column is dates

                DateTime stockDate;
                if (!DateTime.TryParse(stocks[row][0], out stockDate))
                    throw new Exception(stocks[row][0] + " at row " + row.ToString() + " does not represent a valid date");
                stockDates.Add(stockDate);

                for (int col = 1; col < stocks[row].Length; col++)
                {
                    if (!double.TryParse(stocks[row][col], out stocksData[row - 1][col - 1]))
                    {
                        throw new InvalidCastException("Field at row " + (row).ToString() + ", col " + col.ToString() + " is not a valid double value");
                    }
                }
            }
        }


        private static Tuple<List<string>, List<DateTime>, double[][]> LoadCsvAsTuple(string inputCsv)
        {
            string[][] stocks = SharpML.Types.Utils.LoadCsvAsStrings(inputCsv);

            List<string> stockNames;
            List<DateTime> stockDates;
            double[][] stocksData;
            ProcessData(stocks, out stockNames, out stockDates, out stocksData);

            FillNaNs(stocksData);

            Tuple<List<string>, List<DateTime>, double[][]> result = Tuple.Create(stockNames, stockDates, stocksData);
            return result;
        }

        public static StocksDataset CreateDataset(string inputCsv, int firstColumnToPredict, int predictCount, DataSetType dataSetType)
        {
            var t = LoadCsvAsTuple(inputCsv);

            //check if config is correct
            if (predictCount > t.Item3.Length)
                throw new ArgumentException("PredictCount cannot be larger than input columns");

            StocksDataset ds;
            if (dataSetType == DataSetType.MovingAverage)
            {
                ds = new MovingAverageDataSet(t.Item1.ToArray(), t.Item2.ToArray(), t.Item3, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.RSI)
            {
                ds = new RsiDataSet(t.Item1.ToArray(), t.Item2.ToArray(), t.Item3, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.Enh_MovingAverage)
            {
                ds = new MovingEnhancedAverageDataSet(t.Item1.ToArray(), t.Item2.ToArray(), t.Item3, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.Enh2_MovingAverage)
            {
                ds = new MovingEnhancedAverageDataSet2(t.Item1.ToArray(), t.Item2.ToArray(), t.Item3, firstColumnToPredict, predictCount);
            }
            else
            {
                ds = new StocksDataset(t.Item1.ToArray(), t.Item2.ToArray(), t.Item3, firstColumnToPredict, predictCount);
            }
            return ds;
        }

        private static void FillNaNs(double[][] data)
        {
            string prefix = "Filling NaNs...";
            SharpML.Types.Utils.DrawMessage(prefix, SharpML.Types.Utils.CreateProgressBar(SharpML.Types.Utils.ProgressBarLength, 0), ConsoleColor.Gray);

            int drawEvery = SharpML.Types.Utils.PercentIntervalByLength(data[0].Length);

            Random currentRandom = new Random(Normalizer.Instance.Random.Next());

            // Loop through all columns
            int row = 0;
            for (int col = 0; col < data[0].Length; ++col)
            {
                // Najdeme si statisticke vlastnosti
                double[] d = data.Select(x => x[col]).ToArray();
                double[] diff = new double[d.Length - 1];
                for (int i = 1; i < d.Length; ++i)
                    diff[i - 1] = d[i] - d[i - 1];
                diff = diff.Where(x => !double.IsNaN(x)).ToArray();
                double mean = diff.Mean();
                double stddev = diff.StandardDeviation(mean);
                if (diff.Length <= 1)
                {
                    mean = 0;
                    stddev = 1;
                }

                // Nahodny generator
                Accord.Math.Random.GaussianGenerator gen = new Accord.Math.Random.GaussianGenerator((float)mean, (float)stddev, currentRandom.Next());

                // Doplnime zacatek
                for (row = 0; row < data.Length; ++row)
                    if (!double.IsNaN(data[row][col]))
                    {
                        for (row = row - 1; row >= 0; --row)
                            data[row][col] = data[row + 1][col] + gen.Next();
                        break;
                    }

                // Doplnime konec
                for (row = data.Length - 1; row >= 0; --row)
                    if (!double.IsNaN(data[row][col]))
                    {
                        for (row = row + 1; row < data.Length; ++row)
                            data[row][col] = data[row - 1][col] + gen.Next();
                        break;
                    }

                // Doplnime mezery
                for (row = 0; row < data.Length; ++row)
                {
                    if (double.IsNaN(data[row][col]))
                    {
                        int start = row - 1;
                        for (row = row + 1; row < data.Length; ++row)
                            if (!double.IsNaN(data[row][col]))
                            {
                                int end = row;

                                // Doplnime od start do end
                                for (int i = start + 1; i < end; ++i)
                                {
                                    // Najdeme hodnotu pri aproximaci
                                    double aprox = data[start][col] + (data[end][col] - data[start][col]) / (end - start) * (i - start);

                                    data[i][col] = aprox + gen.Next();
                                }

                                break;
                            }
                    }
                }

                // Pro jistotu rozkopirovani
                for (row = 1; row < data.Length; ++row)
                    if (double.IsNaN(data[row][col]))
                        data[row][col] = data[row - 1][col];
                for (row = data.Length - 2; row >= 0; --row)
                    if (double.IsNaN(data[row][col]))
                        data[row][col] = data[row + 1][col];

                if (col % drawEvery == 0)
                    SharpML.Types.Utils.DrawMessage(prefix, SharpML.Types.Utils.CreateProgressBar(SharpML.Types.Utils.ProgressBarLength, (double)col / data[0].Length * 100.0), ConsoleColor.Gray);
            }

            SharpML.Types.Utils.DrawMessage(prefix, SharpML.Types.Utils.CreateProgressBar(SharpML.Types.Utils.ProgressBarLength, 100), ConsoleColor.Green);
            Console.WriteLine();
        }
    }
}
