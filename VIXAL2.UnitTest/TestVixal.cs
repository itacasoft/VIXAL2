using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using VIXAL2.Data;
using VIXAL2.Data.Base;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestVixal
    {
        [TestMethod]
        public void TestTimeSerieDataset_split()
        {
            const int COUNT = 101;
            const int PREDICT_DAYS = 10;
            const int FIRST_PREDICT = 0;
            const int PREDICT_COUNT = 1;
            const int TESTXEXTENDED_COUNT = 20;

            List<DateTime> dates = new List<DateTime>();
            DateTime firstDate = Convert.ToDateTime("2022-01-05", CultureInfo.InvariantCulture);
            dates.Add(firstDate);
            for (int i = 1; i < 101; i++)
                dates.Add(firstDate.AddDays(i));

            double[][] data = new double[COUNT][];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new double[3];
                data[i][0] = i + 1;
                data[i][1] = (i + 1) * 2;
                data[i][2] = Data.EnergyData.Eni[i];
            }

            //each element 0 is half of the corresponding element 1
            Assert.AreEqual(data[2][0] * 2, data[2][1]);
            //the third column has real data
            Assert.AreNotEqual(data[2][0], data[2][2]);
            Assert.AreNotEqual(data[2][1], data[2][2]);

            List<string> keys = new List<string>();
            keys.Add("Key1");
            keys.Add("Key2");

            TimeSerieDataSet ds = new TimeSerieDataSet(keys.ToArray(), dates.ToArray(), data, FIRST_PREDICT, PREDICT_COUNT);
            ds.PredictDays = PREDICT_DAYS;
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 61);
            Assert.AreEqual(ds.ValidCount, 20);
            Assert.AreEqual(ds.TestCount, 10);

            double[][] x = ds.GetTestArrayExtendedX().Values;
            Assert.AreEqual(x.Length, TESTXEXTENDED_COUNT);

            Assert.AreEqual(x[0][0], 0.81);
            Assert.AreEqual(x[0][1], 0.81);
            Assert.IsTrue(0.62 < x[0][2] && x[0][2] < 0.63);

            Assert.AreEqual(x[12][0], 0.93);
            Assert.AreEqual(x[12][1], 0.93);
            Assert.IsTrue(0.73 < x[12][2] && x[12][2] < 0.74);

            Assert.AreEqual(x[TESTXEXTENDED_COUNT-1][0], 1);
            Assert.AreEqual(x[TESTXEXTENDED_COUNT-1][1], 1);
            Assert.IsTrue(0.68 < x[TESTXEXTENDED_COUNT-1][2] && x[TESTXEXTENDED_COUNT-1][2] < 0.69);

            TimeSerieArray current = ds.GetColumnData(0);
            TimeSerieArray future = ds.GetColumnData(0, 10);

            Assert.AreEqual(future.Length, current.Length - PREDICT_DAYS);
            Assert.AreEqual(future.MinDate, current.MinDate.AddDays(PREDICT_DAYS));
            Assert.AreEqual(future.MaxDate, current.MaxDate);
        }

        [TestMethod]
        public void StockDataset_PredictionResult()
        {
            const int COUNT = 101;
            const int PREDICT_DAYS = 10;
            const int FIRST_PREDICT = 0;
            const int PREDICT_COUNT = 2;

            List<DateTime> dates = new List<DateTime>();
            DateTime firstDate = Convert.ToDateTime("2022-01-05", CultureInfo.InvariantCulture);
            dates.Add(firstDate);
            for (int i = 1; i < 101; i++)
                dates.Add(firstDate.AddDays(i));

            double[][] data = new double[COUNT][];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new double[2];
                data[i][0] = i + 1;
                data[i][1] = (i + 1) * 2;
            }

            List<string> stockNames = new List<string>();
            stockNames.Add("Company A");
            stockNames.Add("Company B");

            StocksDataset ds = new StocksDataset(stockNames.ToArray(), dates.ToArray(), data, FIRST_PREDICT, PREDICT_COUNT);
            ds.PredictDays = PREDICT_DAYS;
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 61);
            Assert.AreEqual(ds.ValidCount, 20);
            Assert.AreEqual(ds.TestCount, 10);

            TimeSerieArray current = ds.GetTestArrayExtendedX();
            TimeSerieArray future = ds.GetTestArrayY();
            Assert.AreEqual(current.Length, future.Length + PREDICT_DAYS);
            Assert.AreEqual(future.Length, ds.TestDataY.Length);
            Assert.AreEqual(future.Values[0][0], ds.TestDataY[0][0]);
            Assert.AreEqual(future.Values[3][0], ds.TestDataY[3][0]);
            Assert.AreEqual(future.Values[future.Length-1][0], ds.TestDataY[future.Length-1][0]);

            ds.PredictedAlloc(current.Length);

            int row;
            DateTime myDate = DateTime.MinValue;
            for (row = 0; row < future.Length; row++)
            {
                for (int col = 0; col < future.Columns; col++)
                {
                    myDate = future.GetDate(row);
                    ds.PredictedSetValue(row, col, myDate, future.GetValue(myDate));
                }
            }

            ds.PredictedSetValue(row, 0, myDate.AddDays(1), 667);
            ds.PredictedSetValue(row, 1, myDate.AddDays(1), 668);
            ds.PredictedSetValue(row+1, 0, myDate.AddDays(2), 669);
            ds.PredictedSetValue(row+1, 1, myDate.AddDays(2), 670);

            Assert.AreEqual(ds.PredictedGetValue(myDate.AddDays(1), 0), 667);
            Assert.AreEqual(ds.PredictedGetValue(row, 0), 667);
            Assert.AreEqual(ds.PredictedGetValue(myDate.AddDays(1), 1), 668);
            Assert.AreEqual(ds.PredictedGetValue(row, 1), 668);

            Assert.AreEqual(ds.PredictedGetValue(myDate.AddDays(2), 0), 669);
            Assert.AreEqual(ds.PredictedGetValue(row+1, 0), 669);
            Assert.AreEqual(ds.PredictedGetValue(myDate.AddDays(2), 1), 670);
            Assert.AreEqual(ds.PredictedGetValue(row + 1, 1), 670);

            double[] results = ds.CompareAgainstPredicted(future);
            Assert.AreEqual(results.Length, 2);

            Assert.AreEqual(results[0], 1);
            Assert.AreEqual(results[1], 1);
        }
    }
}
