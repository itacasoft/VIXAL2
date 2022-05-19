using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using VIXAL2.UnitTest.Data;

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

        private StocksDataset GetDataset(int predictDays)
        {
            const int FIRST_PREDICT = 0;
            const int PREDICT_COUNT = 2;

            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            StocksDataset ds = new StocksDataset(stockNames, dates, data, FIRST_PREDICT, PREDICT_COUNT);
            ds.PredictDays = predictDays;

            return ds;
        }

        private MovingAverageDataSet GetMovingAverageDataset(int predictDays)
        {
            const int FIRST_PREDICT = 0;
            const int PREDICT_COUNT = 2;

            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            MovingAverageDataSet ds = new MovingAverageDataSet(stockNames, dates, data, FIRST_PREDICT, PREDICT_COUNT);
            ds.PredictDays = predictDays;

            return ds;
        }


        [TestMethod]
        public void Test_StockDataset_GetTestArrayExtendedX()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            StocksDataset ds = GetDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 314);
            Assert.AreEqual(ds.ValidCount, 105);
            Assert.AreEqual(ds.TestCount, 95);

            TimeSerieArray current = ds.GetTestArrayExtendedX();
            TimeSerieArray future = ds.GetTestArrayY();
            Assert.AreEqual(current.Length, future.Length + PREDICT_DAYS);
            Assert.AreEqual(future.Length, ds.TestDataY.Length);
            Assert.AreEqual(future.Values[0][0], ds.TestDataY[0][0]);
            Assert.AreEqual(future.Values[3][0], ds.TestDataY[3][0]);
            Assert.AreEqual(future.Values[future.Length-1][0], ds.TestDataY[future.Length-1][0]);

            DateTime mydate = current.MaxDate.AddDays(-15);

            double value3 = future.GetValue(mydate, COLUMN_TO_CHECK);
            DateTime mydate2 = current.GetNextDate(mydate, PREDICT_DAYS).Value;
            double value4 = current.GetValue(mydate2, COLUMN_TO_CHECK);

            Assert.AreEqual(value3, value4);
        }


        [TestMethod]
        public void Test_StockDataset_OriginalData()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            StocksDataset ds = GetDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 314);
            Assert.AreEqual(ds.ValidCount, 105);
            Assert.AreEqual(ds.TestCount, 95);

            TimeSerieArray current = ds.GetTestArrayExtendedX();

            DateTime mydate = current.MaxDate.AddDays(-15);
            double value1 = current.GetValue(mydate, COLUMN_TO_CHECK);
            value1 = ds.Decode(value1, COLUMN_TO_CHECK);

            double value2 = ds.OriginalData.GetValue(mydate, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, value2);

            Assert.AreEqual(ds.OriginalData.MaxDate, current.MaxDate);

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, 1, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, data1[0]);

            double[] data2 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, 10, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, data2[9]);
            Assert.AreEqual(13.470000, data2[8]);
        }

        [TestMethod]
        public void Test_MovingAverageDataset_OriginalData()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 303);
            Assert.AreEqual(ds.ValidCount, 101);
            Assert.AreEqual(ds.TestCount, 91);

            TimeSerieArray current = ds.GetTestArrayExtendedX();

            TimeSerieArray future = ds.GetTestArrayY();

            DateTime mydate = current.MaxDate.AddDays(-15);
            double value1 = current.GetValue(mydate, 1);
            value1 = ds.Decode(value1, 1);

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, ds.Range, COLUMN_TO_CHECK);

            //assert the moving average calculated at date "mydate" is correctly found
            //on original data
            Assert.AreEqual(value1, Utils.Mean(data1));

            double value2 = future.GetValue(mydate, COLUMN_TO_CHECK);
            DateTime myfuturedate = current.GetNextDate(mydate, PREDICT_DAYS).Value;

            double value3 = current.GetValue(myfuturedate, COLUMN_TO_CHECK);
            Assert.AreEqual(value2, value3);
        }
    }
}
