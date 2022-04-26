using VIXAL2.Retrieve.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve
{
    internal class DataSetManager: BaseManager
    {
        public DataSetManager(string dataFolder, string fullDataSet, DateTime dateTo) : base(dataFolder)
        {
            this.dateTo = dateTo;
            this.fileName = fullDataSet;
            this.dataFolder = dataFolder;
        }

        private string fileName;
        private DateTime dateTo;

        public StockList Stocks { get; set; }

        public void ExportCSV()
        {
        }
        public static void ExportCSV(double[][] data, string filename)
        {
            File.WriteAllLines(filename, data.Select(x => string.Join(";", x)));
        }

        public override async Task Run()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";

            string prefix = "3/3 Export FullDataSet...";
            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);

            // For content
            using (StreamWriter sw = new StreamWriter(Path.Combine(dataFolder, fileName)))
            {
                try
                {
                    // Headers
                    sw.WriteLine("Date;" + string.Join(";", Stocks.GetStockSymbols()));

                    List<DateTime> dates = Stocks.Dates;
                    int drawEvery = Utils.PercentIntervalByLength(dates.Count);

                    for (int i = 0; i < dates.Count; ++i)
                    {
                        List<string> fields = new List<string>();
                        fields.Add(dates[i].ToString("yyyy.MM.dd"));

                        for (int j = 0; j < Stocks.Count; j++)
                        {
                            double aPrice = Stocks[j].Prices[dates[i]];
                            if (aPrice == double.NaN)
                            {
                                fields.Add("NaN");
                            }
                            else
                                fields.Add(aPrice.ToString(nfi));
                        }
                        sw.WriteLine(string.Join(";", fields));

                        // Update progress bar
                        if (i % drawEvery == 0)
                            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)i / dates.Count * 100.0), ConsoleColor.Gray);
                    }
                } 
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message, ConsoleColor.Red);
                }
            }

            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 100), ConsoleColor.Green);
            Console.WriteLine();
        }
    }
}
