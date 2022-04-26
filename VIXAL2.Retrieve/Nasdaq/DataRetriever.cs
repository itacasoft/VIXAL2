using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve.Nasdaq
{
    internal class DataRetriever: BaseDataRetriever
    {
        public DataRetriever(string dataFolder) : base(dataFolder)
        {
        }

        public override async Task Download(string product, DateTime dateFrom, DateTime dateTo)
        {
            // Basic url
            string url = "https://api.nasdaq.com/api/screener/stocks?tableonly=true&limit=25&offset=0&download=true";
            // Non funziona da programma, mentre funziona da browser
            string targetFile = "NA_" + dateTo.ToString("yyyyMMdd") + "_daily.json";

            try
            {
                // Download if needed
                if (!File.Exists(targetFile))
                {
                    Console.WriteLine("Downloading Nasdaq current prices ...");
                    await Downloader.DownloadFileAsync(url, targetFile, null, null);
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
