using NeuralNetwork.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using VIXAL2.Data;
using VIXAL2.Data.Base;

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
        static bool mustReiterate = true;
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
                throw new ArgumentNullException("Null argument passed, first argument must be the full Dataset file");

            string inputFile = args[0];
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("File " + inputFile + " not found");

            stockIndex = Convert.ToInt32(args[1]);
            dsType = (DataSetType)Convert.ToInt32(args[2]);
            predictDays = Convert.ToInt32(args[3]);
            range = Convert.ToInt32(args[4]);
            trainingIterations = Convert.ToInt32(args[5]);

            if (args.Length > 6)
                mustReiterate = Convert.ToBoolean(args[6]);

            int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);

            orchestrator = new LSTMOrchestrator(OnReiterate, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, batchSize);
            orchestrator.LoadAndPrepareDataSet(inputFile, stockIndex, 1, dsType, predictDays, range);

            StartTraining(trainingIterations);
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


        static void OnTrainingProgress(int iteration)
        {
            Utils.DrawMessage(prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)iteration / trainingIterations * 100.0), ConsoleColor.Gray);

            if (iteration == trainingIterations)
            {
                Utils.DrawMessage(prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.Green);
                Console.WriteLine();
            }
        }

        static void OnTrainingEnded()
        {
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 0.0), ConsoleColor.Gray);
            var predictedListE = orchestrator.CurrentModelEvaluation();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 25.0), ConsoleColor.Gray);
            var predictedListV = orchestrator.CurrentModelValidation();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 50.0), ConsoleColor.Gray);
            var predictedListT = orchestrator.CurrentModelTest();
            
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 75.0), ConsoleColor.Gray);

            orchestrator.ComputePerformances(predictedListT);

            //DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);

            var tradeResult = orchestrator.SimulateTrades(predictedListT, MONEY, COMMISSION);
            //DrawTrades(tradeResult);

            var predictedListExt = orchestrator.CurrentModelTestExtreme();
            Utils.DrawMessage(prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.Green);

            var man = new ReportManager(trainingIterations, hiddenLayers, cellsCount );
            man.PrintGraphs(orchestrator.DataSet, predictedListE);
        }

        static void OnSimulationEnded()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Simulation ENDED. Thanks for having used VIXAL2 :)!");
        }

        static void OnReiterate(StocksDataset dataset)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Reiteratating... " + ReiterationProgress);
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
