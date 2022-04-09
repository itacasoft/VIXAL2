using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Accord.Math;
using Accord.Statistics;

namespace SharpML.Types
{
    public enum PriceSource
    {
        Undefined,
        Yahoo,
        NasDaq,
        SDW,
        Alphavantage
    }

    public class Stock
    {
        public Stock(string source, string symbol, string downloadDate)
        {
            this.Source = FromString(source);
            this.Symbol = symbol;
            this.DownloadDate = DateTime.ParseExact(downloadDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            Prices = new Dictionary<DateTime, double>();
        }

        public Stock(PriceSource source, string symbol, DateTime downloadDate)
        {
            this.Source = source;
            this.Symbol = symbol;
            this.DownloadDate = downloadDate;

            Prices = new Dictionary<DateTime, double>();
        }

        public static PriceSource FromString(string source)
        {
            PriceSource result = PriceSource.Undefined;
            switch (source)
            {
                case "YA":
                    result = PriceSource.Yahoo;
                    break;
                case "AL":
                    result = PriceSource.Alphavantage;
                    break;
                case "SDW":
                    result = PriceSource.SDW;
                    break;
                case "NAS":
                    result = PriceSource.NasDaq;
                    break;
            }
            return result;
        }

        public string Symbol;
        public PriceSource Source;
        public DateTime DownloadDate;

        public Dictionary<DateTime, double> Prices;

        public override string ToString()
        {
            return Symbol;
        }
    }

    public class StockList : List<Stock>
    {
        public DateTime MinDate
        {
            get
            {
                DateTime result = DateTime.MaxValue;
                foreach (Stock stock in this)
                {
                    foreach (KeyValuePair<DateTime, double> price in stock.Prices)
                    {
                        if (price.Key < result)
                            result = price.Key;
                    }
                }
                return result;
            }
        }

        public DateTime MaxDate
        {
            get
            {
                DateTime result = DateTime.MinValue;
                foreach (Stock stock in this)
                {
                    foreach (KeyValuePair<DateTime, double> price in stock.Prices)
                    {
                        if (price.Key > result)
                            result = price.Key;
                    }
                }
                return result;
            }
        }

        public List<DateTime> Dates
        {
            get
            {
                List<DateTime> result = new List<DateTime>();
                foreach (Stock stock in this)
                {
                    foreach (KeyValuePair<DateTime, double> price in stock.Prices)
                    {
                        if (!result.Contains(price.Key))
                            result.Add(price.Key);
                    }
                }
                result.Sort();
                return result;
            }
        }

        public void RemoveDatesInExcess(DateTime date)
        {
            foreach (Stock stock in this)
            {
                stock.Prices.Remove(date);
            }
        }

        public List<string> GetStockSymbols()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < this.Count; i++)
            {
                result.Add(this[i].Symbol);
            }
            return result;
        }


        public void FillEmptyDates(DateTime date)
        {
            DateTime minDate = this.MinDate;
            foreach (Stock stock in this)
            {
                if (!stock.Prices.ContainsKey(date))
                {
                    bool filled = false;
                    DateTime previousDate = date.AddDays(-1);

                    while ((!filled) && (date > minDate))
                    {
                        if (stock.Prices.ContainsKey(previousDate))
                        {
                            //add a price with value of that date
                            stock.Prices.Add(date, stock.Prices[previousDate]);
                            filled = true;
                        }
                        else
                            previousDate = previousDate.AddDays(-1);
                    }

                    if (!filled)
                        stock.Prices.Add(date, double.NaN);
                }
            }
        }

        public void AlignDates()
        {
            List<DateTime> dates = this.Dates;
            //            string prefix = "2/3 Aligning dates...";
            //            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);
            //            int drawEvery = Utils.PercentIntervalByLength(dates.Count);

            for (int i = 0; i < dates.Count; i++)
            {
                DateTime date = dates[i];
                int isPresent = 0;
                int isNotPresent = 0;

                foreach (Stock stock in this)
                {
                    if (stock.Prices.ContainsKey(date))
                    {
                        isPresent++;
                    }
                    else
                    {
                        isNotPresent++;
                    }
                }

                string s = Convert.ToString(date, CultureInfo.InvariantCulture);

                if (isNotPresent == 0)
                {
                    //ok, niente da fare
                }
                else if (isNotPresent < isPresent)
                {
                    this.FillEmptyDates(date);
                }
                else
                {
                    this.RemoveDatesInExcess(date);
                }

                // Update progress bar
                //                if (i % drawEvery == 0)
                //                    Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)i / (dates.Count) * 100.0), ConsoleColor.Gray);
            }
        }

        public static void FillNaNs(double[][] data)
        {
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
            }
        }
    }
}
