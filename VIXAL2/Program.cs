using System;
using System.Linq;
using System.Configuration;
using System.IO;
using VIXAL2.Data.Base;

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

            for (var x = 1; x < args.Count(); x++)
            {
                switch (args[x].Trim().ToLower())
                {
                    case "/s":
                    case "/si":
                        startStockIndex = Convert.ToInt32(args[x+1]);
                        break;
                    case "/e":
                    case "/ei":
                        endStockIndex = Convert.ToInt32(args[x + 1]);
                        break;
                    case "/t":
                        dsType = (DataSetType)Convert.ToInt32(args[x+1]);
                        break;
                    case "/p":
                        predictDays = Convert.ToInt32(args[x+1]); 
                        break;
                    case "/r":
                        range = Convert.ToInt32(args[x+1]);
                        break;
                    case "/i":
                        trainingIterations = Convert.ToInt32(args[x+1]);
                        break;
                    case "/re":
                        mustReiterate = true; 
                        break;
                }
            }

            if (endStockIndex < startStockIndex) endStockIndex = startStockIndex;

            SimulationManager.BatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);
            SimulationManager.HiddenLayers = Convert.ToInt32(ConfigurationManager.AppSettings["HiddenLayers"]);
            SimulationManager.CellsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CellsCount"]);

            ReportManager.InitialConstructor(trainingIterations, SimulationManager.HiddenLayers, SimulationManager.CellsCount, SimulationManager.BatchSize);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("VIXAL2 started with parameters: ");
            Console.Write("StartIndex = " + startStockIndex + ", ");
            Console.Write("EndIndex = " + endStockIndex + ", ");
            Console.Write("DsType = " + dsType + ", ");
            Console.Write("PredictDays = " + predictDays + ", ");
            Console.Write("Range = " + range + ", ");
            Console.Write("TrainIterations = " + trainingIterations + ", ");
            Console.WriteLine("Mustreiterate = " + mustReiterate);
            Console.WriteLine("---------------------");

            for (int i = startStockIndex; i <= endStockIndex; i++)
            {
                Console.WriteLine("Simulation " + (i-startStockIndex+1).ToString() + " of " + (endStockIndex-startStockIndex+1).ToString() + " about to start at " + DateTime.Now.ToShortTimeString());
                SimulationManager sim = new SimulationManager(inputFile, dsType, predictDays, range, trainingIterations, mustReiterate);
                sim.StartTraining(i);
            }

            ReportManager.PrintOverallReport(dsType.ToString());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("VIXAL2 simulation ENDED. That's all folks :)!");
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Calculates trend of stocks using LSTM");
            Console.WriteLine();
            Console.WriteLine("VIXAL2 [filename] /s [stock index] /t [Dataset type] /i [iterations] /p [predict days] /r [range] /re");
            Console.WriteLine();
        }

    }
}
