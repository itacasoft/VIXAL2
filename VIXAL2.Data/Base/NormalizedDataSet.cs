using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace VIXAL2.Data.Base
{
    public abstract class NormalizedDataSet
    {
        private double[][] _data;
        bool normalizeFirst = false;

        protected double[][] Data
        {
            get 
            {
                return _data; 
            }
            set 
            {
                _data = value;
                dataList = _data.ToList<double[]>();
            }
        }

        public bool NormalizeFirst
        {
            get { return normalizeFirst; }
        }

        protected List<double[]> dataList;
        protected INormalizer normalizer;

        public NormalizedDataSet(double[][] data)
        {
            Data = data;
            string normalizerType = ConfigurationManager.AppSettings["NormalizerType"];
            normalizer = Normalizer.Constructor(normalizerType);
        }

        public virtual void Prepare()
        {
#if NORMALIZE_FIRST
#else
            normalizer.Initialize(dataList);
#endif
        }

        protected void NormalizeAllData()
        {
            normalizer.Initialize(dataList);
            normalizer.NormalizeByRef(dataList);
            normalizeFirst = true;
        }

        public double Normalize(double value, int col)
        {
            return normalizer.Normalize(value, col);
        }

        public double[] Normalize(double[] values, int col)
        {
            return normalizer.Normalize(values, col);
        }

        public double[][] Normalize(double[][] values)
        {
            return normalizer.Normalize(values);
        }

        public double[][] Normalize(double[][] values, int col)
        {
            if ((values.Length > 0) && (values[0].Length > 1))
                throw new InvalidOperationException("This Normalize function can be used only with an array with 1 column and N rows");

            double[][] result = new double[values.Length][];
            for (int row = 0; row < values.Length; row++)
            {
                result[row] = new double[1];
                result[row][0] = Normalize(values[row][0], col);    
            }

            return result;
        }


        public double Decode(double value, int col)
        {
            return normalizer.Decode(value, col);
        }

        public double[] Decode(double[] values, int col)
        {
            return normalizer.Decode(values, col);
        }

        public List<double> Decode(List<double> values, int col)
        {
            return normalizer.Decode(values, col);
        }

        public double[][] Decode(double[][] values)
        {
            return normalizer.Decode(values);
        }
    }
}
