using CNTK;
using System;
using System.Collections.Generic;

namespace NeuralNetwork.Base
{
    public enum DeviceType
    {
        UseDefaultDevice,
        CPUDevice,
        GPUDevice
    }

    public class LSTMTrainer
    {
        int inDim = 5;
        int ouDim = 1;
        string featuresName = "feature";
        string labelsName = "label";
        Function currentModel = null;
        DeviceDescriptor currentDevice = null;
        Trainer currentTrainer = null;

        public LSTMTrainer(int inDim, int ouDim, string featuresName, string labelsName)
        {
            this.inDim = inDim;
            this.ouDim = ouDim; 
            this.featuresName = featuresName;
            this.labelsName = labelsName;
        }

        public void Train(Dictionary<string, (float[][] train, float[][] valid, float[][] test)> dataSet,
            int hiDim, int cellDim, int iteration, int batchSize, Action<int> progressReport, DeviceType deviceType, int deviceId = 0)
        {
            var device = DeviceDescriptor.CPUDevice;
            if (deviceType == DeviceType.GPUDevice)
                device = DeviceDescriptor.GPUDevice(deviceId);
            if (deviceType == DeviceType.UseDefaultDevice)
                device = DeviceDescriptor.UseDefaultDevice();

            //split dataset on train, validate and test parts
            var featureSet = dataSet["features"];
            var labelSet = dataSet["label"];

            // build the model
            var feature = Variable.InputVariable(new int[] { inDim }, DataType.Float, featuresName, null, false /*isSparse*/);
            var label = Variable.InputVariable(new int[] { ouDim }, DataType.Float, labelsName, new List<CNTK.Axis>() { CNTK.Axis.DefaultBatchAxis() }, false);

            var lstmModel = NeuralNetwork.Base.LSTMHelper.CreateModel(feature, ouDim, hiDim, cellDim, device, "timeSeriesOutput");

            Function trainingLoss = CNTKLib.SquaredError(lstmModel, label, "squarederrorLoss");
            Function prediction = CNTKLib.SquaredError(lstmModel, label, "squarederrorEval");


            // prepare for training
            TrainingParameterScheduleDouble learningRatePerSample = new TrainingParameterScheduleDouble(0.0005, 1);
            TrainingParameterScheduleDouble momentumTimeConstant = CNTKLib.MomentumAsTimeConstantSchedule(256);

            IList<Learner> parameterLearners = new List<Learner>() {
                Learner.MomentumSGDLearner(lstmModel.Parameters(), learningRatePerSample, momentumTimeConstant, /*unitGainMomentum = */true)  };

            //create trainer
            currentTrainer = Trainer.CreateTrainer(lstmModel, trainingLoss, prediction, parameterLearners);

            // train the model
            for (int i = 1; i <= iteration; i++)
            {
/*
 * 
                //get the next minibatch amount of data
                foreach (var miniBatchData in nextBatch(featureSet.train, labelSet.train, batchSize))
                {
                    var xValues = Value.CreateBatch<float>(new NDShape(1, inDim), miniBatchData.X, device);
                    var yValues = Value.CreateBatch<float>(new NDShape(1, ouDim), miniBatchData.Y, device);

                    //Combine variables and data in to Dictionary for the training
                    var batchData = new Dictionary<Variable, Value>();
                    batchData.Add(feature, xValues);
                    batchData.Add(label, yValues);

                    //train minibarch data
                    if (i == iteration-1)
                        currentTrainer.TrainMinibatch(batchData, true, device);
                    else
                        currentTrainer.TrainMinibatch(batchData, false, device);

                }
*/
                var lsit = nextBatch2(featureSet.train, labelSet.train, batchSize);

                for (int m=0; m<lsit.Count; m++)
                {
                    var miniBatchData = lsit[m];
                    var xValues = Value.CreateBatch<float>(new NDShape(1, inDim), miniBatchData.X, device);
                    var yValues = Value.CreateBatch<float>(new NDShape(1, ouDim), miniBatchData.Y, device);

                    //Combine variables and data in to Dictionary for the training
                    var batchData = new Dictionary<Variable, Value>();
                    batchData.Add(feature, xValues);
                    batchData.Add(label, yValues);

                    //train minibarch data
                    if (m == lsit.Count - 1)
                        currentTrainer.TrainMinibatch(batchData, true, device);
                    else
                        currentTrainer.TrainMinibatch(batchData, false, device);
                }

                currentModel = lstmModel.Clone();
                currentDevice = device;

                //output training process
                progressReport(i);
            }
        }

        /// <summary>
        /// Iteration method for enumerating data during iteration process of training
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="mMSize"></param>
        /// <returns></returns>
        private static IEnumerable<(float[] X, float[] Y)> nextBatch(float[][] X, float[][] Y, int mMSize)
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

        private static List<(float[] X, float[] Y)> nextBatch2(float[][] X, float[][] Y, int mMSize)
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

            List<(float[] X, float[] Y)> result;
            result = new List<(float[] X, float[] Y)>();

            for (int i = 0; i <= X.Length - 1; i += mMSize)
            {
                var size = X.Length - i;
                if (size > 0 && size > mMSize)
                    size = mMSize;

                var x = asBatch(X, i, size);
                var y = asBatch(Y, i, size);

                result.Add((x, y));
            }

            return result;

        }


        public IList<IList<float>> CurrentModelEvaluate(IEnumerable<float> miniBatchData_X, IEnumerable<float> miniBatchData_Y)
        {
            var xValues = Value.CreateBatch<float>(new NDShape(1, inDim), miniBatchData_X, currentDevice);
            var yValues = Value.CreateBatch<float>(new NDShape(1, ouDim), miniBatchData_Y, currentDevice);

            //model evaluation
            // build the model

            // var fea = Variable.InputVariable(new int[] { inDim }, DataType.Float, featuresName, null, false /*isSparse*/);
            // var lab = Variable.InputVariable(new int[] { ouDim }, DataType.Float, labelsName, new List<CNTK.Axis>() { CNTK.Axis.DefaultBatchAxis() }, false);
            var fea = currentModel.Arguments[0];
            var lab = currentModel.Output;

            var inputDataMap = new Dictionary<Variable, Value>() { { fea, xValues } };
            var outputDataMap = new Dictionary<Variable, Value>() { { lab, null } };

            currentModel.Evaluate(inputDataMap, outputDataMap, currentDevice);

            var oData = outputDataMap[lab].GetDenseData<float>(lab);
            return oData;
        }

        public double PreviousMinibatchLossAverage
        {
            get
            {
                return currentTrainer.PreviousMinibatchLossAverage();       
            }
        }


        public IList<IList<float>> CurrentModelTest(IEnumerable<float> miniBatchData_X)
        {
            var xValues = Value.CreateBatch<float>(new NDShape(1, inDim), miniBatchData_X, currentDevice);

            //model evaluation
            var fea = currentModel.Arguments[0];
            var lab = currentModel.Output;
            //evaluation preparation
            var inputDataMap = new Dictionary<Variable, Value>() { { fea, xValues } };
            var outputDataMap = new Dictionary<Variable, Value>() { { lab, null } };
            currentModel.Evaluate(inputDataMap, outputDataMap, currentDevice);
            //extract the data
            var oData = outputDataMap[lab].GetDenseData<float>(lab);
            return oData;
        }
    }
}
