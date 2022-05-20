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
        public void Test_ComparePredictedAgainstDataY()
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

            List<Performance> perfList = new List<Performance>();

            LSTMUtils.Compare(dataY, predicted, ref perfList);

            Assert.AreEqual(perfList.Count, 5);
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

            LSTMUtils.Compare(dataY2, predicted2, ref perfList);

            Assert.AreEqual(perfList.Count, 5);
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
    }
}
