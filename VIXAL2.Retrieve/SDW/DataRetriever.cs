using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve.SDW
{
    internal class DataRetriever: BaseDataRetriever
    {
        public DataRetriever(string dataFolder) : base(dataFolder)
        {
        }

        public override async Task Download(string product, DateTime dateFrom, DateTime dateTo)
        {
            // Basic url
            string url = "https://sdw.ecb.europa.eu/quickviewexport.do?SERIES_KEY=143.FM.M.U2.EUR.RT.MM.EURIBOR6MD_.HSTA&type=csv";

            // Download
            try
            {
                // Target file
                string targetFile = Path.Combine(dataFolder, "SDW_" + dateTo.ToString("yyyyMM") + "_EURIBOR6MD_monthly.csv");

                // Download if needed
                if (!File.Exists(targetFile))
                {
                    Console.WriteLine("Downloading SDW Euribor ...");
                    await Downloader.DownloadFileAsync(url, targetFile, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ConsoleColor.Red);
            }
        }
    }
}
