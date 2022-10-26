using System;

namespace NeuralNetwork.Base
{
    public class PerformanceDiff: IPerformance
    {
        DateTime _date;
        float _predicted;
        float _real;

        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
            }
        }

        public float Predicted
        {
            get
            {
                return _predicted;
            }
            set
            {
                _predicted = value;
            }
        }

        public float Real
        {
            get
            {
                return _real;
            }
            set
            {
                _real = value;
            }
        }

        public float SuccessPercentage
        {
            get
            {
                float result = 1 - Math.Abs(_predicted - _real) / _real;
                return result;
            }
        }

        public float FailedPercentage
        {
            get
            {
                float result = Math.Abs(_predicted - _real) / _real;
                return result;
            }
        }


        public override string ToString()
        {
            return Date.ToShortDateString() + "; (" + SuccessPercentage.ToString("F2") + ")";
        }
    }
}
