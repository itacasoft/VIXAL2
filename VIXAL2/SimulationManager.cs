using System;
using System.Collections.Generic;
using System.Linq;
using SharpML.Types;
using VIXAL2.Data;
using VIXAL2.Data.Report;
using NeuralNetwork.Base;
using VIXAL2.Data.Base;

namespace VIXAL2
{
    internal class SimulationManager
    {
        #region Static properties
        static string prefixTraining = "Training...";
        static string prefixSimulating = "Simulating...";

        const double MONEY = 10000.00;
        const double COMMISSION = 0.0019;

        public static int BatchSize;
        /// <summary>
        /// Numero di hidden layers della rete neurale
        /// </summary>
        public static int HiddenLayers;
        /// <summary>
        /// Numero di celle della rete neurale
        /// </summary>
        public static int CellsCount;
        #endregion
        /// <summary>
        /// Numero di iterazioni all'interno di un training
        /// </summary>
        int trainingIterations;
        /// <summary>
        /// Indica se la reiterazione è richiesta
        /// </summary>
        bool mustReiterate = false;
        /// <summary>
        /// Numero di re-iterazioni totali
        /// </summary>
        int Reiterations;
        /// <summary>
        /// Numero di giorni di predizione fra X e Y
        /// </summary>
        int predictDays;
        /// <summary>
        /// Numero di valori presi per le medie
        /// </summary>
        int range;
        /// <summary>
        /// Tipo di dataset utilizzato per la simulazione
        /// </summary>
        DataSetType dsType;
        /// <summary>
        /// CSV file containing values for training
        /// </summary>
        string inputFile;

        string yStock;
        LSTMOrchestrator orchestrator;

        public SimulationManager(string inputFile, DataSetType dsType, int predictDays, int range, int trainingIterations, bool mustReiterate)
        {
            this.inputFile = inputFile;
            this.dsType = dsType;
            this.predictDays = predictDays;
            this.range = range;
            this.trainingIterations = trainingIterations;
            this.mustReiterate = mustReiterate;

            orchestrator = new LSTMOrchestrator(OnReiterate, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, BatchSize);
        }


        public void StartTraining(int stockIndex)
        {
            var ds = orchestrator.LoadAndPrepareDataSet(inputFile, stockIndex, 1, dsType, predictDays, range);
            yStock = ds.GetTestArrayY().GetColName(0);

            Utils.DrawMessage(yStock + "|" + prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);
            if (mustReiterate)
                Reiterations = orchestrator.DataSet.TestCount;

            orchestrator.StartTraining_Sync(trainingIterations, HiddenLayers, CellsCount, mustReiterate);
        }

        /// <summary>
        /// Updates for each iteration 
        /// </summary>
        /// <param name="iteration"></param>
        private void OnTrainingProgress(int iteration)
        {
            Utils.DrawMessage(yStock + "|" + prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, (double)iteration / trainingIterations * 100.0), ConsoleColor.Gray);

            if (iteration == trainingIterations)
            {
                Utils.DrawMessage(yStock + "|" + prefixTraining, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.DarkGreen);
                Console.WriteLine();
            }
        }


        /// <summary>
        /// Updates graphs and performs actions at the end of one serie of iteration for a stock 
        /// </summary>
        private void OnTrainingEnded()
        {
            Utils.DrawMessage(yStock + "|" + prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 0.0), ConsoleColor.Gray);
            var listE = orchestrator.CurrentModelTrain();

            Utils.DrawMessage(yStock + "|" + prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 25.0), ConsoleColor.Gray);
            var listV = orchestrator.CurrentModelValidation();

            Utils.DrawMessage(yStock + "|" + prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 50.0), ConsoleColor.Gray);
            var listT = orchestrator.CurrentModelTest();

            Utils.DrawMessage(yStock + "|" + prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 75.0), ConsoleColor.Gray);

            orchestrator.ComputePerformances(listT);

            var listExt = orchestrator.CurrentModelTestExtreme();
            Utils.DrawMessage(yStock + "|" + prefixSimulating, Utils.CreateProgressBar(Utils.ProgressBarLength, 100.0), ConsoleColor.DarkGreen);

            var allLists = listE.Concat(listV).Concat(listT).Concat(listExt).ToList();

            ReportManager.latestPredictedList = allLists;
            //ReportManager reportMan = new ReportManager(orchestrator.DataSet);
            //reportMan.DrawPredicted(allLists);
            //reportMan.Print();
        }

        /// <summary>
        /// Updates graphs and performs actions at the end of all series of simulatios for a stock 
        /// </summary>
        private void OnSimulationEnded()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(yStock + "|" + "Trading... ");

            List<FinTrade> trades = orchestrator.SimulateFinTrades(true);
            ReportItem item = ReportManager.ReportItemAdd(orchestrator.DataSet, orchestrator.WeightedSlopePerformance, orchestrator.AvgSlopePerformance, orchestrator.AvgDiffPerformance);
            ReportManager.EnrichReportItemWithTradesData(item, trades);
            ReportManager.EnrichReportItemWithTradesDataWithCommissions(item, trades);

            ReportManager reportMan = new ReportManager(orchestrator.DataSet);
            reportMan.DrawLatestPredicted();
            reportMan.DrawTrades(trades);
            reportMan.PrintPerformances(orchestrator.SlopePerformances, orchestrator.AvgSlopePerformance, orchestrator.DiffPerformance, orchestrator.AvgDiffPerformance);
            reportMan.Print();

            ReportManager.SaveToXML(orchestrator.DataSet.GetTestArrayY().GetColName(0), orchestrator.DataSet.DsType.ToString(), trades);
        }

        private void OnReiterate(StocksDataset dataset)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(yStock + "|" + "Reiterating... " + ReiterationProgress);
        }

        private string ReiterationProgress
        {
            get
            {
                return (Reiterations - orchestrator.DataSet.TestCount + 1) + "/" + Reiterations;
            }
        }

    }
}
