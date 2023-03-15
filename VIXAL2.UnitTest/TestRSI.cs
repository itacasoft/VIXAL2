using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using VIXAL2.Data;
using VIXAL2.UnitTest.Data;


namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestRSI
    {
        [TestMethod]
        public void TestRsi()
        {
            double[] input1 =
            {
                1,
                2,
                3,
                2,
                1,
                2,
                300,
                10,
                1
            };

            double res = RsiDataSet.SingleRsi(input1);
            Assert.AreEqual(res, 50.0);
        }

        [TestMethod]
        public void TestRsi2()
        {
            double[] RR0 = {
            8.97,
            7.099296,
            7.156265,
            7.055473,
            5.777599,
            6.057189,
            5.689953,
            6.017749,
            5.866998,
            6.104518,
            6.391996,
            6.386737,
            7.340321,
            7.645328
            };

            double[] RR = {
            8.97,
            7.099296,
            7.156265,
            7.055473,
            5.777599,
            6.057189,
            5.689953,
            6.017749,
            5.866998,
            6.104518,
            6.391996,
            6.386737,
            7.340321,
            7.645328,
            7.634810,
            7.201841,
            7.531389,
            8.080927,
            8.063396,
            8.622577,
            8.172955,
            8.164190,
            8.263230,
            8.146661,
            8.158054,
            8.158054,
            8.158054,
            8.034474,
            7.521747,
            7.409561,
            7.526130,
            7.392033,
            6.976592,
            7.316656,
            7.425338,
            7.230763,
            7.365738,
            7.611146,
            7.850419,
            7.637440,
            7.637440,
            7.196583,
            7.613775,
            7.382391,
            7.498959,
            7.564693,
            7.564693,
            7.564693,
            7.637440,
            7.471789,
            7.462148,
            7.467408,
            7.990771,
            7.738781,
            7.780318,
            7.589248,
            7.544942,
            7.624324,
            7.637247,
            7.799701,
            7.838470,
            7.497867,
            7.698167,
            8.095999,
            8.306452,
            8.274145,
            8.774433,
            8.920274,
            8.654440,
            8.519674,
            7.920620,
            7.926158,
            7.919697,
            8.247377,
            8.211378,
            8.155073,
            8.191072,
            8.125536,
            8.211378,
            7.820009,
            7.828316,
            7.750781,
            7.961234,
            7.836624
            };

            int range = 14;
            double[] input = (double[])RR.Clone();

            double[] output = RsiDataSet.GetRsi(input, range);
            double a = RsiDataSet.SingleRsi(RR0);
            Assert.AreEqual(a, output[13]);
            Assert.AreEqual(output.Length, input.Length);

            double[] input1 = new double[range];
            Array.Copy(RR, RR.Length - 14, input1, 0, 14);

            double[] rsi1 = RsiDataSet.GetRsi(input1, range);
            double b = RsiDataSet.SingleRsi(input1);
            Assert.AreEqual(b, rsi1[13]);
        }

        [TestMethod]
        public void TestRsiDataSet()
        {
            const int RANGE = 14;
            const int FIRST_PREDICT = 0;
            const int PREDICT_DAYS = 10;
            const int EXPECTED_TRAINCOUNT = 52;
            const int EXPECTED_VALIDCOUNT = 17;
            const int EXPECTED_TESTCOUNT = 8;
            const int DATA_LENGHT = 100;

            double[][] data = new double[DATA_LENGHT][];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new double[2];
                data[i][0] = EnergyData.Eni[i];
                data[i][1] = EnergyData.Brent[i];
            }

            Assert.AreEqual(data.Length, DATA_LENGHT);

            DateTime[] DD = new DateTime[data.Length];
            //fill date array
            DD[0] = DateTime.Parse("01/01/2010");
            for (int i = 1; i < data.Length; i++)
            {
                DD[i] = DD[i - 1].AddDays(1);
            }

            string[] stocks =
            {
                "COMPANY_A",
                "COMPANY_B"
            };


            RsiDataSet ds = new RsiDataSet(stocks, DD, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(RANGE);
            ds.Prepare(0.60F, 0.2F);

            //train count
            Assert.AreEqual(EXPECTED_TRAINCOUNT, Math.Floor((data.Length - (RANGE - 1)) * ds.TrainPercent));
            //valid count
            Assert.AreEqual(EXPECTED_VALIDCOUNT, Math.Floor((data.Length - (RANGE - 1)) * ds.ValidPercent));
            //test count
            Assert.AreEqual(EXPECTED_TESTCOUNT, data.Length - (RANGE - 1) - PREDICT_DAYS - EXPECTED_TRAINCOUNT - EXPECTED_VALIDCOUNT);

            Assert.AreEqual(ds.TrainDataX.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.TrainDataY.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.ValidCount, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(ds.TestCount, EXPECTED_TESTCOUNT);
            Assert.AreEqual(ds.TestDataY.Length, EXPECTED_TESTCOUNT);
#if NORMALIZE_FIRST
            Assert.IsTrue(ds.TestDataY[2][0] > 0.70 && ds.TestDataY[2][0] < 0.75);
#else
            Assert.IsTrue(ds.TestDataY[2][0] > 60 && ds.TestDataY[2][0] < 65);
#endif
            //assert FeatureLabel structure is equal to the original one
            var fl = ds.GetFeatureLabelDataSet();
            Assert.AreEqual(fl["features"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["features"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["features"].test.Length, EXPECTED_TESTCOUNT);
            Assert.AreEqual(fl["label"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["label"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["label"].test.Length, EXPECTED_TESTCOUNT);

#if NORMALIZE_FIRST
            //assert FeatureLabel values are the same of the original ones
            Assert.IsTrue(Math.Abs(fl["features"].train[3][0] - ds.TrainDataX[3][0]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["features"].train[3][1] - ds.TrainDataX[3][1]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["label"].test[2][0] - ds.TestDataY[2][0]) < 0.0000001);
#else
            double[][] normalized_TrainDataX = ds.Normalize(ds.TrainDataX);
            double[][] normalized_TestDataY = ds.Normalize(ds.TestDataY);

            Assert.IsTrue(Math.Abs(fl["features"].train[3][0] - normalized_TrainDataX[3][0]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["features"].train[3][1] - normalized_TrainDataX[3][1]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["label"].test[2][0] - normalized_TestDataY[2][0]) < 0.0000001);
#endif
        }


    }
}
