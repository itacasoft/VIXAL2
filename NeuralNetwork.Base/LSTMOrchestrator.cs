using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using System.Linq;

namespace NeuralNetwork.Base
{
    public class LSTMOrchestrator
    {
        /// <summary>
        /// Dataset used for the simulation
        /// </summary>
        public StocksDataset DataSet;
        /// <summary>
        /// Performances of the simulation based on slope comparison
        /// </summary>
        //public List<Performance> SlopePerformances;
        public Performance[] SlopePerformances = new Performance[DAYS_FOR_PERFORMANCE];
        /// <summary>
        /// Performance of the simulation based on difference average
        /// </summary>
        //public List<PerformanceDiff> DiffPerformance;
        public PerformanceDiff[] DiffPerformance = new PerformanceDiff[DAYS_FOR_PERFORMANCE];
        /// <summary>
        /// Trades of the simulation
        /// </summary>
        public List<Trade> Trades;
        public PredictedData PredictedData;

        private NeuralNetwork.Base.LSTMTrainer currentLSTMTrainer;
        private Action<int> onTrainingProgress;
        private Action<StocksDataset> onReiterate;
        private Action onTrainingEnded;
        private Action onSimulationEnded;
        private int _batchSize;
        private static string featuresName = "feature";
        private static string labelsName = "label";
        /// <summary>
        /// Indice dello stock che voglio predire
        /// </summary>
        private int indexColumnToPredict;
        
        int MAX_DAYS_FOR_TRADE = 5;
        int TRADE_LENGHT = 10;
        double MIN_TREND = 0.03;
        public const int DAYS_FOR_PERFORMANCE = 15;
        public int Iterations;

        public LSTMOrchestrator(Action<StocksDataset> onReiterate, Action<int> onTrainingProgress, Action onTrainingEnded, Action onSimulationEnded, int batchSize)
        {
            this.onTrainingProgress = onTrainingProgress;
            this.onReiterate = onReiterate;
            this.onTrainingEnded = onTrainingEnded;
            this.onSimulationEnded = onSimulationEnded;
            _batchSize = batchSize;

            MAX_DAYS_FOR_TRADE = Convert.ToInt32(ConfigurationManager.AppSettings["MaxDaysForTradesSimulation"]);
            TRADE_LENGHT = Convert.ToInt32(ConfigurationManager.AppSettings["TradeLenghtForTradesSimulation"]);
            MIN_TREND = Convert.ToDouble(ConfigurationManager.AppSettings["MinTrendForTradesSimulation"], CultureInfo.InvariantCulture);

            //create obects for arrays
            for (int i=0; i< SlopePerformances.Length; i++)
            {
                SlopePerformances[i] = new Performance();
            }

            for (int i = 0; i < DiffPerformance.Length; i++)
            {
                DiffPerformance[i] = new PerformanceDiff();
            }
        }

        private void BaseLoadAndPrepareDataSet(int predictDays, int range = 20)
        {
            if (DataSet is IAverageRangeDataSet)
            {
                ((IAverageRangeDataSet)DataSet).SetRange(range);
            }

            DataSet.PredictDays = predictDays;

            var t = DataSet.CalculateTrainAndValidPercent(DataSet.Dates.Count, DataSet.PredictDays, DataSet.Range);

            DataSet.Prepare(t.Item1, t.Item2);
            Trades = new List<Trade>();
        }

        public StocksDataset LoadAndPrepareDataSet(string inputCsv, int indexColumnToPredict, int predictCount, DataSetType dataSetType, int predictDays, int range, string sDataDa = "1900.01.01", string sDataA = "2099.12.31")
        {
            this.indexColumnToPredict = indexColumnToPredict;
            DataSet = DatasetFactory.CreateDataset(inputCsv, indexColumnToPredict, predictCount, dataSetType, sDataDa, sDataA);

            BaseLoadAndPrepareDataSet(predictDays, range);
            return DataSet;
        }

        public void LoadAndPrepareDataSet(StocksDataset ds, int predictDays, int range = 20)
        {
            this.indexColumnToPredict = ds.FirstColumnToPredict;
            DataSet = ds;

            BaseLoadAndPrepareDataSet(predictDays, range);
        }

        public void StartTraining(int iterations, int hiddenLayersDim, int cellsNumber, bool reiterate)
        {
            this.Iterations = iterations;
            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            Task taskA = Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iterations, _batchSize, TrainingProgress, NeuralNetwork.Base.DeviceType.CPUDevice));

            if (reiterate)
                taskA.ContinueWith(antecedent => ReiterateTrainingAfterForward(hiddenLayersDim, cellsNumber, iterations));
        }

        public void StartTraining_Sync(int iterations, int hiddenLayersDim, int cellsNumber, bool reiterate)
        {
            this.Iterations = iterations;
            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iterations, _batchSize, TrainingProgress, NeuralNetwork.Base.DeviceType.CPUDevice);

            if (reiterate)
                ReiterateTrainingAfterForward_Sync(hiddenLayersDim, cellsNumber, iterations);
        }

        private void ReiterateTrainingAfterForward(int hiddenLayersDim, int cellsNumber, int iteration)
        {
            bool result = DataSet.Forward(1);
            if ((!result) || (currentLSTMTrainer.StopNow))
            {
                if (currentLSTMTrainer.StopNow) 
                    onTrainingEnded();

                onSimulationEnded();
                return;
            }

            onReiterate(DataSet);

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            Task taskA = Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iteration, _batchSize, TrainingProgress, NeuralNetwork.Base.DeviceType.CPUDevice));

            taskA.ContinueWith(antecedent => ReiterateTrainingAfterForward(hiddenLayersDim, cellsNumber, iteration));
        }

        private void ReiterateTrainingAfterForward_Sync(int hiddenLayersDim, int cellsNumber, int iteration)
        {
            bool result = DataSet.Forward(1);
            if ((!result) || (currentLSTMTrainer.StopNow))
            {
                if (currentLSTMTrainer.StopNow)
                    onTrainingEnded();

                onSimulationEnded();
                return;
            }

            onReiterate(DataSet);

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iteration, _batchSize, TrainingProgress, NeuralNetwork.Base.DeviceType.CPUDevice);

            ReiterateTrainingAfterForward_Sync(hiddenLayersDim, cellsNumber, iteration);
        }


        public void StopTrainingNow()
        {
            currentLSTMTrainer.StopNow = true;
        }

        public List<Trade> SimulateTrades(List<DoubleDatedValue> predictedList, double MONEY, double COMMISSION)
        {
            if (DataSet.NormalizeFirst)
            {
                //denormalizzo altrimenti non posso fare calcoli di trading corretti
                for (int i = 0; i < predictedList.Count; i++)
                {
                    predictedList[i].Value = DataSet.Decode(predictedList[i].Value, indexColumnToPredict);
                }
            }

            var tradeSim = new TradesSimulator(MAX_DAYS_FOR_TRADE, TRADE_LENGHT);
            tradeSim.MinTrend = MIN_TREND;
            var tradeResult = tradeSim.Trade(DataSet.OriginalData, indexColumnToPredict, predictedList, MONEY, COMMISSION);
            //prendo solo il primo perchè ritengo che sia più affidabile
            if (tradeResult.Count>0) Trades.Add(tradeResult[0]);

            return Trades;
        }

        public List<FinTrade> SimulateFinTrades(bool applyCommissions)
        {
            var tradeSim = new FinTradeSimulator(this.PredictedData, applyCommissions);
            tradeSim.MinTrend = 0.00;
            var moneyGain = tradeSim.Trade(10000);
            var tradesCount = tradeSim.TradesCount;
            var gainCount = tradeSim.TradesGainCount;
            var lossCount = tradeSim.TradesLossCount;

            return tradeSim.Trades;
        }

        /// <summary>
        /// Calcola slope e difference performance
        /// </summary>
        /// <param name="predictedTestData"></param>
        public void ComputePerformances(List<DoubleDatedValue> predictedTestData)
        {
            //la colonna del TestDataY è sempre la prima perchè ha solo una colonna
            double[] dataYList = Utils.GetVectorFromArray(DataSet.TestDataY, 0);
            
            LSTMUtils.CompareSlopes(dataYList, DoubleDatedValue.ToDoubleArray(predictedTestData), ref SlopePerformances);
            LSTMUtils.CompareDifferences(dataYList, DoubleDatedValue.ToDoubleArray(predictedTestData), ref DiffPerformance);

            SetDatesOnPerformances(ref SlopePerformances);
            SetDatesOnPerformances(ref DiffPerformance);

            if (PredictedData == null)
            {
                List<DatedValue> originalData = new List<DatedValue>();

                for (int i = 0; i < predictedTestData.Count; i++)
                {
                    var myDate = predictedTestData[i].Date;
                    var value = DataSet.OriginalData.GetValue(myDate, indexColumnToPredict);
                    DatedValue item = new DatedValue(myDate, value);
                    originalData.Add(item);
                }

                //creo il PredictedData passandogli l'intera lista di valori originali
                PredictedData = new PredictedData(originalData);
                PredictedData.StockName = DataSet.OriginalData.GetColName(indexColumnToPredict);
            }

            PredictedData.AddPredictedCurve(predictedTestData);
        }

        public double GetPreviousLossAverage()
        {
            return currentLSTMTrainer.PreviousMinibatchLossAverage;
        }

        void TrainingProgress(int iteration)
        {
            onTrainingProgress(iteration);
            if (iteration == Iterations)
            {
                onTrainingEnded();
            }
        }


        public List<DoubleDatedValue> CurrentModelTrain()
        {
            var result = new List<DoubleDatedValue>();
            //get traindatay so I have the correct dates
            var evalDataY = DataSet.GetTrainArrayY();
            int mydateIndex = 0;

            //get the next minibatch amount of data
            foreach (var miniBatchData in GetBatchesForTraining())
            {
                var oData = currentLSTMTrainer.CurrentModelEvaluate(miniBatchData.X, miniBatchData.Y); 
                foreach (var y in oData)
                {
                    //result.Add(y[0]);
                    //denormalize before return
                    if (DataSet.NormalizeFirst)
                        //result.Add(y[0]);
                        result.Add(new DoubleDatedValue(evalDataY.GetDate(mydateIndex), evalDataY.GetFutureDate(mydateIndex), y[0]));
                    else
                        //result.Add(DataSet.Decode(y[0], indexColumnToPredict));
                        result.Add(new DoubleDatedValue(evalDataY.GetDate(mydateIndex), evalDataY.GetFutureDate(mydateIndex), DataSet.Decode(y[0], indexColumnToPredict)));
                }
            }

            return result;
        }

        public List<DoubleDatedValue> CurrentModelValidation()
        {
            var result = new List<DoubleDatedValue>();
            //get validdatay so I have the correct dates
            var validDataY = DataSet.GetValidArrayY();

            //get the next minibatch amount of data
            int mydateIndex = 0;

            foreach (var miniBatchData in GetBatchesForValidation())
            {
                var oData = currentLSTMTrainer.CurrentModelTest(miniBatchData.X);

                foreach (var y in oData)
                {
                    if (DataSet.NormalizeFirst)
                        result.Add(new DoubleDatedValue(validDataY.GetDate(mydateIndex), validDataY.GetFutureDate(mydateIndex), y[0]));
                    else
                        result.Add(new DoubleDatedValue(validDataY.GetDate(mydateIndex), validDataY.GetFutureDate(mydateIndex), DataSet.Decode(y[0], indexColumnToPredict)));

                    mydateIndex++;
                }
            }
            return result;
        }


        public List<DoubleDatedValue> CurrentModelTest()
        {
            var result = new List<DoubleDatedValue>();
            //get testdatay so I have the correct dates
            var testDataY = DataSet.GetTestArrayY();

            //get the next minibatch amount of data
            int mydateIndex = 0;

            foreach (var miniBatchData in GetBatchesForTest())
            {
                var oData = currentLSTMTrainer.CurrentModelTest(miniBatchData.X);

                foreach (var y in oData)
                {
                    if (DataSet.NormalizeFirst)
                        result.Add(new DoubleDatedValue(testDataY.GetDate(mydateIndex), testDataY.GetFutureDate(mydateIndex), y[0]));
                    else
                        result.Add(new DoubleDatedValue(testDataY.GetDate(mydateIndex), testDataY.GetFutureDate(mydateIndex), DataSet.Decode(y[0], indexColumnToPredict)));

                    mydateIndex++;
                }
            }
            return result;
        }

        public List<DoubleDatedValue> CurrentModelTestExtreme()
        {
            var result = new List<DoubleDatedValue>();

            TimeSerieArrayExt dad;
            if (DataSet.NormalizeFirst)
                //predico anche l'estremo
                dad = DataSet.GetExtendedArrayX(false);
            else
                //predico anche l'estremo con dati normalizzati
                dad = DataSet.GetExtendedArrayX(true);

            float[] batch = new float[dad.Length * dad.Columns];
            int sss = 0;
            for (int i = 0; i < dad.Length; i++)
            {
                for (int j = 0; j < dad.Columns; j++)
                    batch[sss++] = (float)dad.Values[i][j];
            }
            var oDataExt = currentLSTMTrainer.CurrentModelTest(batch);

            int mydateIndex = 0;
            foreach (var y in oDataExt)
            {
                if (DataSet.NormalizeFirst)
                    result.Add(new DoubleDatedValue(dad.GetDate(mydateIndex), dad.GetFutureDate(mydateIndex), y[0]));
                else
                    result.Add(new DoubleDatedValue(dad.GetDate(mydateIndex), dad.GetFutureDate(mydateIndex), DataSet.Decode(y[0], indexColumnToPredict)));

                mydateIndex++;
            }
            return result;
        }

        public IEnumerable<(float[] X, float[] Y)> GetBatchesForTraining()
        {
            return nextBatch(DataSet["features"].train, DataSet["label"].train, _batchSize);
        }

        public IEnumerable<(float[] X, float[] Y)> GetBatchesForValidation()
        {
            return nextBatch(DataSet["features"].valid, DataSet["label"].valid, _batchSize);
        }

        public IEnumerable<(float[] X, float[] Y)> GetBatchesForTest()
        {
            return nextBatch(DataSet["features"].test, DataSet["label"].test, _batchSize);
        }

        /// <summary>
        /// Iteration method for enumerating data during iteration process of training
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="mMSize"></param>
        /// <returns></returns>
        private IEnumerable<(float[] X, float[] Y)> nextBatch(float[][] X, float[][] Y, int mMSize)
        {

            float[] asBatch(float[][] data, int start, int count)
            {
                var lst = new List<float>();
                for (int i = start; i < start + count; i++)
                {
                    if (i >= data.Length)
                        break;

                    lst.AddRange(data[i]);
                }
                return lst.ToArray();
            }

            for (int i = 0; i <= X.Length - 1; i += mMSize)
            {
                var size = X.Length - i;
                if (size > 0 && size > mMSize)
                    size = mMSize;

                var x = asBatch(X, i, size);
                var y = asBatch(Y, i, size);

                yield return (x, y);
            }
        }

        public double AvgSlopePerformance
        {
            get
            {
                double avgSlopePerformance = 0;

                //calcolo la media dei primi DAYS_FOR_PERFORMANCE
                for (int i = 1; i < SlopePerformances.Length; i++)
                {
                    avgSlopePerformance += SlopePerformances[i].SuccessPercentage;
                }
                avgSlopePerformance = avgSlopePerformance / (SlopePerformances.Length-1);
                return avgSlopePerformance;
            }
        }

        public double WeightedSlopePerformance
        {
            get
            {
                double result = 0;

                //calcolo la media dei primi DAYS_FOR_PERFORMANCE
                for (int i = 1; i < SlopePerformances.Length; i++)
                {
                    //il coefficiente del peso deve essere attorno a 1.8-1.9
                    double weight = 1.85 * ((double)SlopePerformances.Length + 1.00 - (double)i)/ (double)SlopePerformances.Length;
                    result += SlopePerformances[i].SuccessPercentage * weight;
                }
                result = result / (SlopePerformances.Length-1);
                return result;
            }
        }


        public double AvgDiffPerformance
        {
            get
            {
                double avgDiffPerformance = 0;
                //calcolo la media dei primi DAYS_FOR_PERFORMANCE

                for (int i = 0; i < DiffPerformance.Length; i++)
                {
                    avgDiffPerformance += DiffPerformance[i].SuccessPercentage;
                }
                avgDiffPerformance = avgDiffPerformance / DiffPerformance.Length;
                return avgDiffPerformance;
            }
        }

        public void SetDatesOnPerformances(ref Performance[] performances)
        {
            var dad = DataSet.GetExtendedArrayX(false);
            for (int i = 0; (i < dad.Length && i < performances.Count()); i++)
            {
                performances[i].Date = dad.GetFutureDate(i);
            }
        }

        public void SetDatesOnPerformances(ref PerformanceDiff[] performances)
        {
            var dad = DataSet.GetExtendedArrayX(false);
            for (int i = 0; (i < dad.Length && i < performances.Count()); i++)
            {
                performances[i].Date = dad.GetFutureDate(i);
            }
        }

    }
}
