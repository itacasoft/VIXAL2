using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private int _batchSize;
        public StocksDataset DataSet;
        public LSTMOrchestrator(Action<StocksDataset> reloadReport, Action<int> progressReport, int batchSize)
        {
            _progressReport = progressReport;
            _reloadReport = reloadReport;
            _batchSize = batchSize;
        }

        public static string featuresName = "feature";
        public static string labelsName = "label";

        private TimeSerieArray originalTestArrayY;

        public void LoadAndPrepareDataSet(string inputCsv, int firstColumnToPredict, int predictCount, int dataSetType, int predictDays)
        {
            DataSet = DatasetFactory.CreateDataset(inputCsv, firstColumnToPredict, predictCount, dataSetType);
            DataSet.TrainPercent = 0.96F;
            DataSet.ValidPercent = 0.0F;
            DataSet.PredictDays = predictDays;
            if (DataSet.GetType() == typeof(MovingAverageDataSet))
                ((MovingAverageDataSet)DataSet).PredictDays = predictDays;
            DataSet.Prepare();
            originalTestArrayY = DataSet.GetTestArrayY();
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
            if (!result) return;

            _reloadReport(DataSet);

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);

            Task taskA = Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iteration, _batchSize, _progressReport, NeuralNetwork.Base.DeviceType.CPUDevice));

            taskA.ContinueWith(antecedent => ReiterateTrainingAfterForward(hiddenLayersDim, cellsNumber, iteration));
        }

        public void StopTrainingNow()
        {
            currentLSTMTrainer.StopNow = true;
        }

        public float CompareForwardWithDataY(List<Tuple<DateTime, float>> forwardModelList)
        {
            return LSTMUtils.Compare2(originalTestArrayY, forwardModelList);
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
