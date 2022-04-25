using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Accord.Math;
using VIXAL2.UnitTest.Data;
using SharpML.Types;

namespace VIXAL2.UnitTest
{
    internal class TestData
    {
        public double[][] TrainX;
        public double[][] ValidX;
        public double[][] TestX;
    }
    
    [TestClass]
    public class MatrixTests
    {
        internal TestData GetEnergyData(double trainPerc, double validPerc)
        {
            int TrainCount = Convert.ToInt32(EnergyData.ICLN.Length * trainPerc);
            int ValidCount = Convert.ToInt32(EnergyData.ICLN.Length * validPerc);
            int TestCount = EnergyData.ICLN.Length - ValidCount - TrainCount;
            TestData result = new TestData();
            
            result.TrainX  = new double[TrainCount][];
            for (int i = 0; i < result.TrainX.Length; i++)
            {
                result.TrainX[i] = new double[4];
                result.TrainX[i][0] = EnergyData.Eni[i];
                result.TrainX[i][1] = EnergyData.ICLN[i];
                result.TrainX[i][2] = EnergyData.Brent[i];
                result.TrainX[i][3] = EnergyData.EurDolX[i];
            }

            result.ValidX = new double[ValidCount][];
            for (int i = 0; i < result.ValidX.Length; i++)
            {
                result.ValidX[i] = new double[4];
                result.ValidX[i][0] = EnergyData.Eni[i + TrainCount];
                result.ValidX[i][1] = EnergyData.ICLN[i + TrainCount];
                result.ValidX[i][2] = EnergyData.Brent[i + TrainCount];
                result.ValidX[i][3] = EnergyData.EurDolX[i + TrainCount];
            }

            result.TestX = new double[TestCount][];
            for (int i = 0; i < result.TestX.Length; i++)
            {
                result.TestX[i] = new double[4];
                result.TestX[i][0] = EnergyData.Eni[i + TrainCount + ValidCount];
                result.TestX[i][1] = EnergyData.ICLN[i + TrainCount + ValidCount];
                result.TestX[i][2] = EnergyData.Brent[i + TrainCount + ValidCount];
                result.TestX[i][3] = EnergyData.EurDolX[i + TrainCount + ValidCount];
            }

            return result;
        }

        [TestMethod]
        public void GetColumnFromMatrixTest()
        {
            double[][] matrix = GetEnergyData(0.6,0.2).ValidX;
            double[] vector = Utils.GetVectorFromArray(matrix, 0);

            Assert.AreEqual(vector.Length, matrix.Length);

            var vector1 = matrix.GetColumn<double>(0);
            Assert.AreEqual(vector.Length, vector1.Length);
            Assert.AreEqual(vector[0], vector1[0]);
            Assert.AreEqual(vector[11], vector1[11]);
            Assert.AreEqual(vector[vector.Length-1], vector1[vector.Length-1]);

            Assert.AreEqual(vector1[11], matrix[11][0]);
            vector1[11] = vector1[11] + 14.1;
            Assert.AreNotEqual(vector1[11], matrix[11][0]);
        }
    }
}
