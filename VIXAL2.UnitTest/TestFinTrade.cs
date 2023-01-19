using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using VIXAL2.Data.Base;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestFinTrade
    {
        [TestMethod]
        public void CalculateFinTrades_Long()
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
        public void CalculateFinTrades_ZeroCommission()
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
        public void CalculateFinTrades_Short()
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
    }
}
