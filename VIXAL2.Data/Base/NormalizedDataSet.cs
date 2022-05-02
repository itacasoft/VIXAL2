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
    }
}
