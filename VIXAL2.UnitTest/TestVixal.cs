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
            const int TEST_COUNT = 10;


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
            Assert.AreEqual(ds.TestCount, TEST_COUNT);

            double[][] x = ds.GetTestArrayX().Values; //ds.GetTestArrayExtendedX_ForUnitTest().Values;
            Assert.AreEqual(x.Length, TEST_COUNT);

            Assert.AreEqual(x[0][0], 0.81);
            Assert.AreEqual(x[0][1], 0.81);
            Assert.IsTrue(0.62 < x[0][2] && x[0][2] < 0.63);

            Assert.AreEqual(x[TEST_COUNT-1][0], 0.9);
            Assert.AreEqual(x[TEST_COUNT-1][1], 0.9);
            Assert.IsTrue(0.63 < x[9][2] && x[9][2] < 0.64);

            TimeSerieArray current = ds.GetColumnData(0);
            TimeSerieArray future = ds.GetColumnData(0, PREDICT_DAYS);

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

            TimeSerieArray current = ds.GetTestArrayX();
            TimeSerieArray future = ds.GetTestArrayY();
            Assert.AreEqual(current.Length, future.Length);
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

            TimeSerieArray current = ds.GetTestArrayX();

            DateTime mydate = current.MaxDate.AddDays(-15);
            double value1 = current.GetValue(mydate, COLUMN_TO_CHECK);
            value1 = ds.Decode(value1, COLUMN_TO_CHECK);

            double value2 = ds.OriginalData.GetValue(mydate, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, value2);

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, 1, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, data1[0]);

            double[] data2 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, 10, COLUMN_TO_CHECK);
            Assert.AreEqual(value1, data2[9]);
            Assert.AreEqual(13.480000, data2[8]);
        }

        [TestMethod]
        public void Test_StockDataset_ExtendedArrayX()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            StocksDataset ds = GetDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 314);
            Assert.AreEqual(ds.ValidCount, 105);
            Assert.AreEqual(ds.TestCount, 95);

            TimeSerieArray current = ds.GetTestArrayX();
            TimeSerieArray extended = ds.GetExtendedArrayX();
            Assert.AreEqual(current.MaxDate.AddDays(1), extended.MinDate);
        }


        [TestMethod]
        public void Test_StockDataset_GetTrainArrayY()
        {
            const int PREDICT_DAYS = 10;

            StocksDataset ds = GetDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 314);
            Assert.AreEqual(ds.ValidCount, 105);
            Assert.AreEqual(ds.TestCount, 95);

            //checks lenght of 2 arrays are the same
            var futureTrain1 = ds.TrainDataY;
            var futureTrain2 = ds.GetTrainArrayY();
            Assert.AreEqual(futureTrain1.Length, futureTrain2.Length);
            //checks first, last and a middle values are the same
            Assert.AreEqual(futureTrain1[0][0], futureTrain2.Values[0][0]);
            Assert.AreEqual(futureTrain1[3][0], futureTrain2.Values[3][0]);
            Assert.AreEqual(futureTrain1[futureTrain1.Length-1][0], futureTrain2.Values[futureTrain1.Length - 1][0]);
            Assert.AreEqual(futureTrain1[0][1], futureTrain2.Values[0][1]);
            Assert.AreEqual(futureTrain1[4][1], futureTrain2.Values[4][1]);
            Assert.AreEqual(futureTrain1[futureTrain1.Length - 1][1], futureTrain2.Values[futureTrain1.Length - 1][1]);
            //checks first, last and a middle dates are correct
            Assert.AreEqual(ds.Dates[0], futureTrain2.GetDate(0));
            Assert.AreEqual(ds.Dates[5], futureTrain2.GetDate(5));
            Assert.AreEqual(ds.Dates[ds.TrainCount-1], futureTrain2.GetDate(futureTrain2.Length-1));

            //checks lenght of 2 arrays are the same
            var futureValidate1 = ds.ValidDataY;
            var futureValidate2 = ds.GetValidArrayY();
            Assert.AreEqual(futureValidate1.Length, futureValidate2.Length);
            //checks first, last and a middle value are the same
            Assert.AreEqual(futureValidate1[0][0], futureValidate2.Values[0][0]);
            Assert.AreEqual(futureValidate1[3][0], futureValidate2.Values[3][0]);
            Assert.AreEqual(futureValidate1[futureValidate1.Length - 1][0], futureValidate2.Values[futureValidate2.Length - 1][0]);
            Assert.AreEqual(futureValidate1[0][1], futureValidate2.Values[0][1]);
            Assert.AreEqual(futureValidate1[4][1], futureValidate2.Values[4][1]);
            Assert.AreEqual(futureValidate1[futureValidate1.Length - 1][1], futureValidate2.Values[futureValidate2.Length - 1][1]);
            //checks first, last and a middle dates are correct
            Assert.AreEqual(ds.Dates[0 + ds.TrainCount], futureValidate2.GetDate(0));
            Assert.AreEqual(ds.Dates[5 + ds.TrainCount], futureValidate2.GetDate(5));
            Assert.AreEqual(ds.Dates[ds.TrainCount+ds.ValidCount - 1], futureValidate2.GetDate(futureValidate2.Length - 1));
            //checks that train and validate arrays dates are continguos
            DateTime dateLastTrain = futureTrain2.MaxDate;
            DateTime dateFirstValidate = futureValidate2.MinDate;
            Assert.AreEqual(dateLastTrain.AddDays(1), dateFirstValidate);
        }

        [TestMethod]
        public void Test_MovingAverageDataset_GetTrainArrayY()
        {
            const int PREDICT_DAYS = 10;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare();

            Assert.AreEqual(ds.TrainCount, 303);
            Assert.AreEqual(ds.ValidCount, 101);
            Assert.AreEqual(ds.TestCount, 91);

            //checks lenght of 2 arrays are the same
            var futureTrain1 = ds.TrainDataY;
            var futureTrain2 = ds.GetTrainArrayY();
            Assert.AreEqual(futureTrain1.Length, futureTrain2.Length);
            //checks first, last and a middle values are the same
            Assert.AreEqual(futureTrain1[0][0], futureTrain2.Values[0][0]);
            Assert.AreEqual(futureTrain1[3][0], futureTrain2.Values[3][0]);
            Assert.AreEqual(futureTrain1[futureTrain1.Length - 1][0], futureTrain2.Values[futureTrain1.Length - 1][0]);
            Assert.AreEqual(futureTrain1[0][1], futureTrain2.Values[0][1]);
            Assert.AreEqual(futureTrain1[4][1], futureTrain2.Values[4][1]);
            Assert.AreEqual(futureTrain1[futureTrain1.Length - 1][1], futureTrain2.Values[futureTrain1.Length - 1][1]);
            //checks first, last and a middle dates are correct
            Assert.AreEqual(ds.Dates[0], futureTrain2.GetDate(0));
            Assert.AreEqual(ds.Dates[5], futureTrain2.GetDate(5));
            Assert.AreEqual(ds.Dates[ds.TrainCount - 1], futureTrain2.GetDate(futureTrain2.Length - 1));

            //checks lenght of 2 arrays are the same
            var futureValidate1 = ds.ValidDataY;
            var futureValidate2 = ds.GetValidArrayY();
            Assert.AreEqual(futureValidate1.Length, futureValidate2.Length);
            //checks first, last and a middle value are the same
            Assert.AreEqual(futureValidate1[0][0], futureValidate2.Values[0][0]);
            Assert.AreEqual(futureValidate1[3][0], futureValidate2.Values[3][0]);
            Assert.AreEqual(futureValidate1[futureValidate1.Length - 1][0], futureValidate2.Values[futureValidate2.Length - 1][0]);
            Assert.AreEqual(futureValidate1[0][1], futureValidate2.Values[0][1]);
            Assert.AreEqual(futureValidate1[4][1], futureValidate2.Values[4][1]);
            Assert.AreEqual(futureValidate1[futureValidate1.Length - 1][1], futureValidate2.Values[futureValidate2.Length - 1][1]);
            //checks first, last and a middle dates are correct
            Assert.AreEqual(ds.Dates[0 + ds.TrainCount], futureValidate2.GetDate(0));
            Assert.AreEqual(ds.Dates[5 + ds.TrainCount], futureValidate2.GetDate(5));
            Assert.AreEqual(ds.Dates[ds.TrainCount + ds.ValidCount - 1], futureValidate2.GetDate(futureValidate2.Length - 1));
            //checks that train and validate arrays dates are continguos
            DateTime dateLastTrain = futureTrain2.MaxDate;
            DateTime dateFirstValidate = futureValidate2.MinDate;

            if (dateLastTrain.DayOfWeek == DayOfWeek.Friday)
            {
                //if last of train is Friday, the first of validate must be monday
                Assert.AreEqual(dateLastTrain.AddDays(3), dateFirstValidate);
            }
            else
            {
                Assert.AreEqual(dateLastTrain.AddDays(1), dateFirstValidate);
            }
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

            TimeSerieArray current = ds.GetTestArrayX();
            TimeSerieArray future = ds.GetTestArrayY();

            //arrayX e arrayY hanno la stessa lunghezza
            Assert.AreEqual(current.Length, future.Length);

            DateTime mydate = current.MaxDate.AddDays(-15);
            double value1 = current.GetValue(mydate, 1);
            value1 = ds.Decode(value1, 1);

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumn(mydate, ds.Range, COLUMN_TO_CHECK);

            //assert the moving average calculated at date "mydate" is correctly found
            //on original data
            Assert.AreEqual(value1, Utils.Mean(data1));
            //verifico che il decimo valore del current è uguale al primo del future
            Assert.AreEqual(current.Values[10][0], future.Values[0][0]);

            DateTime mynextdate = current.GetNextDate(mydate, PREDICT_DAYS).Value;
            double value2 = current.GetValue(mynextdate, COLUMN_TO_CHECK);
            double value3 = future.GetValue(mydate, COLUMN_TO_CHECK);
            Assert.AreEqual(value2, value3);
        }
    }
}
