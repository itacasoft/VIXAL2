using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data;
using VIXAL2.UnitTest.Data;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class DiscontinuousDataSetTest
    {
        [TestMethod]
        public void CreateStairVectorTest()
        {
            double[] dataY = EnergyData.Eni_Future(10);
            IEnumerable<double> dataXE = EnergyData.Eni.Take(dataY.Length);
            double[] dataX = dataXE.ToArray<double>();

            Assert.AreEqual(dataX[0], EnergyData.Eni[0]);

            double[] stairData = DiscontinuousDataSet.CreateStairVector(dataX, dataY);
            Assert.AreEqual(stairData[0], -0.2);
            Assert.AreEqual(stairData[2], 0.1);
            Assert.AreEqual(stairData[6], 0.2);
        }
    }
}
