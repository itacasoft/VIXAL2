using System;
using System.Linq;
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

        public static double[] TakeValues(IEnumerable<DatedValue> list)
        {
            List<double> resultX = new List<double>();

            foreach (DatedValue d in list)
            {
                resultX.Add(d.Value);
            }

            return resultX.ToArray();
        }

        public static IEnumerable<DatedValue> SubstituteValues(IEnumerable<DatedValue> list, IEnumerable<double> values)
        {
            List<DatedValue> result = new List<DatedValue>();

            var d = list.ToArray();
            var x = values.ToArray();

            for(int i=0; i<d.Length; i++)
            {
                result.Add(new DatedValue(d[i].Date, x[i])); 
            }

            return result;
        }
    }
}
