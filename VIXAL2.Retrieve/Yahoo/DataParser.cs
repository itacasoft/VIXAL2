using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SharpML.Types;

namespace VIXAL2.Retrieve.Yahoo
{
    internal class DataParser : BaseDataParser
    {
        public DataParser(string dataFolder, DateTime dateTo) : base(dataFolder)
        {
            this.dateTo = dateTo;
        }

        private DateTime dateTo;

        public override StockList Parse()
        {
            string prefix = "2/3 Parsing Yahoo...";
            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);
            StockList result = new StockList();
            string fileName = "";
            int row = 0;

            try
            {
                List<string> filles = new List<string>();
                DirectoryInfo di = new DirectoryInfo(dataFolder);
                FileInfo[] fiArray = di.GetFiles("YA_" + dateTo.ToString("yyyyMMdd") + "*");
                Array.Sort(fiArray, (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.CreationTime, y.CreationTime));
                foreach (FileInfo fi in fiArray)
                {
                    filles.Add(fi.FullName);
                }

                //parsing of yahoo files
                //string[] files = Directory.GetFiles(dataFolder, "YA_" + dateTo.ToString("yyyyMMdd") + "*");
                string[] files = filles.ToArray();
                // Count when we update progress bar
                int drawEvery = Utils.PercentIntervalByLength(files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    fileName = Path.GetFileName(files[i]);
                    string[] pieces = fileName.Split('_');
                    string source = pieces[0];
                    string sdate = pieces[1];
                    string symbol = pieces[2];

                    Stock stock = new Stock(source, symbol, sdate);

                    // Load data
                    string[] lines = File.ReadAllLines(files[i]);

                    // Check header
                    if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                        throw new Exception("Unknown file header");

                    // Parse historical prices
                    for (row = 1; row < lines.Length; ++row)
                    {
                        string[] parts = lines[row].Split(',');

                        DateTime priceDate = DateTime.ParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if (!stock.Prices.ContainsKey(priceDate))
                        {
                            double priceValue = double.NaN;

                            double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out priceValue);
                            if (priceValue == 0) priceValue = double.NaN;

                            stock.Prices.Add(priceDate, priceValue);
                        }
                    }

                    result.Add(stock);

                    // Update progress bar
                    if (i % drawEvery == 0)
                        Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)i / files.Length * 100.0), ConsoleColor.Gray);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Utils.DrawMessage("", ex.Message + " at file " + fileName + ", row " + (row+1).ToString(), ConsoleColor.Red);
                Console.WriteLine();

                System.Environment.Exit(1);
            }

            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 100), ConsoleColor.Green);
            Console.WriteLine();

            return result;
        }
    }
}
