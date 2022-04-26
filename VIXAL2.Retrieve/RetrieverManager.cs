using VIXAL2.Retrieve.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpML.Types;

namespace VIXAL2.Retrieve
{
    internal class RetrieverManager: BaseManager
    {
        public RetrieverManager(string dataFolder, string inputFile, DateTime dateFrom, DateTime dateTo): base(dataFolder)
        {
            this.dateFrom = dateFrom;
            this.dateTo = dateTo;
            this.inputFile = inputFile;
        }

        private DateTime dateFrom;
        private DateTime dateTo;
        private string inputFile;

        public override async Task Run()
        {
            string prefix = "1/3 Downloading...";
            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);

            string[][] stocks = Utils.LoadCsvAsStrings(Path.Combine(dataFolder, inputFile));
            // Count when we update progress bar
            int drawEvery = Utils.PercentIntervalByLength(stocks.Length);

            for (int i = 0; i < stocks.Length; i++)
            {
                switch (stocks[i][0])
                {
                    case "YA":
                        await new Yahoo.DataRetriever(dataFolder).Download(stocks[i][1], dateFrom, dateTo);
                        break;
                    case "AL":
                        await new Alphavantage.DataRetriever(dataFolder).Download(stocks[i][1], dateFrom.Date, dateTo);
                        break;
                    case "SDW":
                        await new SDW.DataRetriever(dataFolder).Download(stocks[i][1], dateFrom, dateTo);
                        break;
                    default:
                        break;
                }

                // Update progress bar
                if (i % drawEvery == 0)
                    Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)i / (stocks.Length) * 100.0), ConsoleColor.Gray);
            }

            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 100), ConsoleColor.Green);
            Console.WriteLine();
        }
    }
}
