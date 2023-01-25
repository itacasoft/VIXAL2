using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using VIXAL2.Data.Base;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestFinTrades
    {
        [TestMethod]
        public void CalculateFinTrades_Long_Test()
        {
            var trade = new FinTrade(DateTime.Now, 10, 2000, TradingPosition.Long);
            Assert.AreEqual(trade.StartMoney, 1996.2);

            trade.Close(DateTime.Now.AddDays(1), 10.2);
            Assert.AreEqual(trade.Gain, 36.06);
            Assert.AreEqual(trade.EndMoney, 2032.26);
            Assert.AreEqual(trade.GainPerc, 0.0181);

            trade = new FinTrade(DateTime.Now, 10, 1000, TradingPosition.Long);
            Assert.AreEqual(trade.StartMoney, 997.05);

            trade.Close(DateTime.Now.AddDays(1), 10.2);
            Assert.AreEqual(trade.Gain, 16.99);
            Assert.AreEqual(trade.EndMoney, 1014.04);
            Assert.AreEqual(trade.GainPerc, 0.017);

            trade = new FinTrade(DateTime.Now, 10, 10000, TradingPosition.Long);
            Assert.AreEqual(trade.StartMoney, 9981.00);

            trade.Close(DateTime.Now.AddDays(1), 10.2);
            Assert.AreEqual(trade.Gain, 180.62);
            Assert.AreEqual(trade.EndMoney, 10161.62);
            Assert.AreEqual(trade.GainPerc, 0.0181);
        }

        [TestMethod]
        public void CalculateFinTrades_ZeroCommission_Test()
        {
            var trade = new FinTrade(DateTime.Now, 10, 2000, TradingPosition.Long, false);
            Assert.AreEqual(trade.StartMoney, 2000);

            trade.Close(DateTime.Now.AddDays(1), 10.2);
            Assert.AreEqual(trade.Gain, 40);
            Assert.AreEqual(trade.EndMoney, 2040);
            Assert.AreEqual(trade.GainPerc, 0.02);

            trade = new FinTrade(DateTime.Now, 10, 2000, TradingPosition.Short, false);
            Assert.AreEqual(trade.StartMoney, 2000);

            trade.Close(DateTime.Now.AddDays(1), 9.8);
            Assert.AreEqual(trade.Gain, 40);
            Assert.AreEqual(trade.EndMoney, 1960);
            Assert.AreEqual(trade.GainPerc, 0.02);
        }


        [TestMethod]
        public void CalculateFinTrades_Short_Test()
        {
            var trade = new FinTrade(DateTime.Now, 10.2, 2000, TradingPosition.Short);
            Assert.AreEqual(trade.StartMoney, 1996.2);

            trade.Close(DateTime.Now.AddDays(1), 10);
            Assert.AreEqual(trade.Gain, 42.86);
            Assert.AreEqual(trade.EndMoney, 1953.34);
            Assert.AreEqual(trade.GainPerc, 0.0219);

            trade = new FinTrade(DateTime.Now, 10, 1000, TradingPosition.Short);
            Assert.AreEqual(trade.StartMoney, 997.05);

            trade.Close(DateTime.Now.AddDays(1), 10.2);
            Assert.AreEqual(trade.Gain, -16.99);
            Assert.AreEqual(trade.EndMoney, 1014.04);
            Assert.AreEqual(trade.GainPerc, -0.0168);
        }

        private PredictedData GetSimplePredictedData()
        {
            List<DatedValue> originalData = new List<DatedValue>();
            originalData.Add(new DatedValue(DateTime.Now, 10.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(1), 11.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(2), 12.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(3), 11.0));

            PredictedData p = new PredictedData(originalData);

            List<DoubleDatedValue> predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-40), DateTime.Now, 10.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(1), 11.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(2), 12.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(3), 13.5));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(1), 11.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(2), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(3), 13.5));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(2), 12.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(3), 11.5));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(3), 13.5));
            p.AddPredictedCurve(predictedData);

            return p;
        }

        private PredictedData GetComplexPredictedData()
        {
            List<DatedValue> originalData = new List<DatedValue>();
            originalData.Add(new DatedValue(DateTime.Now, 10.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(1), 10.4));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(2), 10.3));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(3), 10.7));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(4), 11.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(5), 11.1));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(6), 11.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(7), 10.8));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(8), 10.7));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(9), 10.7));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(10), 10.8));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(11), 11.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(12), 11.3));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(13), 11.0));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(14), 10.5));
            originalData.Add(new DatedValue(DateTime.Now.AddDays(15), 10.0));

            PredictedData p = new PredictedData(originalData);

            List<DoubleDatedValue> predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-40), DateTime.Now, 10.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(1), 10.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(2), 10.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(3), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(4), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(1), 10.5));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(2), 10.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(3), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(4), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(2), 10.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(3), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(4), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-39), DateTime.Now.AddDays(3), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(4), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-38), DateTime.Now.AddDays(4), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);


            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-37), DateTime.Now.AddDays(5), 11.2));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-36), DateTime.Now.AddDays(6), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-35), DateTime.Now.AddDays(7), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-34), DateTime.Now.AddDays(8), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-33), DateTime.Now.AddDays(9), 10.8));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-32), DateTime.Now.AddDays(10), 10.9));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-31), DateTime.Now.AddDays(11), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(12), 11.4));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-29), DateTime.Now.AddDays(13), 11.1));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-28), DateTime.Now.AddDays(14), 10.6));
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);


            predictedData = new List<DoubleDatedValue>();
            predictedData.Add(new DoubleDatedValue(DateTime.Now.AddDays(-27), DateTime.Now.AddDays(15), 10.1));
            p.AddPredictedCurve(predictedData);

            return p;

        }

        [TestMethod]
        public void FinTradeSimulator_CalculateTrend_Test1()
        {
            var p = GetSimplePredictedData();

            var tradeSim = new FinTradeSimulator(p);
            tradeSim.MinTrend = 0.02;
            var t0 = tradeSim.GetPredictedTrend(0);
            Assert.AreEqual(t0, Trend.Up);

            var t1 = tradeSim.GetPredictedTrend(1);
            Assert.AreEqual(t1, Trend.None);

            var t2 = tradeSim.GetPredictedTrend(2);
            Assert.AreEqual(t2, Trend.Down);

            var t3 = tradeSim.GetPredictedTrend(3);
            Assert.AreEqual(t3, Trend.EOF);

            tradeSim = new FinTradeSimulator(p);
            tradeSim.MinTrend = 0;
            t1 = tradeSim.GetPredictedTrend(1);
            Assert.AreEqual(t1, Trend.Down);

            var money1 = tradeSim.Trade(10000);
            Assert.IsTrue(money1 > 10000);
            Assert.AreEqual(tradeSim.TradesCount, 1);
            Assert.AreEqual(tradeSim.TradesGainCount, 1);
            Assert.AreEqual(tradeSim.TradesLossCount, 0);
        }

        [TestMethod]
        public void FinTradeSimulator_CalculateTrend_Test2()
        {
            var p = GetComplexPredictedData();

            var tradeSim = new FinTradeSimulator(p, false);
            tradeSim.MinTrend = 0.02;

            var money1 = tradeSim.Trade(10000);
            Assert.IsTrue(money1 > 10000);
            Assert.AreEqual(tradeSim.TradesCount, 1);
            Assert.AreEqual(tradeSim.TradesGainCount, 1);
            Assert.AreEqual(tradeSim.TradesLossCount, 0);
        }

        [TestMethod]
        public void FinTradeSimulator_CalculateTrend_Test3_Mintrend_zero()
        {
            var p = GetComplexPredictedData();

            var tradeSim = new FinTradeSimulator(p, false);
            tradeSim.MinTrend = 0;

            var money1 = tradeSim.Trade(10000);
            Assert.IsTrue(money1 > 10000);
            Assert.AreEqual(tradeSim.TradesCount, 1);
            Assert.AreEqual(tradeSim.TradesGainCount, 1);
            Assert.AreEqual(tradeSim.TradesLossCount, 0);
        }

    }
}
