using CNTK;
using System;
using System.Collections.Generic;

namespace NeuralNetwork.Base
{
    public class LSTMTrainer
    {
        int inDim = 5;
        int ouDim = 1;
        string featuresName = "feature";
        string labelsName = "label";

        public LSTMTrainer(int inDim, int ouDim, string featuresName, string labelsName)
        {
            this.inDim = inDim;
            this.ouDim = ouDim; 
            this.featuresName = featuresName;
            this.labelsName = labelsName;
        }

        public void Train(Dictionary<string, (float[][] train, float[][] valid, float[][] test)> dataSet,
            int hiDim, int cellDim, int iteration, int batchSize, Action<CNTK.Trainer, Function, int, DeviceDescriptor> progressReport, DeviceDescriptor device)
        {
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
            var trainer = Trainer.CreateTrainer(lstmModel, trainingLoss, prediction, parameterLearners);

            // train the model
            for (int i = 1; i <= iteration; i++)
            {
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
                    trainer.TrainMinibatch(batchData, device);
                }

                //output training process
                progressReport(trainer, lstmModel.Clone(), i, device);
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

    }
}
