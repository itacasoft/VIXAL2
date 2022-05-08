using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CNTK.UnitTestProject
{
    [TestClass]
    public class BasicTests
    {
       public static void CalculateMeanStd(string trainingDataPath, IList<StreamConfiguration> streamConfig)
        {
            var device = DeviceDescriptor.CPUDevice;
            //calculate mean and std for the minibatchsource
            // prepare the training data
            var d = new Dictionary<StreamInformation, Tuple<NDArrayView,NDArrayView>>();
            using (var mbs = MinibatchSource.TextFormatMinibatchSource(
               trainingDataPath, streamConfig, MinibatchSource.FullDataSweep, false))
            {
                d.Add(mbs.StreamInfo("feature"), new Tuple<NDArrayView, NDArrayView>(null, null));
                //compute mean and standard deviation of the population for inputs variables
                MinibatchSource.ComputeInputPerDimMeansAndInvStdDevs(mbs, d, device);
            }
        }


        public static Function NormalizedLayer(Variable inputLayer, Variable mean, Variable std)
        {
            var normalizedLayer = CNTKLib.PerDimMeanVarianceNormalize(inputLayer, mean, std);
            return normalizedLayer;
        }

        private static IEnumerable<IEnumerable<float>> GetNormalizedList(IEnumerable<IEnumerable<float>> data)
        {
            //define device where the calculation will executes
            var device = DeviceDescriptor.CPUDevice;
            Assert.AreEqual(device, DeviceDescriptor.CPUDevice);
            //convert data into enumerable list
            //var data = mData.AsEnumerable<IEnumerable<float>>();

            //assign the values 
            var vData = Value.CreateBatchOfSequences<float>(new int[] { 4 }, data, device);
            //create variable to describe the data
            var features = Variable.InputVariable(vData.Shape, DataType.Float);

            //define mean function for the variable
            var mean = CNTKLib.ReduceMean(features, new Axis(2));//Axis(2)- means calculate 
                                                                 //mean along the third axes which represent 4 features

            //map variables and data
            var inputDataMap = new Dictionary<Variable, Value>() { { features, vData } };
            var meanDataMap = new Dictionary<Variable, Value>() { { mean, null } };

            //mean calculation
            mean.Evaluate(inputDataMap, meanDataMap, device);
            //get result
            var meanValues = meanDataMap[mean].GetDenseData<float>(mean);

            //Calculation of standard deviation
            var std = calculateStd(features);
            var stdDataMap = new Dictionary<Variable, Value>() { { std, null } };
            //mean calculation
            std.Evaluate(inputDataMap, stdDataMap, device);
            //get result
            var stdValues = stdDataMap[std].GetDenseData<float>(std);

            //Once we have mean and std, we can calculate Standardized values for the data
            var gaussNormalization = CNTKLib.ElementDivide(CNTKLib.Minus(features, mean), std);
            var gaussDataMap = new Dictionary<Variable, Value>()
                               { { gaussNormalization, null } };
            //mean calculation
            gaussNormalization.Evaluate(inputDataMap, gaussDataMap, device);

            //get result
            var normValues = gaussDataMap[gaussNormalization].GetDenseData<float>(gaussNormalization);
            return normValues;
        }

        private static IEnumerable<IEnumerable<float>> GetNormalized(IEnumerable<float> data)
        {
            //define device where the calculation will executes
            var device = DeviceDescriptor.CPUDevice;
            Assert.AreEqual(device, DeviceDescriptor.CPUDevice);
            //convert data into enumerable list
            //var data = mData.AsEnumerable<IEnumerable<float>>();

            //assign the values 
            var vData = Value.CreateBatch<float>(new int[] { 1 }, data, device);
            //create variable to describe the data
            var features = Variable.InputVariable(vData.Shape, DataType.Float);

            //define mean function for the variable
            var mean = CNTKLib.ReduceMean(features, new Axis(2));//Axis(2)- means calculate 
                                                                 //mean along the third axes which represent 4 features

            //map variables and data
            var inputDataMap = new Dictionary<Variable, Value>() { { features, vData } };
            var meanDataMap = new Dictionary<Variable, Value>() { { mean, null } };

            //mean calculation
            mean.Evaluate(inputDataMap, meanDataMap, device);
            //get result
            var meanValues = meanDataMap[mean].GetDenseData<float>(mean);

            //Calculation of standard deviation
            var std = calculateStd(features);
            var stdDataMap = new Dictionary<Variable, Value>() { { std, null } };
            //mean calculation
            std.Evaluate(inputDataMap, stdDataMap, device);
            //get result
            var stdValues = stdDataMap[std].GetDenseData<float>(std);

            //Once we have mean and std, we can calculate Standardized values for the data
            var gaussNormalization = CNTKLib.ElementDivide(CNTKLib.Minus(features, mean), std);
            var gaussDataMap = new Dictionary<Variable, Value>()
                               { { gaussNormalization, null } };
            //mean calculation
            gaussNormalization.Evaluate(inputDataMap, gaussDataMap, device);

            //get result
            var normValues = gaussDataMap[gaussNormalization].GetDenseData<float>(gaussNormalization);
            return normValues;
        }


        [TestMethod]
        public void TestMethod1()
        {
            var data = Data.BasicArray.AsEnumerable<IEnumerable<float>>();
            var ndata = GetNormalizedList(data);

            double[][] data2 = new double[data.Count()][];
            int i = 0;
            foreach(var feature in data)
            {
                data2[i] = new double[feature.Count()];
                int j = 0;
                foreach (var value in feature)
                {
                    data2[i][j] = value;
                    j++;
                }
                i++;
            }

            var _instance = new ModernNormalizer();
            _instance.Initialize(data2);
            var data3 = _instance.Normalize(data2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var data = Data.IWDA.AsEnumerable<float>();
            var ndata = GetNormalized(data);

            //copy to doubla array
            double[] data2 = new double[data.Count()];
            int i = 0;
            foreach (var feature in data)
            {
                data2[i] = (double)feature;
                i++;
            }

            var _instance = new ModernNormalizer();
            _instance.Initialize(data2);
            var data2n = _instance.Normalize(data2,0);

            Assert.AreEqual(data.Count(), data2n.Count());
        }

        private static Function calculateStd(Variable features)
        {
            var mean = CNTKLib.ReduceMean(features, new Axis(2));
            var remainder = CNTKLib.Minus(features, mean);
            var squared = CNTKLib.Square(remainder);
            //the last dimension indicate the number of samples
            var n = new Constant(new NDShape(0),DataType.Float, features.Shape.Dimensions.Last() - 1);
            var elm = CNTKLib.ElementDivide(squared, n);
            var sum = CNTKLib.ReduceSum(elm, new Axis(2));
            var stdVal = CNTKLib.Sqrt(sum);
            return stdVal;
        }
    }

    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this Array target)
        {
            foreach (var item in target)
                yield return (T)item;
        }
    }

}
