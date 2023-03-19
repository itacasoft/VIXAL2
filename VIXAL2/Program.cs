using System;
using System.Linq;
using System.IO;
using VIXAL2.Data.Base;
using System.Collections.Generic;
using System.Configuration;
using VIXAL2.Data;

namespace VIXAL2
{
    internal class Program
    {
        /// <summary>
        /// Numero di iterazioni all'interno di un training
        /// </summary>
        static int trainingIterations;
        /// <summary>
        /// Indica se la reiterazione è richiesta
        /// </summary>
        static bool mustReiterate = false;
        /// <summary>
        /// Numero di giorni di predizione fra X e Y
        /// </summary>
        static int predictDays;
        /// <summary>
        /// Numero di valori presi per le medie
        /// </summary>
        static int range;
        /// <summary>
        /// Tipo di dataset utilizzato per la simulazione
        /// </summary>
        static DataSetType dsType;
        /// <summary>
        /// CSV file containing values for training
        /// </summary>
        static string inputFile;
        /// <summary>
        /// Stock index where to start simulation from
        /// </summary>
        static int startStockIndex;
        /// <summary>
        /// Stock index where to end simulation
        /// </summary>
        static int endStockIndex = 0;
        /// <summary>
        /// Filter by date (from, including)
        /// </summary>
        static string sFromDate = "1900.01.01";
        /// <summary>
        /// Filter by date (to, including)
        /// </summary>
        static string sToDate = "2099.12.31";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentNullException("Null argument passed");
            }

            if (args[0] == "/?")
            {
                DisplayHelp();
                return;
            }

            inputFile = args[0];
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("File " + inputFile + " not found");

            var parameters = GetParameters(args);
            DisplayParameters(parameters);

            var t = DatasetFactory.LoadCsvAsRawData(inputFile, sFromDate, sToDate);
            if (t.StocksCount < endStockIndex)
                throw new ArgumentOutOfRangeException("EdnStockIndex is greater than stocks count");

            Report.Manager.InitialConstructor(trainingIterations, SimulationManager.HiddenLayers, SimulationManager.CellsCount, SimulationManager.BatchSize);
            Report.Manager.SaveParametersToFile(dsType.ToString(), parameters);

            for (int i = startStockIndex; i <= endStockIndex; i++)
            {
                Console.WriteLine("Simulation " + (i-startStockIndex+1).ToString() + " of " + (endStockIndex-startStockIndex+1).ToString() + " starting at " + DateTime.Now.ToShortTimeString() + "...");
                SimulationManager sim = new SimulationManager(inputFile, dsType, predictDays, range, trainingIterations, mustReiterate, sFromDate, sToDate);
                sim.StartTraining(i);
            }

            Report.Manager.PrintOverallReportAsExcel(dsType.ToString());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("VIXAL2 simulation ENDED. That's all folks :)!");
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Calculates trend of stocks using LSTM");
            Console.WriteLine();
            Console.WriteLine("VIXAL2 [filename] /si [stock index] /ty [Dataset type] /it [iterations] /pr [predict days] /ra [range] /fd [from date (including)] /td [to date (including)] /re");
            Console.WriteLine();
        }

        static Dictionary<string, string> GetParameters(string[] args)
        {
            //default
            int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);
            int hiddenLayers = Convert.ToInt32(ConfigurationManager.AppSettings["HiddenLayers"]);
            int cellsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CellsCount"]);

            for (var x = 1; x < args.Count(); x++)
            {
                switch (args[x].Trim().ToLower())
                {
                    case "/si":
                        startStockIndex = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/ei":
                        endStockIndex = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/t":
                    case "/ty":
                        dsType = (DataSetType)Convert.ToInt32(args[x + 1]);
                        break;
                    case "/p":
                    case "/pr":
                        predictDays = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/r":
                    case "/ra":
                        range = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/i":
                    case "/it":
                        trainingIterations = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/fd":
                        sFromDate = args[x + 1];
                        break;
                    case "/td":
                        sToDate = args[x + 1];
                        break;
                    case "/re":
                        mustReiterate = true;
                        break;
                    case "/tc":
                        StocksDataset.MinTestCount = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/hl":
                        hiddenLayers = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/cc":
                        cellsCount = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/bs":
                        batchSize = Convert.ToInt32(args[x + 1]);
                        break;
                }
            }

            if (endStockIndex < startStockIndex) endStockIndex = startStockIndex;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("InputFile", inputFile);
            parameters.Add("StartIndex", startStockIndex.ToString());
            parameters.Add("EndIndex", endStockIndex.ToString());
            parameters.Add("DsType", dsType.ToString());
            parameters.Add("PredictDays", predictDays.ToString());
            parameters.Add("Range", range.ToString());
            parameters.Add("TrainIterations", trainingIterations.ToString());
            parameters.Add("DateFrom", sFromDate);
            parameters.Add("DateTo", sToDate);
            parameters.Add("MustReiterate", mustReiterate.ToString());
            parameters.Add("TestCount", StocksDataset.MinTestCount.ToString());
            parameters.Add("HiddenLayers", hiddenLayers.ToString());
            parameters.Add("CellsCount", cellsCount.ToString());
            parameters.Add("BatchSize", batchSize.ToString());

            SimulationManager.BatchSize = batchSize;
            SimulationManager.HiddenLayers = hiddenLayers;
            SimulationManager.CellsCount = cellsCount;

            return parameters;
        }

        static void DisplayParameters(Dictionary<string, string> parames)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("VIXAL2 started with parameters: ");

            foreach (var v in parames)
            {
                Console.WriteLine("  " + v.Key + " = " + v.Value);
            }
            Console.WriteLine("---------------------");
        }
    }
}
