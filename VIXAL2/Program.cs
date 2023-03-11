using NeuralNetwork.Base;
using SharpML.Types;
using System;
using System.Linq;
using System.Configuration;
using System.IO;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using System.Collections.Generic;
using VIXAL2.Data.Report;

namespace VIXAL2
{
    internal class Program
    {
        static LSTMOrchestrator orchestrator;
        static string prefixTraining = "Training...";
        static string prefixSimulating = "Simulating...";
        /// <summary>
        /// Numero di iterazioni all'interno di un training
        /// </summary>
        static int trainingIterations;
        /// <summary>
        /// Indica se la reiterazione è richiesta
        /// </summary>
        static bool mustReiterate = false;
        /// <summary>
        /// Numero di re-iterazioni totali
        /// </summary>
        static int Reiterations;
        /// <summary>
        /// Numero di giorni di predizione fra X e Y
        /// </summary>
        static int predictDays;
        /// <summary>
        /// Numero di valori presi per le medie
        /// </summary>
        static int range;
        /// <summary>
        /// Indice della stock da considerare
        /// </summary>
        static int stockIndex;
        /// <summary>
        /// Tipo di dataset utilizzato per la simulazione
        /// </summary>
        static DataSetType dsType;
        /// <summary>
        /// Numero di hidden layers della rete neurale
        /// </summary>
        static int hiddenLayers;
        /// <summary>
        /// Numero di celle della rete neurale
        /// </summary>
        static int cellsCount;
        const double MONEY = 10000.00;
        const double COMMISSION = 0.0019;

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

            string inputFile = args[0];
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("File " + inputFile + " not found");

            for (var x = 1; x < args.Count(); x++)
            {
                switch (args[x].Trim().ToLower())
                {
                    case "/s":
                    case "/si":
                        stockIndex = Convert.ToInt32(args[x+1]);
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
            
            int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);

            orchestrator = new LSTMOrchestrator(OnReiterate, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, batchSize);
            orchestrator.LoadAndPrepareDataSet(inputFile, stockIndex, 1, dsType, predictDays, range);

            hiddenLayers = Convert.ToInt32(ConfigurationManager.AppSettings["HiddenLayers"]);
            cellsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CellsCount"]);

            ReportManager.InitialConstructor(trainingIterations, hiddenLayers, cellsCount);
            StartTraining(trainingIterations);
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Calculates trend of stocks using LSTM");
            Console.WriteLine();
            Console.WriteLine("VIXAL2 [filename] /s [stock index] /t [Dataset type] /i [iterations] /p [predict days] /r [range] /re");
            Console.WriteLine();
        }

        static void StartTraining(int iterations)
        {
            hiddenLayers = Convert.ToInt32(ConfigurationManager.AppSettings["HiddenLayers"]);
            cellsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CellsCount"]);

            Utils.DrawMessage(prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);
            if (mustReiterate)
                Reiterations = orchestrator.DataSet.TestCount;

            orchestrator.StartTraining_Sync(iterations, hiddenLayers, cellsCount, mustReiterate);
        }

        /// <summary>
        /// Updates for each iteration 
        /// </summary>
        /// <param name="iteration"></param>
        static void OnTrainingProgress(int iteration)
        {
            Utils.DrawMessage(prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)iteration / trainingIterations * 100.0), ConsoleColor.Gray);

            if (iteration == trainingIterations)
            {
                Utils.DrawMessage(prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.Green);
                Console.WriteLine();
            }
        }


        /// <summary>
        /// Updates graphs and performs actions at the end of one serie of iteration for a stock 
        /// </summary>
        static void OnTrainingEnded()
        {
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 0.0), ConsoleColor.Gray);
            var listE = orchestrator.CurrentModelTrain();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 25.0), ConsoleColor.Gray);
            var listV = orchestrator.CurrentModelValidation();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 50.0), ConsoleColor.Gray);
            var listT = orchestrator.CurrentModelTest();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 75.0), ConsoleColor.Gray);

            orchestrator.ComputePerformances(listT);

            //DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);

            var listExt = orchestrator.CurrentModelTestExtreme();
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.Green);

            var allLists = listE.Concat(listV).Concat(listT).Concat(listExt).ToList();

            ReportManager reportMan = new ReportManager(orchestrator.DataSet);
            reportMan.DrawPredicted(allLists);
            reportMan.Print();
        }

        /// <summary>
        /// Updates graphs and performs actions at the end of all series of simulatios for a stock 
        /// </summary>
        static void OnSimulationEnded()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Trading... ");
            
            List<FinTrade> trades = orchestrator.SimulateFinTrades(true);
            ReportItem item = ReportManager.ReportItemAdd(orchestrator.DataSet, orchestrator.WeightedSlopePerformance, orchestrator.AvgSlopePerformance, orchestrator.AvgDiffPerformance);
            ReportManager.EnrichReportItemWithTradesData(item, trades);
            ReportManager.EnrichReportItemWithTradesDataWithCommissions(item, trades);

            ReportManager reportMan = new ReportManager(orchestrator.DataSet);
            reportMan.DrawLatestPredicted();
            reportMan.DrawTrades(trades);
            reportMan.PrintPerformances(orchestrator.SlopePerformances, orchestrator.AvgSlopePerformance, orchestrator.DiffPerformance, orchestrator.AvgDiffPerformance);
            reportMan.Print();

            ReportManager.SaveToXML(orchestrator.DataSet.GetTestArrayY().GetColName(0), orchestrator.DataSet.ClassShortName, trades);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Simulation ENDED. Thanks for having used VIXAL2 :)!");
        }

        static void OnReiterate(StocksDataset dataset)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Reiterating... " + ReiterationProgress);
        }

        static string ReiterationProgress
        {
            get
            {
                return (Reiterations - orchestrator.DataSet.TestCount + 1) + "/" + Reiterations;
            }
        }
    }
}
