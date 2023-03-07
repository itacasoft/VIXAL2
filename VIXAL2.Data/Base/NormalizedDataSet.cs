using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace VIXAL2.Data.Base
{
    public abstract class NormalizedDataSet
    {
        protected List<double[]> _data;
        protected double[][] _originalData;
        bool normalizeFirst = false;

        public NormalizedDataSet(double[][] data)
        {
            _data = data.ToList();
            _originalData = (double[][])data.Clone();
            string normalizerType = ConfigurationManager.AppSettings["NormalizerType"];
            normalizer = Normalizer.Constructor(normalizerType);
        }

        protected virtual void ReloadFromOriginal()
        {
            _data = ((double[][])_originalData.Clone()).ToList();
        }

        protected List<double[]> Data
        {
            get 
            {
                return _data; 
            }
        }

        public bool NormalizeFirst
        {
            get { return normalizeFirst; }
        }

        protected INormalizer normalizer;

        protected void Prepare()
        {
#if NORMALIZE_FIRST
#else
            normalizer.Initialize(_data);
#endif
        }

        protected void NormalizeAllData()
        {
            normalizer.Initialize(_data);
            normalizer.NormalizeByRef(_data);
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

        public void Normalize(ref TimeSerieArrayExt tsArray, int col)
        {
            for (int i=0; i< tsArray.Values.Length; i++)
            {
                tsArray.Values[i] = Normalize(tsArray.Values[i], col);
            }
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
