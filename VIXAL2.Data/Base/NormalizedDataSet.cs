using SharpML.Types;
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

        protected List<double[]> dataList;
        protected INormalizer normalizer;

        public NormalizedDataSet(double[][] data)
        {
            Data = data;
            string normalizerType = ConfigurationManager.AppSettings["NormalizerType"];
            normalizer = Normalizer.Constructor(normalizerType);
        }

        public abstract void Prepare();

        protected void NormalizeAllData()
        {
            normalizer.Initialize(dataList);
            normalizer.NormalizeByRef(dataList);
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


        public double Decode(double value, int col)
        {
            return normalizer.Decode(value, col);
        }

        public double[] Decode(double[] values, int col)
        {
            return normalizer.Decode(values, col);
        }

        public double[][] Decode(double[][] values)
        {
            return normalizer.Decode(values);
        }
    }
}
