using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Base
{
    public class TimeSerieArrayExt : TimeSerieArray
    {
        private DateTime[] futureDates;

        internal DateTime[] FutureDates
        {
            get
            {
                return futureDates;
            }
        }

        public TimeSerieArrayExt(int rows, int cols) : base(rows, cols)
        {
            this.futureDates = new DateTime[rows];
        }

        public TimeSerieArrayExt(string[] stockNames, DateTime[] dates, double[][] allData) : base(stockNames, dates, allData)
        {
            int rows = allData.Length;
            this.futureDates = new DateTime[rows];
        }

        public void SetValue(int row, int col, DateTime date, DateTime futureDate, double value)
        {
            base.SetValue(row, col, date, value);
            futureDates[row] = futureDate;
        }

        public DateTime GetFutureDate(int row)
        {
            return futureDates[row];
        }

        public int PredictDays
        {
            get;set;
        }
        public int Range
        {
            get; set;
        }

        public string ToStringExt()
        {
            return GetColName(0) + ", PD:" + this.PredictDays;
        }
    }
}
