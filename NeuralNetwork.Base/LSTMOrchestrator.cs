using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VIXAL2.Data;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public class LSTMOrchestrator
    {
        private NeuralNetwork.Base.LSTMTrainer currentLSTMTrainer;

        private Action<int> _progressReport;
        private Action<StocksDataset> _reloadReport;
        private Action<int> _endReport;
        private int _batchSize;
        public StocksDataset DataSet;
        public LSTMOrchestrator(Action<StocksDataset> reloadReport, Action<int> progressReport, Action<int> endReport, int batchSize)
        {
            _progressReport = progressReport;
            _reloadReport = reloadReport;
            _endReport = endReport;
            _batchSize = batchSize;
        }

        public static string featuresName = "feature";
        public static string labelsName = "label";
        public int IndexColumnToPredict;
        private TimeSerieArray originalTestArrayY;
        public List<Performance> performances;

        public void LoadAndPrepareDataSet(string inputCsv, int firstColumnToPredict, int predictCount, int dataSetType, int predictDays, int range = 20)
        {
            IndexColumnToPredict = firstColumnToPredict;
            DataSet = DatasetFactory.CreateDataset(inputCsv, firstColumnToPredict, predictCount, dataSetType);
            DataSet.TrainPercent = 0.95F;
            DataSet.ValidPercent = 0.0F;
            DataSet.PredictDays = predictDays;

            if (DataSet is IAverageRangeDataSet)
            {
                ((IAverageRangeDataSet)DataSet).SetRange(range);
            }
            else if (DataSet.GetType() == typeof(MovingEnhancedAverageDataSet2))
            {
                ((MovingEnhancedAverageDataSet2)DataSet).SetHalfRange(range);
            }

            DataSet.Prepare();
            originalTestArrayY = DataSet.GetTestArrayY();
            performances = new List<Performance>();
        }

        public void StartTraining(int iterations, int hiddenLayersDim, int cellsNumber, bool reiterate)
        {
            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            Task taskA = Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iterations, _batchSize, _progressReport, NeuralNetwork.Base.DeviceType.CPUDevice));

            if (reiterate)
                taskA.ContinueWith(antecedent => ReiterateTrainingAfterForward(hiddenLayersDim, cellsNumber, iterations));
        }

        private void ReiterateTrainingAfterForward(int hiddenLayersDim, int cellsNumber, int iteration)
        {
            bool result = DataSet.Forward(1);
            if ((!result) || (currentLSTMTrainer.StopNow))
            {
                PerformEnd(iteration);
                return;
            }

            _reloadReport(DataSet);

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            Task taskA = Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iteration, _batchSize, _progressReport, NeuralNetwork.Base.DeviceType.CPUDevice));

            taskA.ContinueWith(antecedent => ReiterateTrainingAfterForward(hiddenLayersDim, cellsNumber, iteration));
        }

        private void PerformEnd(int iteration)
        {
            int i = 0;
            while (i < performances.Count)
            {
                if (performances[i].Total < 20)
                    performances.RemoveAt(i);
                else
                    i++;
            }

            _endReport(iteration);
        }

        public void StopTrainingNow()
        {
            currentLSTMTrainer.StopNow = true;
        }

        public Tuple<float, float, float> CompareForwardWithDataY()
        {
            var result = LSTMUtils.Compare(originalTestArrayY, 0, DataSet.ForwardPredicted);
            return result;
        }

        public List<Performance> ComparePredictedAgainstDataY(double[] predicted, int columnToPredict)
        {
            double[] dataYList = Utils.GetVectorFromArray(DataSet.TestDataY, columnToPredict);
            LSTMUtils.Compare(dataYList, predicted, ref performances);
            return performances;
        }

        public double GetPreviousLossAverage()
        {
            return currentLSTMTrainer.PreviousMinibatchLossAverage;
        }

        public IList<IList<float>> CurrentModelEvaluate(IEnumerable<float> miniBatchData_X, IEnumerable<float> miniBatchData_Y)
        {
            return currentLSTMTrainer.CurrentModelEvaluate(miniBatchData_X, miniBatchData_Y);
        }

        public IList<IList<float>> CurrentModelTest(IEnumerable<float> miniBatchData_X)
        {
            return currentLSTMTrainer.CurrentModelTest(miniBatchData_X);
        }

        public IEnumerable<(float[] X, float[] Y)> GetBatchesForTraining()
        {
            return nextBatch(DataSet["features"].train, DataSet["label"].train, _batchSize);
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
    }
}
