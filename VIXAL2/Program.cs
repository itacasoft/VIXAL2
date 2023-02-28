using NeuralNetwork.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data.Base;

namespace VIXAL2
{
    internal class Program
    {
        static LSTMOrchestrator orchestrator;


        static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException("Null argument passed, first argument must be the full Dataset file");

            string inputFile = args[0];
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("File " + inputFile + " not found");

            int stockIndex = Convert.ToInt32(args[1]);
            DataSetType dsType = (DataSetType)Convert.ToInt32(args[2]);
            int predictDays = Convert.ToInt32(args[3]);
            int range = Convert.ToInt32(args[4]);
            int iterations = Convert.ToInt32(args[5]);

            int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["BatchSize"]);

            orchestrator = new LSTMOrchestrator(null, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, batchSize);
            orchestrator.LoadAndPrepareDataSet(inputFile, stockIndex, 1, dsType, predictDays, range);

            StartTraining(iterations).Wait();
        }

        static async Task StartTraining(int iterations)
        {
            int hiddenLayers = Convert.ToInt32(ConfigurationManager.AppSettings["HiddenLayers"]);
            int cellsCount = Convert.ToInt32(ConfigurationManager.AppSettings["CellsCount"]);
            orchestrator.StartTraining(iterations, hiddenLayers, cellsCount, false);
        }


        static void OnTrainingProgress(int iteration)
        {
            string prefix = "Training...";
            Utils.DrawMessage(prefix, Utils.CreateProgressBar(Utils.ProgressBarLength, 0), ConsoleColor.Gray);
        }

        static void OnTrainingEnded()
        {

        }

        static void OnSimulationEnded()
        {

        }
    }
}
