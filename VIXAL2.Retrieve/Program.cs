using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Globalization;

namespace VIXAL2.Retrieve
{
    internal class Program
    {
        static async Task MainAsync(DateTime dateTo)
        {
            //read appSettings
            string dataFolder = ConfigurationManager.AppSettings["DataFolder"];
            if (!Directory.Exists(dataFolder))
                dataFolder = ".";

            string timeFrameYears = ConfigurationManager.AppSettings["TimeFrameYears"];
            int years = 0;
            if (!int.TryParse(timeFrameYears, out years))
                throw new ArgumentException("Invalide <appSettings> parameter \"TimeFrameYears\"");

            string inputCsvFile = ConfigurationManager.AppSettings["InputCsvFile"];
            if (!File.Exists(Path.Combine(dataFolder, inputCsvFile)))
                throw new FileNotFoundException("File " + Path.Combine(dataFolder, inputCsvFile) + " not found");

            string outputCsvFile = ConfigurationManager.AppSettings["OutputCsvFile"];

            DateTime dateFrom = dateTo.AddYears(-(years));

            RetrieverManager retriever = new RetrieverManager(dataFolder, inputCsvFile, dateFrom, dateTo);
            await retriever.Run();

            ParserManager parser = new ParserManager(dataFolder, dateTo);
            await parser.Run();

            DataSetManager dataMan = new DataSetManager(dataFolder, outputCsvFile, dateTo);
            dataMan.Stocks = parser.Stocks;
            await dataMan.Run();
        }

        static void Main(string[] args)
        {
            //get dateTo from args
            DateTime dateTo = DateTime.Today;
            if (args.Length > 0)
            {
                try
                {
                    dateTo = Convert.ToDateTime(args[0], CultureInfo.InvariantCulture);
                }
                catch
                {
                    dateTo = DateTime.ParseExact(args[0], "yyyyMMdd", CultureInfo.InvariantCulture);
                }
            }

            //add 24 hours so that it takes also prices of current day
            dateTo = dateTo.AddHours(23.59);
            MainAsync(dateTo).Wait();
        }
    }
}
