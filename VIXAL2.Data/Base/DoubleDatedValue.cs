using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Base
{
    public class DoubleDatedValue : DatedValue
    {
        DateTime _predictionDate;

        public DoubleDatedValue(DateTime predictionDate, DateTime date, double value) : base(date, value)
        {
            _predictionDate = predictionDate;
        }

        public DateTime PredictionDate
        {
            get { return _predictionDate; }
        }
    }
}
