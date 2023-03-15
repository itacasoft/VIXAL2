using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using VIXAL2.UnitTest.Data;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class UnitTestBasic
    {
        [TestMethod]
        public void TestOaDates()
        {
            int[] dates = EnergyData.OaDates;

            Assert.AreEqual(dates.Length, EnergyData.SDates.Length);
            Assert.AreEqual(EnergyData.SDates[0], "2020-03-08");
            Assert.AreEqual(EnergyData.SDates[dates.Length - 1], "2022-03-08");
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
        public void Test_DatesToIndexes()
        {
            List<DateTime> dates = new List<DateTime>();
            dates.Add(Convert.ToDateTime("2022-01-04"));
            dates.Add(Convert.ToDateTime("2022-01-05"));
            dates.Add(Convert.ToDateTime("2022-01-06"));
            dates.Add(Convert.ToDateTime("2022-01-07"));
            dates.Add(Convert.ToDateTime("2022-01-08"));
            dates.Add(Convert.ToDateTime("2022-01-11"));
            dates.Add(Convert.ToDateTime("2022-01-12"));
            dates.Add(Convert.ToDateTime("2022-01-13"));
            dates.Add(Convert.ToDateTime("2022-01-16"));
            dates.Add(Convert.ToDateTime("2022-01-17"));

            var result = DatasetFactory.DatesToIndexes(Convert.ToDateTime("2022-01-10"), Convert.ToDateTime("2022-01-13"), dates);
            Assert.AreEqual(result.Item1, 5);
            Assert.AreEqual(result.Item2, 7);

            result = DatasetFactory.DatesToIndexes(Convert.ToDateTime("1900-01-01"), Convert.ToDateTime("2100-01-13"), dates);
            Assert.AreEqual(result.Item1, 0);
            Assert.AreEqual(result.Item2, 9);
        }

        [TestMethod]
        public void Test_LoadCsvAsRawData_NoFilter()
        {
            string[][] stocksData = new string[11][];
            for (int row = 0; row < 11; row++) stocksData[row] = new string[3];

            stocksData[0][0] = "Date";
            stocksData[0][1] = "APPLE";
            stocksData[0][2] = "ENI";

            stocksData[1][0] = "2017.03.14";
            stocksData[2][0] = "2017.03.15";
            stocksData[3][0] = "2017.03.16";
            stocksData[4][0] = "2017.03.17";
            stocksData[5][0] = "2017.03.20";
            stocksData[6][0] = "2017.03.21";
            stocksData[7][0] = "2017.03.22";
            stocksData[8][0] = "2017.03.23";
            stocksData[9][0] = "2017.03.24";
            stocksData[10][0] = "2017.03.27";

            for (int row = 1; row < 11; row++)
            {
                stocksData[row][1] = "2,01";
                stocksData[row][2] = "3,66";
            }

            RawStocksData raw = DatasetFactory.LoadArrayAsRawData(stocksData);
            Assert.AreEqual(raw.stockNames.Count, 2);
            Assert.AreEqual(raw.stockNames[1], "ENI");

            Assert.AreEqual(raw.stockDates.Count, 10);
            Assert.AreEqual(raw.stockDates[0], Convert.ToDateTime("2017.03.14"));
            Assert.AreEqual(raw.stockDates[2], Convert.ToDateTime("2017.03.16"));

            Assert.AreEqual(raw.stocksData.Length, 10);
            Assert.AreEqual(raw.stocksData[2][1], 3.66);
            Assert.AreEqual(raw.stocksData[3][0], 2.01);
        }

        [TestMethod]
        public void Test_LoadCsvAsRawData_Filtered()
        {
            string[][] stocksData = new string[11][];
            for (int row = 0; row < 11; row++) stocksData[row] = new string[3];

            stocksData[0][0] = "Date";
            stocksData[0][1] = "APPLE";
            stocksData[0][2] = "ENI";

            stocksData[1][0] = "2017.03.14";
            stocksData[2][0] = "2017.03.15";
            stocksData[3][0] = "2017.03.16";
            stocksData[4][0] = "2017.03.17";
            stocksData[5][0] = "2017.03.20";
            stocksData[6][0] = "2017.03.21";
            stocksData[7][0] = "2017.03.22";
            stocksData[8][0] = "2017.03.23";
            stocksData[9][0] = "2017.03.24";
            stocksData[10][0] = "2017.03.27";

            for (int row = 1; row < 11; row++)
            {
                stocksData[row][1] = "2,01";
                stocksData[row][2] = "3,66";
            }

            RawStocksData raw = DatasetFactory.LoadArrayAsRawData(stocksData, "2017.03.14", "2017.03.16");

            Assert.AreEqual(raw.stockDates.Count, 3);
            Assert.AreEqual(raw.stockDates[0], Convert.ToDateTime("2017.03.14"));
            Assert.AreEqual(raw.stockDates[2], Convert.ToDateTime("2017.03.16"));
            Assert.AreEqual(raw.stocksData.Length, 3);

            raw = DatasetFactory.LoadArrayAsRawData(stocksData, "2017.03.15", "2017.03.18");

            Assert.AreEqual(raw.stockDates.Count, 3);
            Assert.AreEqual(raw.stockDates[0], Convert.ToDateTime("2017.03.15"));
            Assert.AreEqual(raw.stockDates[2], Convert.ToDateTime("2017.03.17"));
            Assert.AreEqual(raw.stocksData.Length, 3);


            raw = DatasetFactory.LoadArrayAsRawData(stocksData, "2017.03.25");

            Assert.AreEqual(raw.stockDates.Count, 1);
            Assert.AreEqual(raw.stockDates[0], Convert.ToDateTime("2017.03.27"));
            Assert.AreEqual(raw.stocksData.Length, 1);
        }

    }
}

