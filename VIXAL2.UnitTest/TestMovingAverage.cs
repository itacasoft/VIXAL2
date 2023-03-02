using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using System;
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
        public void Test_MovingAverageDataset_MeanCalculation()
        {
            const int PREDICT_DAYS = 10;
            const int COLUMN_TO_CHECK = 1;

            MovingAverageDataSet ds = GetMovingAverageDataset(PREDICT_DAYS);
            ds.Prepare();

            int range = ds.Range;
            int count = ds.OriginalData.Length;
            double validPerc = ds.ValidPercent;
            double trainPerc = ds.TrainPercent;

            Assert.AreEqual(ds.TrainCount,Convert.ToInt32((count - range + 1)*trainPerc));

            TimeSerieArray current = ds.GetTestArrayX();

            DateTime mydate = current.MaxDate.AddDays(-15);
            double value1 = current.GetValue(mydate, 1);
            value1 = Math.Round(value1, 2, MidpointRounding.AwayFromZero);

#if NORMALIZE_FIRST
            value1 = ds.Decode(value1, 1);
#endif

            double[] data1 = ds.OriginalData.GetPreviousValuesFromColumnIncludingCurrent(mydate, ds.Range, COLUMN_TO_CHECK);

            //assert the moving average calculation is correct
            //and that calculated at date "mydate" is correctly found on original data

            double mean1 = Math.Round(Utils.Mean(data1), 2, MidpointRounding.AwayFromZero);
            Assert.AreEqual(value1, mean1);
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
            ds.Prepare();

            Assert.AreEqual(ds.DelayDays, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(9);
            ds.Prepare();

            Assert.AreEqual(ds.DelayDays, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(3);
            ds.Prepare();

            Assert.AreEqual(ds.DelayDays, GetDelayDaysCalculatedFromDates(ds));

            ds = GetMovingEnhancedAverageDataset(40);
            ds.SetRange(1);
            ds.Prepare();

            Assert.AreEqual(ds.DelayDays, GetDelayDaysCalculatedFromDates(ds));

            var ds1 = GetMovingAverageDataset(40);
            ds1.SetRange(10);
            ds1.Prepare();
            var gg1 = GetDelayDaysCalculatedFromDates(ds1);

            Assert.AreEqual(ds1.DelayDays, gg1);

        }

        [TestMethod]
        public void Test_MovingAverageDataset_Reverse()
        {
            const int PREDICT_DAYS = 20;
            const int FIRST_PREDICT = 1;

            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            MovingAverageDataSet ds = new MovingAverageDataSet(stockNames, dates, data, FIRST_PREDICT, 1);
            ds.PredictDays = PREDICT_DAYS;
            ds.SetRange(10);
            ds.Prepare();

            DateTime myDate = ds.MaxDate;

            //creo una lista di valori predicted
            List<DatedValue> listPredicted = new List<DatedValue>();
            listPredicted.Add(new DatedValue(Utils.AddBusinessDays(myDate, 1), 13.6));
            listPredicted.Add(new DatedValue(Utils.AddBusinessDays(myDate, 2), 13.5));
            listPredicted.Add(new DatedValue(Utils.AddBusinessDays(myDate, 3), 13.4));
            listPredicted.Add(new DatedValue(Utils.AddBusinessDays(myDate, 4), 13.3));

            //estraggo la lista dei valori reverse
            var reverse = ds.GetReverseMovingAverageValues(listPredicted);

#if NORMALIZE_FIRST
            value1 = ds.Decode(value1, 1);
#endif
            //verifico che si tratti di valori corretti e che le date coincidano
            Assert.IsTrue(reverse[0].Value > 12 && reverse[0].Value < 13);
            Assert.AreEqual(reverse[0].Date, listPredicted[0].Date);

            Assert.IsTrue(reverse[1].Value > 12 && reverse[1].Value < 13);
            Assert.AreEqual(reverse[1].Date, listPredicted[1].Date);

            Assert.IsTrue(reverse[3].Value > 12 && reverse[3].Value < 13);
            Assert.AreEqual(reverse[3].Date, listPredicted[3].Date);
        }
    }
}
