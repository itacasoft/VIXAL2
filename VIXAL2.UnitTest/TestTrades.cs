using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using VIXAL2.Data.Base;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestTrades
    {
        [TestMethod]
        public void CalculateTrend()
        {
            var tradeSim = new TradesSimulator(5, 10);
            tradeSim.MinTrend = 0.04;

            var trend = tradeSim.GetTrend(10,10.5);
            Assert.AreEqual(trend, 1);

            trend = tradeSim.GetTrend(10.5, 10.1);
            Assert.AreEqual(trend, 0);

            trend = tradeSim.GetTrend(10.6, 10);
            Assert.AreEqual(trend, -1);

            trend = tradeSim.GetTrend(10, 9.65);
            Assert.AreEqual(trend, 0);
        }


        DateTime minDate = Convert.ToDateTime("2022-05-01");

        [TestMethod]
        public void CalculateTrade()
        {
            var originalData = GetSimpleTimeSerieArray();
            var predictedList = GetSimplePredictedList();

            var tradeSim = new TradesSimulator(5, 10);
            tradeSim.MinTrend = 0.04;
            var tradeResult = tradeSim.Trade(originalData, 0, predictedList, 10000, 0.0019);
            Assert.IsTrue(tradeResult.Count > 0);

            Assert.AreEqual(tradeResult[0].PredictedTrend, -1);

            Assert.IsTrue(tradeResult[0].GainPerc > 0.39);
            Assert.IsTrue(tradeResult[0].GainPerc < 0.40);
        }

        [TestMethod]
        public void CalculateTrade_notrade()
        {
            var originalData = GetSimpleTimeSerieArray();
            var predictedList = GetSimplePredictedList();
            predictedList.RemoveRange(9, predictedList.Count-9);

            var tradeSim = new TradesSimulator(5, 10);
            tradeSim.MinTrend = 0.04;
            var tradeResult = tradeSim.Trade(originalData, 0, predictedList, 10000, 0.0019);
            Assert.AreEqual(tradeResult.Count, 0);
        }


        List<DoubleDatedValue> GetSimplePredictedList()
        {
            var myDate = minDate;
            var myPDate = minDate.AddDays(-40);

            var predictedList = new List<DoubleDatedValue>();
            predictedList.Add(new DoubleDatedValue(myPDate, myDate, 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(1), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(2), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(3), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(4), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(5), 0.79));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(6), 0.78));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(7), 0.77));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(8), 0.76));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(9), 0.75));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(10), 0.74));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(11), 0.73));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(12), 0.72));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(13), 0.73));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(14), 0.74));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(15), 0.75));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(16), 0.76));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(17), 0.77));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(18), 0.78));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(19), 0.79));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(20), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(21), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(22), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(23), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(24), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(25), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(26), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(27), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(28), 0.82));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(29), 0.81));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(30), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(31), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(32), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(33), 0.80));
            predictedList.Add(new DoubleDatedValue(myPDate, myDate.AddDays(34), 0.81));

            return predictedList;
        }

        TimeSerieArray GetSimpleTimeSerieArray()
        {
            DateTime minDate = Convert.ToDateTime("2022-05-01");
            string[] stockNames = new string[1];
            stockNames[0] = "MyStock";

            DateTime[] dates = new DateTime[35];
            dates[0] = minDate;
            for (int i = 1; i < dates.Length; i++)
            {
                dates[i] = dates[i - 1].AddDays(1);
            }

            double[][] data = new double[35][];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new double[1];
                data[i][0] = VIXAL2.UnitTest.Data.EnergyData.Eni[i];
            }

            var result = new TimeSerieArray(stockNames, dates, data);
            return result;
        }


    }
 }
