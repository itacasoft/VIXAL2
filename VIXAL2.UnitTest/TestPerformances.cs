using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestPerformances
    {
        [TestMethod]
        public void Test_ComparePredictedAgainstDataYSlopes()
        {
            double[] dataY = new double[5];
            dataY[0] = 1;
            dataY[1] = 3;
            dataY[2] = 7;
            dataY[3] = 2;
            dataY[4] = 8;

            double[] predicted = new double[5];
            predicted[0] = 2;
            predicted[1] = 5;
            predicted[2] = 11;
            predicted[3] = 4;
            predicted[4] = 8;

            Performance[] perfList = new Performance[15];

            LSTMUtils.CompareSlopes(dataY, predicted, ref perfList);

            Assert.AreEqual(perfList.Count(), 5);
            Assert.AreEqual(perfList[0].Guessed, 0);
            Assert.AreEqual(perfList[0].Failed, 0);
            Assert.AreEqual(perfList[0].Total, 0);
            Assert.AreEqual(perfList[0].SuccessPercentage, double.NaN);
            Assert.AreEqual(perfList[1].Guessed, 1);
            Assert.AreEqual(perfList[1].Failed, 0);
            Assert.AreEqual(perfList[1].Total, 1);
            Assert.AreEqual(perfList[1].SuccessPercentage, 1);
            Assert.AreEqual(perfList[2].Guessed, 1);
            Assert.AreEqual(perfList[2].Failed, 0);
            Assert.AreEqual(perfList[2].Total, 1);
            Assert.AreEqual(perfList[2].SuccessPercentage, 1);
            Assert.AreEqual(perfList[4].Guessed, 1);
            Assert.AreEqual(perfList[4].Failed, 0);
            Assert.AreEqual(perfList[4].Total, 1);
            Assert.AreEqual(perfList[4].SuccessPercentage, 1);

            double[] dataY2 = new double[4];
            dataY2[0] = 3;
            dataY2[1] = 7;
            dataY2[2] = 2;
            dataY2[3] = 8;

            double[] predicted2 = new double[4];
            predicted2[0] = 2;
            predicted2[1] = 3;
            predicted2[2] = 11;
            predicted2[3] = 14;

            LSTMUtils.CompareSlopes(dataY2, predicted2, ref perfList);

            Assert.AreEqual(perfList.Count(), 5);
            Assert.AreEqual(perfList[1].Guessed, 2);
            Assert.AreEqual(perfList[1].Failed, 0);
            Assert.AreEqual(perfList[1].Total, 2);
            Assert.AreEqual(perfList[1].SuccessPercentage, 1);
            Assert.AreEqual(perfList[2].Guessed, 1);
            Assert.AreEqual(perfList[2].Failed, 1);
            Assert.AreEqual(perfList[2].Total, 2);
            Assert.AreEqual(perfList[2].SuccessPercentage, 0.5);
            Assert.AreEqual(perfList[4].Guessed, 1);
            Assert.AreEqual(perfList[4].Failed, 0);
            Assert.AreEqual(perfList[4].Total, 1);
            Assert.AreEqual(perfList[4].SuccessPercentage, 1);

        }


        [TestMethod]
        public void CompareDifferences_on_same_values()
        {
            double[] dataY = new double[10];
            dataY[0] = 1;
            dataY[1] = 2;
            dataY[2] = 3;
            dataY[3] = 4;
            dataY[4] = 5;
            dataY[5] = 6;
            dataY[6] = 7;
            dataY[7] = 8;
            dataY[8] = 9;
            dataY[9] = 10;

            double[] predicted = new double[10];
            predicted[0] = 1;
            predicted[1] = 2;
            predicted[2] = 3;
            predicted[3] = 4;
            predicted[4] = 5;
            predicted[5] = 6;
            predicted[6] = 7;
            predicted[7] = 8;
            predicted[8] = 9;
            predicted[9] = 10;

            double diff = LSTMUtils.CompareDifferences(dataY, predicted);
            Assert.AreEqual(diff,0);
        }

        [TestMethod]
        public void CompareDifferences_on_parallel_values()
        {
            double[] dataY = new double[10];
            dataY[0] = 1;
            dataY[1] = 2;
            dataY[2] = 3;
            dataY[3] = 4;
            dataY[4] = 5;
            dataY[5] = 6;
            dataY[6] = 7;
            dataY[7] = 8;
            dataY[8] = 9;
            dataY[9] = 10;

            double[] predicted = new double[10];
            predicted[0] = 3;
            predicted[1] = 4;
            predicted[2] = 5;
            predicted[3] = 6;
            predicted[4] = 7;
            predicted[5] = 8;
            predicted[6] = 9;
            predicted[7] = 10;
            predicted[8] = 11;
            predicted[9] = 12;

            double diff = LSTMUtils.CompareDifferences(dataY, predicted);
            Assert.AreEqual(diff, 0);
        }

        [TestMethod]
        public void CompareDifferences_on_inverse_values()
        {
            double[] dataY = new double[10];
            dataY[0] = 1;
            dataY[1] = 2;
            dataY[2] = 3;
            dataY[3] = 4;
            dataY[4] = 5;
            dataY[5] = 6;
            dataY[6] = 7;
            dataY[7] = 8;
            dataY[8] = 9;
            dataY[9] = 10;

            double[] predicted = new double[10];
            predicted[0] = 10;
            predicted[1] = 9;
            predicted[2] = 8;
            predicted[3] = 7;
            predicted[4] = 6;
            predicted[5] = 5;
            predicted[6] = 4;
            predicted[7] = 3;
            predicted[8] = 2;
            predicted[9] = 1;

            double diff = LSTMUtils.CompareDifferences(dataY, predicted);
            Assert.IsTrue(diff > 1.8 && diff < 1.9);
        }

        [TestMethod]
        public void CompareDifferences_for_max_difference()
        {
            double[] dataY = new double[10];
            dataY[0] = 0.000001;
            dataY[1] = 0.000001;
            dataY[2] = 0.000001;
            dataY[3] = 0.000001;
            dataY[4] = 0.000001;
            dataY[5] = 10;
            dataY[6] = 10;
            dataY[7] = 10;
            dataY[8] = 10;
            dataY[9] = 10;

            double[] predicted = new double[10];
            predicted[0] = 10;
            predicted[1] = 10;
            predicted[2] = 10;
            predicted[3] = 10;
            predicted[4] = 10;
            predicted[5] = 0.000001;
            predicted[6] = 0.000001;
            predicted[7] = 0.000001;
            predicted[8] = 0.000001;
            predicted[9] = 0.000001;

            double diff = LSTMUtils.CompareDifferences(dataY, predicted);
            Assert.IsTrue(diff > 4999999 && diff < 5000000);
        }

        [TestMethod]
        public void CompareDifferences_realistic_case()
        {
            double[] dataY = new double[10];
            dataY[0] = 1;
            dataY[1] = 2;
            dataY[2] = 3;
            dataY[3] = 4;
            dataY[4] = 5;
            dataY[5] = 6;
            dataY[6] = 7;
            dataY[7] = 8;
            dataY[8] = 9;
            dataY[9] = 10;

            double[] predicted = new double[10];
            predicted[0] = 1;
            predicted[1] = 2;
            predicted[2] = 3;
            predicted[3] = 3;
            predicted[4] = 3;
            predicted[5] = 8;
            predicted[6] = 10;
            predicted[7] = 8;
            predicted[8] = 9;
            predicted[9] = 10;

            double diff = LSTMUtils.CompareDifferences(dataY, predicted);
            Assert.IsTrue(diff > 0.18 && diff < 0.19);
        }


        [TestMethod]
        public void CompareDifferences_15_values_inverse()
        {
            double[] dataY2 = new double[15];
            dataY2[0] = 1;
            dataY2[1] = 2;
            dataY2[2] = 3;
            dataY2[3] = 4;
            dataY2[4] = 5;
            dataY2[5] = 6;
            dataY2[6] = 7;
            dataY2[7] = 8;
            dataY2[8] = 9;
            dataY2[9] = 10;
            dataY2[10] = 11;
            dataY2[11] = 12;
            dataY2[12] = 13;
            dataY2[13] = 14;
            dataY2[14] = 15;

            double[] predicted2 = new double[15];
            predicted2[0] = 15;
            predicted2[1] = 14;
            predicted2[2] = 13;
            predicted2[3] = 12;
            predicted2[4] = 11;
            predicted2[5] = 10;
            predicted2[6] = 9;
            predicted2[7] = 8;
            predicted2[8] = 7;
            predicted2[9] = 6;
            predicted2[10] = 5;
            predicted2[11] = 4;
            predicted2[12] = 3;
            predicted2[13] = 2;
            predicted2[14] = 1;

            double diff = LSTMUtils.CompareDifferences(dataY2, predicted2);
            Assert.IsTrue(diff > 2.12 && diff < 2.13);
        }
    }
}
