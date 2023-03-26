using System;
using System.Collections.Generic;
using System.Linq;
using SharpML.Types;
using VIXAL2.Data;
using VIXAL2.Data.Report;
using NeuralNetwork.Base;
using VIXAL2.Data.Base;
using System.Configuration;

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
        /// Input dtaa containing values for training and simulation
        /// </summary>
        RawStocksData inputData;
        /// <summary>
        /// Filter by date (from)
        /// </summary>
        string sDataDa;
        /// <summary>
        /// Filter by date (to)
        /// </summary>
        string sDataA;

        string yStock;
        LSTMOrchestrator orchestrator;

        public SimulationManager(RawStocksData inputData, DataSetType dsType, int predictDays, int range, int trainingIterations, bool mustReiterate, string sDataDa = "1900.01.01", string sDataA = "2099.12.31")
        {
            this.inputData = inputData;
            this.dsType = dsType;
            this.predictDays = predictDays;
            this.range = range;
            this.trainingIterations = trainingIterations;
            this.mustReiterate = mustReiterate;
            this.sDataA = sDataA;
            this.sDataDa = sDataDa;

            orchestrator = new LSTMOrchestrator(OnReiterate, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, BatchSize);
        }


        public void StartTraining(int stockIndex)
        {
            var ds = orchestrator.LoadAndPrepareDataSet(inputData, stockIndex, 1, dsType, predictDays, range);
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

            GraphManager.latestModelList = listE.Concat(listV).Concat(listT).ToList();
            GraphManager.latestPredictedList = listExt.ToList();
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
            ReportItem item = Report.Manager.ReportItemAdd(orchestrator.DataSet, orchestrator.WeightedSlopePerformance, orchestrator.AvgSlopePerformance, orchestrator.AvgDiffPerformance);
            //Report.Manager.EnrichReportItemWithTradesData(item, trades);
            Report.Manager.EnrichReportItemWithTradesDataWithCommissions(item, trades);

            GraphManager graphMan = new GraphManager(orchestrator.DataSet, trainingIterations, HiddenLayers, CellsCount, BatchSize);
            graphMan.DrawLatestList();
            graphMan.DrawTrades(trades);
            graphMan.PrintPerformances(orchestrator.SlopePerformances, orchestrator.AvgSlopePerformance, orchestrator.DiffPerformance, orchestrator.AvgDiffPerformance);
            graphMan.Print();

            Report.Manager.SaveToExcel(orchestrator.DataSet.GetTestArrayY().GetColName(0), orchestrator.DataSet.DsType.ToString(), trades);
//            Report.Manager.SaveTradesToXML(orchestrator.DataSet.GetTestArrayY().GetColName(0), orchestrator.DataSet.DsType.ToString(), trades);
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
