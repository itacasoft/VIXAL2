using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using VIXAL2.UnitTest.Data;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestMovingAverage
    {
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

        private MovingEnhancedAverageDataSet GetMovingEnhancedAverageDataset(int predictDays)
        {
            const int FIRST_PREDICT = 0;
            const int PREDICT_COUNT = 2;

            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            var ds = new MovingEnhancedAverageDataSet(stockNames, dates, data, FIRST_PREDICT, PREDICT_COUNT);
            ds.PredictDays = predictDays;

            return ds;
        }


        [TestMethod]
        public void TestMovingAverageArray1()
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
        public void TestMovingAverageArray2()
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
        public void TestMovingAverageDataSet()
        {
            const int EXPECTED_TRAINCOUNT = 52;
            const int EXPECTED_VALIDCOUNT = 17;
            const int EXPECTED_TESTCOUNT = 8;
            const int RANGE = 14;
            const int FIRST_PREDICT = 0;
            const int PREDICT_DAYS = 10;
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


            MovingAverageDataSet ds = new MovingAverageDataSet(stocks, DD, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(RANGE);
            ds.Prepare(EXPECTED_VALIDCOUNT, EXPECTED_TESTCOUNT);

            //test count
            Assert.AreEqual(EXPECTED_TESTCOUNT, data.Length - (RANGE - 1) - PREDICT_DAYS - EXPECTED_TRAINCOUNT - EXPECTED_VALIDCOUNT);

            Assert.AreEqual(ds.TrainDataX.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.TrainDataY.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.ValidCount, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(ds.TestCount, EXPECTED_TESTCOUNT);
            Assert.AreEqual(ds.TestDataY.Length, EXPECTED_TESTCOUNT);

#if NORMALIZE_FIRST
            Assert.IsTrue(ds.TestDataY[2][0] > 0.80 && ds.TestDataY[2][0] < 0.82);
#else
            Assert.IsTrue(ds.TestDataY[2][0] > 7 && ds.TestDataY[2][0] < 8);
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

        [TestMethod]
        public void TestMovingAverageForwardDataSet()
        {
            const int RANGE = 14;
            const int FIRST_PREDICT = 0;
            const int PREDICT_DAYS = 10;
            const int EXPECTED_TRAINCOUNT = 70;
            const int EXPECTED_VALIDCOUNT = 0;
            const int EXPECTED_TESTCOUNT = 7;
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


            MovingAverageDataSet ds = new MovingAverageDataSet(stocks, DD, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(RANGE);
            ds.Prepare(EXPECTED_VALIDCOUNT, EXPECTED_TESTCOUNT);

            //test count
            Assert.AreEqual(EXPECTED_TESTCOUNT, data.Length - (RANGE - 1) - PREDICT_DAYS - EXPECTED_TRAINCOUNT - EXPECTED_VALIDCOUNT);

            Assert.AreEqual(ds.TrainDataX.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.TrainDataY.Length, EXPECTED_TRAINCOUNT);
            Assert.AreEqual(ds.ValidCount, EXPECTED_VALIDCOUNT);
            Assert.AreEqual(ds.TestCount, EXPECTED_TESTCOUNT);
            Assert.AreEqual(ds.TestDataY.Length, EXPECTED_TESTCOUNT);
            double valueToCheck = ds.TestDataX[0][0];

#if NORMALIZE_FIRST
            Assert.IsTrue(valueToCheck > 0.82 && valueToCheck < 0.84);
#else
            Assert.IsTrue(valueToCheck > 8.0 && valueToCheck < 8.1);
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

            bool result = ds.Forward(1);
            Assert.IsTrue(result);
            Assert.AreEqual(ds.TrainDataX.Length, EXPECTED_TRAINCOUNT + 1);
            Assert.AreEqual(ds.TrainDataY.Length, EXPECTED_TRAINCOUNT + 1);
            Assert.AreEqual(ds.TestCount, EXPECTED_TESTCOUNT - 1);
            Assert.AreEqual(ds.TestDataY.Length, EXPECTED_TESTCOUNT - 1);
            Assert.AreEqual(ds.TrainDataX[70][0], valueToCheck);

            result = ds.Forward(6);
            Assert.IsFalse(result);
            Assert.AreEqual(ds.TestCount, 6);
        }


        [TestMethod]
        public void Test_MovingAverageDataset_GetTrainArrayY()
        {
            const int PREDICT_DAYS = 10;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare(50, 41);

            Assert.AreEqual(ds.TrainCount, 404);
            Assert.AreEqual(ds.ValidCount, 50);
            Assert.AreEqual(ds.TestCount, 41);

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

        TimeSerieArray GetSimpleTimeSerieArray(DateTime minDate)
        {
            string[] stockNames = new string[1];
            stockNames[0] = "MyStock";

            DateTime[] dates = new DateTime[15];
            dates[0] = minDate;
            for (int i = 1; i < dates.Length; i++)
            {
                dates[i] = dates[i - 1].AddDays(1);
            }

            double[][] data = new double[15][];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new double[1];
                data[i][0] = i + 1;
            }

            var result = new TimeSerieArray(stockNames, dates, data);
            return result;
        }

        [TestMethod]
        public void Test_MovingAverageDataset_GetPreviousValues()
        {
            DateTime minDate = Convert.ToDateTime("2022-05-01");

            var tarray = GetSimpleTimeSerieArray(minDate);
            var value1 = tarray.GetValue(minDate.AddDays(10), 0);
            Assert.AreEqual(value1, tarray[10][0]);

            var data = tarray.GetPreviousValuesFromColumn(minDate.AddDays(10), 10, 0);

            Assert.AreEqual(data.Length, 10);
            Assert.AreEqual(data[0], 1);
            Assert.AreNotEqual(value1, data[data.Length - 1]);
        }

        [TestMethod]
        public void Test_MovingAverageDataset_GetNextValues()
        {
            DateTime minDate = Convert.ToDateTime("2022-05-01");

            var tarray = GetSimpleTimeSerieArray(minDate);
            var value1 = tarray.GetValue(minDate.AddDays(1), 0);
            Assert.AreEqual(value1, tarray[1][0]);

            var data = tarray.GetNextValuesFromColumn(minDate.AddDays(1), 10, 0);

            Assert.AreEqual(data.Length, 10);
            Assert.AreEqual(data[0], 3);
            Assert.AreEqual(data[9], 12);
            Assert.AreNotEqual(value1, data[0]);
        }

        [TestMethod]
        public void Test_MovingAverageDataset_MeanCalculation_Simple()
        {
            const int PREDICT_DAYS = 5;
            const int COLUMN_TO_CHECK = 0;
            const int RANGE = 5;

            DateTime[] dates = EnergyData.Dates.Take(20).ToArray();
            double[] data = EnergyData.Eni.Take(20).ToArray();
            string stockName = "ENI";
            Assert.AreEqual(dates.Length, data.Length);
            //Assert.AreEqual(stockNames.Length, data[0].Length);

            MovingAverageDataSet ds = new MovingAverageDataSet(stockName, dates, data);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(RANGE);
            ds.Prepare(0, 4);

            TimeSerieArray current = ds.GetTestArrayX();

            DateTime mydate = current.MaxDate; //26-03
            double value1 = current.GetValue(mydate, COLUMN_TO_CHECK);

#if NORMALIZE_FIRST
            value1 = ds.Decode(value1, 1);
#endif

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumnIncludingCurrent(mydate, ds.Range, COLUMN_TO_CHECK);

            //assert the moving average calculation is correct
            //and that calculated at date "mydate" is correctly found on original data

            double mean1 = Utils.Mean(data1);
            Assert.AreEqual(Math.Round(value1, 2, MidpointRounding.AwayFromZero), Math.Round(mean1, 2, MidpointRounding.AwayFromZero));

        }

        [TestMethod]
        public void Test_MovingAverageDataset_MeanCalculation()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 2;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare(0, 30);

            int range = ds.Range;
            int count = ds.OriginalData.Length;

            TimeSerieArray current = ds.GetTestArrayX();

            DateTime mydate = current.MaxDate.AddDays(-15); //07-02
            double value1 = current.GetValue(mydate, COLUMN_TO_CHECK);

#if NORMALIZE_FIRST
            value1 = ds.Decode(value1, 1);
#endif

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumnIncludingCurrent(mydate, ds.Range, COLUMN_TO_CHECK);

            //assert the moving average calculation is correct
            //and that calculated at date "mydate" is correctly found on original data

            double mean1 = Utils.Mean(data1);
            Assert.AreEqual(Math.Round(value1, 2, MidpointRounding.AwayFromZero), Math.Round(mean1, 2, MidpointRounding.AwayFromZero));
        }

        [TestMethod]
        public void Test_MovingAverageDataset_OriginalData()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare(101, 91);

            Assert.AreEqual(ds.TrainCount, 303);
            Assert.AreEqual(ds.ValidCount, 101);
            Assert.AreEqual(ds.TestCount, 91);

            TimeSerieArray current = ds.GetTestArrayX();
            TimeSerieArray future = ds.GetTestArrayY();

            //arrayX e arrayY hanno la stessa lunghezza
            Assert.AreEqual(current.Length, future.Length);

            //verifico che il decimo valore del current è uguale al primo del future
            Assert.AreEqual(current.Values[10][0], future.Values[0][0]);

            DateTime mydate = current.MaxDate.AddDays(-15);
            DateTime mynextdate = current.GetNextDate(mydate, PREDICT_DAYS).Value;
            double value2 = current.GetValue(mynextdate, COLUMN_TO_CHECK);
            double value3 = future.GetValue(mydate, COLUMN_TO_CHECK);
            Assert.AreEqual(value2, value3);
        }

        [TestMethod]
        public void Test_MovingEnhancedAverageDataset_Pieces_Are_Centered()
        {
            double[] array2 = new double[] { 1, 3, 5, 7, 9, 15, 9, 7, 5 };
            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;

            var ds = new MovingEnhancedAverageDataSet(stockNames, dates, data, -1, -1);
            ds.SetRange(1);
            var pieces = ds.PiecesAround100;
            Assert.AreEqual(pieces.Count, 1);
            Assert.AreEqual(pieces[0], 100);

            ds.SetRange(2);
            pieces = ds.PiecesAround100;
            Assert.AreEqual(pieces.Count, 2);
            Assert.AreEqual(pieces[0], 99);
            Assert.AreEqual(pieces[1], 100);

            ds.SetRange(3);
            pieces = ds.PiecesAround100;
            Assert.AreEqual(pieces.Count, 3);
            Assert.AreEqual(pieces[0], 99);
            Assert.AreEqual(pieces[1], 100);
            Assert.AreEqual(pieces[2], 101);

            ds.SetRange(4);
            pieces = ds.PiecesAround100;
            Assert.AreEqual(pieces.Count, 4);
            Assert.AreEqual(pieces[0], 98);
            Assert.AreEqual(pieces[1], 99);
            Assert.AreEqual(pieces[2], 100);
            Assert.AreEqual(pieces[3], 101);

            ds.SetRange(5);
            pieces = ds.PiecesAround100;
            Assert.AreEqual(pieces.Count, 5);
            Assert.AreEqual(pieces[0], 98);
            Assert.AreEqual(pieces[1], 99);
            Assert.AreEqual(pieces[2], 100);
            Assert.AreEqual(pieces[3], 101);
            Assert.AreEqual(pieces[4], 102);
        }


        [TestMethod]
        public void Test_MovingEnhancedAverageDataset_EnhancedMovingAverageCalculation()
        {
            double[] array2 = new double[] { 1, 3, 5, 7, 9, 15, 9, 7, 5 };
            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;

            var ds = new MovingEnhancedAverageDataSet(stockNames, dates, data, -1, -1);
            ds.SetRange(1);

            var avg1 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg1.Length, array2.Length);
            Assert.AreEqual(avg1[0], 1);
            Assert.AreEqual(avg1[2], 5);
            Assert.AreEqual(avg1[4], 9);
            Assert.AreEqual(avg1[8], 5);

            ds.SetRange(2);
            var avg2 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg2.Length, array2.Length);
            Assert.AreEqual(avg2[0], double.NaN);
            Assert.AreEqual(avg2[2], (3.0 + 5.0) / 2.0);
            Assert.AreEqual(avg2[3], (5.0 + 7.0) / 2.0);
            Assert.AreEqual(avg2[4], (7.0 + 9.0) / 2.0);
            Assert.AreEqual(avg2[8], (7.0 + 5.0) / 2.0);

            ds.SetRange(3);
            var avg3 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg3.Length, array2.Length);
            Assert.AreEqual(avg3[0], double.NaN);
            Assert.AreEqual(avg3[1], (1.0 + 3.0 + 5.0) / 3.0);
            Assert.AreEqual(avg3[2], 5);
            Assert.AreEqual(avg3[5], 11);
            Assert.AreEqual(avg3[7], 7);
            Assert.AreEqual(avg3[8], double.NaN);
        }

        [TestMethod]
        public void Test_MovingEnhancedAverageDataset2_EnhancedMovingAverageCalculation()
        {
            double[] array2 = new double[] { 1, 3, 5, 7, 9, 15, 9, 7, 5 };
            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;

            var ds = new MovingEnhancedAverageDataSet2(stockNames, dates, data, -1, -1);
            ds.SetRange(1);
            var avg1 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg1.Length, array2.Length);
            Assert.AreEqual(avg1[0], 1);
            Assert.AreEqual(avg1[2], 5);
            Assert.AreEqual(avg1[4], 9);
            Assert.AreEqual(avg1[8], 5);

            ds.SetRange(2);
            var avg2 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg2.Length, array2.Length);
            Assert.AreEqual(avg2[0], 1);
            Assert.AreEqual(avg2[2], 4);
            Assert.AreEqual(avg2[4], (7.0 + 9.0) / 2.0);
            Assert.AreEqual(avg2[8], 6);

            ds.SetRange(3);
            var avg3 = ds.GetFutureMovingAverage(array2);
            Assert.AreEqual(avg3.Length, array2.Length);
            Assert.AreEqual(avg3[0], (1.0 + 3.0) / 2.0);
            Assert.AreEqual(avg3[1], (1.0 + 3.0 + 5.0) / 3.0);
            Assert.AreEqual(avg3[2], 5);
            Assert.AreEqual(avg3[5], 11);
            Assert.AreEqual(avg3[7], 7);
            Assert.AreEqual(avg3[8], 6);
        }

        private int GetDelayDaysCalculatedFromDates(StocksDataset ds)
        {
            int result = 0;
            for (int i = 0; i < ds.OriginalData.Dates.Length; i++)
            {
                if (ds.OriginalData.Dates[i] == ds.MinDate)
                {
                    break;
                }

                result++;
            }
            return result;
        }

        [TestMethod]
        public void Test_DelayDays_is_equal_as_calculated_from_days()
        {
            var ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(10);
            ds.Prepare(0, 30);

            Assert.AreEqual(ds.DaysGapAtStart, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(9);
            ds.Prepare(0, 30);

            Assert.AreEqual(ds.DaysGapAtStart, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(3);
            ds.Prepare(0, 30);

            Assert.AreEqual(ds.DaysGapAtStart, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(1);
            ds.Prepare(0, 30);

            Assert.AreEqual(ds.DaysGapAtStart, GetDelayDaysCalculatedFromDates(ds));

            var ds1 = GetMovingAverageDataset(40);
            ds1.SetRange(10);
            ds1.Prepare(0, 30);
            var gg1 = GetDelayDaysCalculatedFromDates(ds1);

            Assert.AreEqual(ds1.DaysGapAtStart, gg1);

        }

        [TestMethod]
        public void Test_MovingAverageDataset_Reverse()
        {
            const int RANGE = 5;
            double[] MyStock =
            {
                10,11,12,13,14,15,16,17,18,19,20,21,22,23
            };

            var movingAvg = MovingAverageDataSet.GetMovingAverage(MyStock, RANGE);
            movingAvg = movingAvg.Where(x => !double.IsNaN(x)).ToArray();

            var reverse = MovingAverageDataSet.GetReverseMovingAverage(movingAvg, RANGE, MyStock.Take(RANGE - 1).ToArray());

            Assert.AreEqual(MyStock.Length, reverse.Length);
            Assert.AreEqual(MyStock[0], reverse[0]);
            Assert.AreEqual(MyStock[3], reverse[3]);
            Assert.AreEqual(MyStock[MyStock.Length-1], reverse[MyStock.Length-1]);
        }

        [TestMethod]
        public void Test_MovingAverageDataset_Reverse2()
        {
            const int PREDICT_DAYS = 10;
            const int FIRST_PREDICT = 1;
            const int RANGE = 5;

            DateTime[] dates = EnergyData.Dates.Take(60).ToArray();
            double[][] data = EnergyData.AllData.Take(60).ToArray();
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            MovingAverageDataSet ds = new MovingAverageDataSet(stockNames, dates, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(RANGE);
            ds.Prepare(0, 30);

            DateTime myDate = ds.MaxDate;

            //prendo la lista di valori direttamente dal ds
            var mavg = ds.GetTrainArrayY();
            List<DatedValue> listAvg = new List<DatedValue>();
            for (int i = 0; i < mavg.Dates.Length; i++)
            {
                listAvg.Add(new DatedValue(mavg.FutureDates[i], mavg.Values[i][0]));
            }

            //estraggo la lista dei valori reverse
            var reverse = ds.GetReverseMovingAverageValues(listAvg);

#if NORMALIZE_FIRST
            value1 = ds.Decode(value1, 1);
#endif
            //verifico che si tratti di valori corretti e che le date coincidano
            Assert.AreEqual(reverse.Count, mavg.Length + RANGE-1);
            Assert.AreEqual(reverse[0].Date, ds.OriginalData.Dates[PREDICT_DAYS]);
            Assert.AreEqual(Math.Round(reverse[0].Value, 2), Math.Round(ds.OriginalData.Values[PREDICT_DAYS][1], 2));

            Assert.AreEqual(reverse[5].Date, ds.OriginalData.Dates[5 + PREDICT_DAYS]);
            Assert.AreEqual(Math.Round(reverse[5].Value,2), Math.Round(ds.OriginalData.Values[5+PREDICT_DAYS][1],2));

            Assert.AreEqual(reverse[reverse.Count-1].Date, ds.OriginalData.Dates[reverse.Count - 1 + PREDICT_DAYS]);
            Assert.AreEqual(Math.Round(reverse[reverse.Count - 1].Value,2), Math.Round(ds.OriginalData.Values[reverse.Count - 1 + PREDICT_DAYS][1],2));
        }
    }
}
