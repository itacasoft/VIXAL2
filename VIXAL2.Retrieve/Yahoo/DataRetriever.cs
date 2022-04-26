using System;
using System.IO;
using System.Threading.Tasks;
using SharpML.Types;

namespace VIXAL2.Retrieve.Yahoo
{
    internal class DataRetriever: BaseDataRetriever
    {
        public DataRetriever(string dataFolder) : base(dataFolder)
        {
        }

        public override async Task Download(string product, DateTime dateFrom, DateTime dateTo)
        {
            long period1 = Utils.ToUnixTimestamp(dateFrom);
            long period2 = Utils.ToUnixTimestamp(dateTo);
            // Basic url
            string url = "https://query1.finance.yahoo.com/v7/finance/download/___?period1=" + period1.ToString() + "&period2=" + period2.ToString() + "&interval=1d&events=history&includeAdjustedClose=true";
            //es: https://query1.finance.yahoo.com/v7/finance/download/MSFT?period1=1614280300&period2=1645816300&interval=1d&events=history&includeAdjustedClose=true

            // Prepare url
            string productUrl = url.Replace("___", product);

            // Target file
            string targetFile = Path.Combine(dataFolder, "YA_" + dateTo.ToString("yyyyMMdd") + "_" + product + "_daily.csv");

            try
            {
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
                File.Delete(targetFile);
            }
        }
    }
}
