using System;
using System.Collections.Generic;

namespace VIXAL2.Data.Base
{
    public class DatedValue
    {
        DateTime _date;
        double _value;

        public DatedValue(DateTime date, double value)
        {
            _date = date;
            _value = value;
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public double Value { 
            get { return _value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            return _date.ToShortDateString() + " " + _value.ToString("F");
        }

        public static double[] ToDoubleArray(List<DoubleDatedValue> input)
        {
            var result = new double[input.Count];
            for (int i = 0; i < input.Count; i++)
            {
                result[i] = input[i].Value;
            }
            return result;
        }
    }
}
