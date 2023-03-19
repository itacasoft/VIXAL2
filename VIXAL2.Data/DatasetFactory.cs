using System;
using System.Collections.Generic;
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
        private static RawStocksData InternalLoadArrayAsRawData(string[][] rawData)
        {
            //first row is header (symbols), exluding the first field (date)
            RawStocksData result = new RawStocksData(rawData.Length - 1);

            for (int i = 1; i < rawData[0].Length; i++)
            {
                result.stockNames.Add(rawData[0][i]);
            }

            for (int row = 1; row < rawData.Length; row++)
            {
                result.stocksData[row - 1] = new double[rawData[row].Length - 1]; //first column is dates

                DateTime stockDate;
                if (!DateTime.TryParse(rawData[row][0], out stockDate))
                    throw new Exception(rawData[row][0] + " at row " + row.ToString() + " does not represent a valid date");
                result.stockDates.Add(stockDate);

                for (int col = 1; col < rawData[row].Length; col++)
                {
                    if (!double.TryParse(rawData[row][col], out result.stocksData[row - 1][col - 1]))
                    {
                        throw new InvalidCastException("Field at row " + (row).ToString() + ", col " + col.ToString() + " is not a valid double value");
                    }
                }
            }

            return result;
        }

        public static Tuple<int, int> DatesToIndexes(DateTime dataDa, DateTime dataA, List<DateTime> dates)
        {
            int indexDa = -1;
            int indexA = -1;
            for (int i = 0; i < dates.Count; i++)
            {
                if (dates[i] >= dataDa)
                {
                    indexDa = i;
                    break;
                }
            }

            for (int i = dates.Count - 1; i >= 0; i--)
            {
                if (dates[i] <= dataA)
                {
                    indexA = i;
                    break;
                }
            }

            return new Tuple<int, int>(indexDa, indexA);
        }

        public static RawStocksData LoadArrayAsRawData(string[][] stocksData, string sDataDa = "1900.01.01", string sDataA = "2099.12.31")
        {
            var result = InternalLoadArrayAsRawData(stocksData);
            FillNaNs(result.stocksData);

            bool filter = (sDataDa != "1900.01.01") || (sDataA != "2099.12.31");

            if (filter)
            {
                DateTime dataDa, dataA;
                DateTime.TryParse(sDataDa, out dataDa);
                DateTime.TryParse(sDataA, out dataA);

                var t = DatesToIndexes(dataDa, dataA, result.stockDates);

                //Filter by indexes
                result.stocksData = result.stocksData.Skip(t.Item1).Take(t.Item2 - t.Item1 + 1).ToArray();
                result.stockDates = result.stockDates.Skip(t.Item1).Take(t.Item2 - t.Item1 + 1).ToList();
            }

            return result;
        }

        public static RawStocksData LoadCsvAsRawData(string inputCsv, string sDataDa = "1900.01.01", string sDataA = "2099.12.31")
        {
            string[][] stocksData = SharpML.Types.Utils.LoadCsvAsStrings(inputCsv);
            return LoadArrayAsRawData(stocksData, sDataDa, sDataA);
        }

        public static StocksDataset CreateDataset(string inputCsv, int firstColumnToPredict, int predictCount, DataSetType dataSetType, string sDataDa = "1900.01.01", string sDataA = "2099.12.31")
        {
            var t = LoadCsvAsRawData(inputCsv, sDataDa, sDataA);
            return CreateDataset(t, firstColumnToPredict, predictCount, dataSetType);
        }

        public static StocksDataset CreateDataset(RawStocksData inputData, int firstColumnToPredict, int predictCount, DataSetType dataSetType)
        {
            //check if config is correct
            if (predictCount > inputData.stocksData.Length)
                throw new ArgumentException("PredictCount cannot be larger than input columns");

            StocksDataset ds;
            if (dataSetType == DataSetType.MovingAverage)
            {
                ds = new MovingAverageDataSet(inputData.stockNames.ToArray(), inputData.stockDates.ToArray(), inputData.stocksData, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.RSI)
            {
                ds = new RsiDataSet(inputData.stockNames.ToArray(), inputData.stockDates.ToArray(), inputData.stocksData, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.Enh_MovingAverage)
            {
                ds = new MovingEnhancedAverageDataSet(inputData.stockNames.ToArray(), inputData.stockDates.ToArray(), inputData.stocksData, firstColumnToPredict, predictCount);
            }
            else if (dataSetType == DataSetType.Enh2_MovingAverage)
            {
                ds = new MovingEnhancedAverageDataSet2(inputData.stockNames.ToArray(), inputData.stockDates.ToArray(), inputData.stocksData, firstColumnToPredict, predictCount);
            }
            else
            {
                ds = new StocksDataset(inputData.stockNames.ToArray(), inputData.stockDates.ToArray(), inputData.stocksData, firstColumnToPredict, predictCount);
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
