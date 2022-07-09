using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class DatedValueF
    {
        DateTime _date;
        float _value;

        public DatedValueF(DateTime date, float value)
        {
            _date = date;
            _value = value;
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public float Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return _date.ToShortDateString() + " " + _value.ToString("F");
        }
    }
}
