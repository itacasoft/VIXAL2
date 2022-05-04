using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class MovingForwardDataSet : StocksDataset, IAverageRangeDataSet
    {
        protected int range = 20;

        public MovingForwardDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public int Range
        {
            get { return range; }
            set { range = value; }
        }
    }
}
