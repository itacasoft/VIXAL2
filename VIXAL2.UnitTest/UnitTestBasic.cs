using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SharpML.Types;
using VIXAL2.UnitTest.Data;
using System.Globalization;
using VIXAL2.Data;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class UnitTestBasic
    {
        [TestMethod]
        public void TestOaDates()
        {
            int[] dates = EnergyData.OaDates;

            Assert.AreEqual(dates.Length, EnergyData.Dates.Length);
            Assert.AreEqual(EnergyData.Dates[0], "2020-03-08");
            Assert.AreEqual(EnergyData.Dates[dates.Length - 1], "2022-03-08");
            Assert.AreEqual(dates[0], 43898);
            Assert.AreEqual(dates[dates.Length - 1], 44628);
        }

        [TestMethod]
        public void TestStocksMissingDates()
        {
            StockList stocks = new StockList();
            Stock aa = new Stock(PriceSource.NasDaq, "AA", Convert.ToDateTime("2022-02-01", CultureInfo.InvariantCulture));
            aa.Prices.Add(Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture), 1.1);
            aa.Prices.Add(Convert.ToDateTime("2022-01-02", CultureInfo.InvariantCulture), 1.2);
            aa.Prices.Add(Convert.ToDateTime("2022-01-03", CultureInfo.InvariantCulture), 1.3);

            Stock bb = new Stock(PriceSource.NasDaq, "BB", Convert.ToDateTime("2022-02-01", CultureInfo.InvariantCulture));
            bb.Prices.Add(Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture), 2.1);
            bb.Prices.Add(Convert.ToDateTime("2022-01-03", CultureInfo.InvariantCulture), 2.2);
            bb.Prices.Add(Convert.ToDateTime("2022-01-04", CultureInfo.InvariantCulture), 2.3);

            Stock cc = new Stock(PriceSource.NasDaq, "CC", Convert.ToDateTime("2022-02-01", CultureInfo.InvariantCulture));
            cc.Prices.Add(Convert.ToDateTime("2022-01-05", CultureInfo.InvariantCulture), 3.5);
            cc.Prices.Add(Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture), 3.1);
            cc.Prices.Add(Convert.ToDateTime("2022-01-03", CultureInfo.InvariantCulture), 3.3);
            cc.Prices.Add(Convert.ToDateTime("2022-01-04", CultureInfo.InvariantCulture), 3.4);
            cc.Prices.Add(Convert.ToDateTime("2022-01-02", CultureInfo.InvariantCulture), 3.2);

            Stock dd = new Stock(PriceSource.NasDaq, "DD", Convert.ToDateTime("2022-02-01", CultureInfo.InvariantCulture));
            dd.Prices.Add(Convert.ToDateTime("2022-01-02", CultureInfo.InvariantCulture), 4.2);
            dd.Prices.Add(Convert.ToDateTime("2022-01-03", CultureInfo.InvariantCulture), 4.3);
            dd.Prices.Add(Convert.ToDateTime("2022-01-04", CultureInfo.InvariantCulture), 4.4);

            Stock ee = new Stock(PriceSource.Yahoo, "EE", Convert.ToDateTime("2022-02-01", CultureInfo.InvariantCulture));
            ee.Prices.Add(Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture), 5.1);
            ee.Prices.Add(Convert.ToDateTime("2022-01-04", CultureInfo.InvariantCulture), 5.4);
            ee.Prices.Add(Convert.ToDateTime("2022-01-05", CultureInfo.InvariantCulture), 5.5);

            stocks.Add(aa);
            stocks.Add(bb);
            stocks.Add(cc);
            stocks.Add(dd);
            stocks.Add(ee);

            stocks.AlignDates();
            Assert.AreEqual(stocks.MinDate, Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture));
            Assert.AreEqual(stocks.MaxDate, Convert.ToDateTime("2022-01-04", CultureInfo.InvariantCulture));
            Assert.AreEqual(stocks[1].Symbol, "BB");
            Assert.AreEqual(stocks[1].Prices[Convert.ToDateTime("2022-01-02", CultureInfo.InvariantCulture)], 2.1);
            Assert.AreEqual(stocks[2].Symbol, "CC");
            Assert.AreEqual(stocks[2].Prices.Count, 4);
            Assert.AreEqual(stocks[3].Symbol, "DD");
            Assert.AreEqual(stocks[3].Prices.Count, 4);
            Assert.AreEqual(stocks[3].Prices[Convert.ToDateTime("2022-01-01", CultureInfo.InvariantCulture)], double.NaN);
            Assert.AreEqual(stocks[4].Prices[Convert.ToDateTime("2022-01-02", CultureInfo.InvariantCulture)], 5.1);
        }


        [TestMethod]
        public void TestMovingAverage()
        {
            double[] MM = {
            1.00,
            2.00,
            3.00,
            4.00,
            5.00,
            6.00,
            7.00,
            8.00,
            9.00,
            10.00};

            int range = 3;
            double[] input = (double[])MM.Clone();

            double[] output = MovingAverageDataSet.GetMovingAverage(input, range);
            Assert.AreEqual(output.Length, input.Length);
            Assert.IsTrue(output[0] == 1 || double.IsNaN(output[0]));
            Assert.IsTrue(output[1] == 1.5 || double.IsNaN(output[1]));
            Assert.AreEqual(output[2], 2);
            Assert.AreEqual(output[3], 3);
        }

        [TestMethod]
        public void TestMovingAverageArray()
        {
            double[][] input =
            {
                new double[] { 1, 0, 1 },
                new double[] { 2, 2, 3 },
                new double[] { 3, 4, 5 },
                new double[] { 4, 6, 7 },
                new double[] { 5, 8, 9 },
                new double[] { 6, 10, 11 },
                new double[] { 7, 12, 13 },
                new double[] { 8, 14, 15 },
                new double[] { 9, 16, 17 },
                new double[] { 10, 18, 19 }
            };

            int range = 3;
            double[][] output = MovingAverageDataSet.GetMovingAverage(input, range);
            Assert.AreEqual(output.Length, input.Length);
            Assert.IsTrue(output[0][0] == 1 || double.IsNaN(output[0][0]));
            Assert.IsTrue(output[1][0] == 1.5 || double.IsNaN(output[1][0]));
            Assert.AreEqual(output[2][0], 2);
            Assert.AreEqual(output[3][0], 3);
            Assert.AreEqual(output[3][1], 4);
        }

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

            for (int i=0; i<data.Length; i++)
            {
                data[i] = new double[2];
                data[i][0] = EnergyData.Eni[i];
                data[i][1] = EnergyData.Brent[i];
            }

            Assert.AreEqual(data.Length, DATA_LENGHT);

            DateTime[] DD = new DateTime[data.Length];
            //fill date array
            DD[0] = DateTime.Parse("01/01/2010");
            for (int i= 1; i<data.Length; i++)
            {
                DD[i] = DD[i-1].AddDays(1);
            }

            string[] stocks =
            {
                "COMPANY_A",
                "COMPANY_B"
            };


            RsiDataSet ds = new RsiDataSet(stocks, DD, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.Range = RANGE;
            ds.Prepare();

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
            Assert.IsTrue(ds.TestDataY[2][0] > 0.70 && ds.TestDataY[2][0] < 0.72);

            //assert FeatureLabel structure is equal to the original one
            var fl = ds.GetFeatureLabelDataSet();
            Assert.AreEqual(fl["features"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["features"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["features"].test.Length, EXPECTED_TESTCOUNT);
            Assert.AreEqual(fl["label"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["label"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["label"].test.Length, EXPECTED_TESTCOUNT);

            //assert FeatureLabel values are the same of the original ones
            Assert.IsTrue(Math.Abs(fl["features"].train[3][0] - ds.TrainDataX[3][0]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["features"].train[3][1] - ds.TrainDataX[3][1]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["label"].test[2][0] - ds.TestDataY[2][0]) < 0.0000001);
        }


        [TestMethod]
        public void TestMovingForwardDataSet()
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


            MovingForwardDataSet ds = new MovingForwardDataSet(stocks, DD, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.Range = RANGE;
            ds.Prepare();

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
            Assert.IsTrue(ds.TestDataY[2][0] > 0.70 && ds.TestDataY[2][0] < 0.72);

            //assert FeatureLabel structure is equal to the original one
            var fl = ds.GetFeatureLabelDataSet();
            Assert.AreEqual(fl["features"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["features"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["features"].test.Length, EXPECTED_TESTCOUNT);
            Assert.AreEqual(fl["label"].train.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(fl["label"].valid.Length, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(fl["label"].test.Length, EXPECTED_TESTCOUNT);

            //assert FeatureLabel values are the same of the original ones
            Assert.IsTrue(Math.Abs(fl["features"].train[3][0] - ds.TrainDataX[3][0]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["features"].train[3][1] - ds.TrainDataX[3][1]) < 0.0000001);
            Assert.IsTrue(Math.Abs(fl["label"].test[2][0] - ds.TestDataY[2][0]) < 0.0000001);
        }

    }
}

