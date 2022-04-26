using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve.Alphavantage
{
    internal class DataRetriever: BaseDataRetriever
    {
        public DataRetriever(string dataFolder) : base(dataFolder)
        {
        }

        public override async Task Download(string symbol, DateTime dateFrom, DateTime dateTo)
        {
            // Basic url
            string url = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=___&apikey=WOY3MVXCUFYHFU70";

            // Download
            try
            {
                // Prepare url
                string productUrl = url.Replace("___", symbol);

                // Target file
                string targetFile = "AV_" + dateTo.ToString("yyyyMMdd") + "_" + symbol + "_daily.json";

                // Download if needed
                if (!File.Exists(targetFile))
                {
                    await Downloader.DownloadFileAsync(productUrl, targetFile, null, null);
                    System.Threading.Thread.Sleep(12000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }

    }
}
