using System;
using System.Collections.Generic;

namespace VIXAL2.Data.Base
{
    public class RawStocksData
    {
        public List<string> stockNames;
        public List<DateTime> stockDates;
        public double[][] stocksData;

        public RawStocksData(int length)
        {
            stockNames = new List<string>();
            stockDates = new List<DateTime>();
            stocksData = new double[length][];
        }

        public int StocksCount
        {
            get
            {
                return stockNames.Count;
            }
        }
    }
}
