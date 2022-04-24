using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Accord.Math;
using VIXAL2.UnitTest.Data;

namespace VIXAL2.UnitTest
{
    internal class TestData
    {
        public double[][] TrainX;
        public double[][] ValidX;
        public double[][] TestX;
    }

    public class PCATests
    {
        internal TestData GetEnergyData(double validFrom, double validTo, double testFrom)
        {
            int ValidCount = Convert.ToInt32(EnergyData.ICLN.Length * (validTo - validFrom));
            int TrainCount = Convert.ToInt32(EnergyData.ICLN.Length * (validFrom));
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

        private void PcaTransformation(int xy)
        {
/*
            TestData data = GetEnergyData(0.6, 0.8, 0.8);
            double[][] train = data.TrainX;
            double[][] valid = data.ValidX;
            double[][] test = data.TestX;

            // Perform PCA
            double[] w = null;
            double[,] u = null;
            double[,] vt = new double[0, 0];
            double[,] cov = train.Transpose().Dot(train).ToMatrix().Divide(train.Length);

            alglib.svd.rmatrixsvd(cov, cov.GetLength(0), cov.GetLength(1), 0, 1, 2, ref w, ref u, ref vt);
            vt = vt.Transpose();

            // Reduce coefficients
            double[,] vtt = new double[vt.GetLength(0), (int)Math.Round(vt.GetLength(1) * 1.0)];
            for (int i = 0; i < vtt.GetLength(0); ++i)
                for (int k = 0; k < vtt.GetLength(1); ++k)
                    vtt[i, k] = vt[i, k];
            vt = vtt;

            // Get new data
            train = train.Dot(vt);
            valid = valid.Dot(vt);
            test = test.Dot(vt);

            Types.RnnConfig rnnConfig = new Types.RnnConfig();
            rnnConfig.PcaCoefficients[0] = vt.Transpose();

            data.TrainX = train;
            data.ValidX = valid;
            data.TestX = test;
*/
        }
    }
}
