using VIXAL2.Retrieve.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpML.Types;

namespace VIXAL2.Retrieve
{
    internal class ParserManager : BaseManager
    {
        public ParserManager(string dataFolder, DateTime dateTo) : base(dataFolder)
        {
            this.dateTo = dateTo;
        }

        private DateTime dateTo;

        public StockList Stocks { get; set; }

        public override async Task Run()
        {
            Yahoo.DataParser parser = new Yahoo.DataParser(dataFolder, dateTo);
            Stocks = parser.Parse();

            RemoveWeekEnds(Stocks);
            Stocks.AlignDates();
        }

        private void RemoveWeekEnds(StockList stocks)
        {
            foreach(Stock stock in stocks)
            {
                //collect days to remove
                List<DateTime> daysToRemove = new List<DateTime>();

                foreach (KeyValuePair<DateTime, double> price in stock.Prices)
                {
                    //remove sat and sun
                    if ((price.Key.DayOfWeek == DayOfWeek.Sunday) || (price.Key.DayOfWeek == DayOfWeek.Saturday))
                    {
                        daysToRemove.Add(price.Key);
                    }
                    else //remove new eve year
                    if((price.Key.Month == 1) && (price.Key.Day == 1))
                    {
                        daysToRemove.Add(price.Key);
                    }
                }
                //remove them
                foreach (DateTime date in daysToRemove)
                {
                    stock.Prices.Remove(date);
                }
            }
        }
    }
}
